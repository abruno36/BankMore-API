using BankMore.API.Exceptions;
using BankMore.API.Models.DTOs;
using BankMore.Domain.Entities;
using BankMore.Domain.Exceptions;
using BankMore.Infrastructure.Data;
using BankMore.Shared.Interfaces;
using BankMore.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankMore.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly BankMoreDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ICryptoService _cryptoService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            BankMoreDbContext context,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            ICryptoService cryptoService,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _cryptoService = cryptoService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<ContaCorrente> CadastrarUsuario(Shared.Models.CadastroRequest request)
        {
            var cpfLimpo = new string(request.Cpf.Where(char.IsDigit).ToArray());

            if (cpfLimpo.Length != 11)
                throw new DomainException("CPF inválido", "INVALID_DOCUMENT");

            var cpfHash = _cryptoService.Hash(cpfLimpo);
            var cpfExistente = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.CPFHash == cpfHash);

            if (cpfExistente != null)
                throw new DomainException("CPF já cadastrado", "DUPLICATE_DOCUMENT");

            var senhaHash = _passwordHasher.HashPassword(request.Senha);
            var cpfCriptografado = _cryptoService.Criptografar(cpfLimpo);

            var conta = new ContaCorrente
            {
                NumeroConta = await GerarNumeroContaUnico(),
                CPFCriptografado = cpfCriptografado,
                CPFHash = cpfHash,
                SenhaHash = senhaHash,
                NomeTitular = request.NomeTitular,
                Email = "",
                Ativa = true,
                DataCriacao = DateTime.UtcNow
            };

            _context.ContasCorrentes.Add(conta);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Conta cadastrada: {NumeroConta}", conta.NumeroConta);

            return conta;
        }

        public async Task<string> Autenticar(Shared.Models.LoginRequest request)
        {
            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == request.Identificador);

            if (conta == null)
                throw new ContaNaoEncontradaException();

            if (!conta.Ativa)
                throw new ContaInativaException();

            if (!_passwordHasher.VerifyPassword(request.Senha, conta.SenhaHash))
                throw new SenhaIncorretaException();

            _logger.LogInformation("Login bem-sucedido: {NumeroConta}", conta.NumeroConta);

            return _tokenService.GenerateToken(conta.Id.ToString(), conta.Email ?? "");
        }

        public async Task<bool> ValidarCredenciais(string identificador, string senha)
        {
            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == identificador);

            if (conta == null || !conta.Ativa)
                return false;

            return _passwordHasher.VerifyPassword(senha, conta.SenhaHash);
        }

        private async Task<string> GerarNumeroContaUnico()
        {
            var random = new Random();
            string numeroConta;

            do
            {
                numeroConta = random.Next(100000, 999999).ToString("000000");
            } while (await _context.ContasCorrentes.AnyAsync(c => c.NumeroConta == numeroConta));

            return numeroConta;
        }
    }
}