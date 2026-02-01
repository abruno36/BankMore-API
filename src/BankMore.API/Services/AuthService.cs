using BankMore.API.Models.DTOs;
using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankMore.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly BankMoreDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ICryptoService _cryptoService;

        public AuthService(
            BankMoreDbContext context,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            ICryptoService cryptoService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _cryptoService = cryptoService;
        }

        public async Task<ContaCorrente> CadastrarUsuario(CadastroRequest request)
        {
            var cpfLimpo = new string(request.CPF.Where(char.IsDigit).ToArray());
            if (cpfLimpo.Length != 11)
                throw new Exception("CPF inválido");

            var cpfHash = _cryptoService.Hash(cpfLimpo);

            if (await _context.ContasCorrentes.AnyAsync(c => c.CPFHash == cpfHash))
                throw new Exception("CPF já cadastrado");

            var emailExistente = await _context.ContasCorrentes
                .AnyAsync(c => c.Email == request.Email);

            if (emailExistente)
                throw new Exception("Email já cadastrado");

            var cpfCriptografado = _cryptoService.Criptografar(cpfLimpo);

            var conta = new ContaCorrente
            {
                CPFCriptografado = cpfCriptografado,
                CPFHash = cpfHash,
                NomeTitular = request.NomeCompleto,
                Email = request.Email,
                SenhaHash = _passwordHasher.HashPassword(request.Senha),
                NumeroConta = await GerarNumeroContaUnico(),
                Ativa = true,
                DataCriacao = DateTime.UtcNow
            };

            _context.ContasCorrentes.Add(conta);
            await _context.SaveChangesAsync();

            return conta;
        }

        private async Task<string> GerarNumeroContaUnico()
        {
            string numeroConta;
            bool existe;

            var random = new Random();

            do
            {
                numeroConta = random.Next(100000, 999999).ToString("000000");
                existe = await _context.ContasCorrentes
                    .AnyAsync(c => c.NumeroConta == numeroConta);
            } while (existe);

            return numeroConta;
        }

        public async Task<string> Autenticar(LoginRequest request)
        {
            ContaCorrente? conta = null;

            var cpfHash = _cryptoService.Hash(request.Identificador);
            conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.CPFHash == cpfHash);

            if (conta == null)
            {
                conta = await _context.ContasCorrentes
                    .FirstOrDefaultAsync(c => c.NumeroConta == request.Identificador);
            }

            if (conta == null || !conta.Ativa)
                throw new UnauthorizedAccessException("Conta não encontrada ou inativa");

            if (!_passwordHasher.VerifyPassword(request.Senha, conta.SenhaHash))
                throw new UnauthorizedAccessException("Senha inválida");

            return GenerateToken(conta);
        }

        public async Task<bool> ValidarCredenciais(string identificador, string senha)
        {
            ContaCorrente? conta = null;

            var cpfHash = _cryptoService.Hash(identificador);
            conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.CPFHash == cpfHash);

            if (conta == null)
            {
                conta = await _context.ContasCorrentes
                    .FirstOrDefaultAsync(c => c.NumeroConta == identificador);
            }

            if (conta == null || !conta.Ativa)
                return false;

            return _passwordHasher.VerifyPassword(senha, conta.SenhaHash);
        }

        private string GenerateToken(ContaCorrente conta)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("contaId", conta.NumeroConta),
                    new Claim("cpf", conta.CPFCriptografado),
                    new Claim(ClaimTypes.Name, conta.NomeTitular)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool ValidarCPF(string cpf)
        {
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            return cpf.Length == 11;
        }

        private string GerarNumeroConta()
        {
            return new Random().Next(100000, 999999).ToString("000000");
        }
    }
}