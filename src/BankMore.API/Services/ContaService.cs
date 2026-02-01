using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using BankMore.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankMore.API.Services
{
    public class ContaService : IContaService
    {
        private readonly BankMoreDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICryptoService _cryptoService;

        public ContaService(
            BankMoreDbContext context,
            IPasswordHasher passwordHasher,
            ICryptoService cryptoService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _cryptoService = cryptoService;
        }

        public async Task<string> CadastrarContaAsync(string cpf, string senha, string nomeTitular)
        {
            var cpfLimpo = new string(cpf.Where(char.IsDigit).ToArray());
            if (cpfLimpo.Length != 11)
                throw new Exception("CPF inválido");

            var cpfHash = _cryptoService.Hash(cpfLimpo);
            var cpfExistente = await _context.ContasCorrentes
                .AnyAsync(c => c.CPFHash == cpfHash);

            if (cpfExistente)
                throw new Exception($"CPF já cadastrado. Hash: {cpfHash}");

            var numeroConta = await GerarNumeroContaUnico();
            var cpfCriptografado = _cryptoService.Criptografar(cpfLimpo);
            var email = $"{cpfLimpo.Substring(0, 8)}@email.com";

            var conta = new ContaCorrente
            {
                CPFCriptografado = cpfCriptografado,
                CPFHash = cpfHash,
                NomeTitular = nomeTitular,
                Email = email,
                SenhaHash = _passwordHasher.HashPassword(senha),
                NumeroConta = numeroConta,
                Ativa = true,
                DataCriacao = DateTime.UtcNow
            };

            _context.ContasCorrentes.Add(conta);
            await _context.SaveChangesAsync();

            return conta.NumeroConta;
        }

        public async Task<decimal> ConsultarSaldoAsync(string contaId)
        {
            return 0m;
        }

        public async Task<bool> UsuarioTemAcessoContaAsync(string userId, string contaId)
        {
            return true;
        }

        public async Task<bool> InativarContaAsync(string contaId, string senha)
        {
            var conta = await ObterContaPorIdentificador(contaId);
            if (conta == null)
                throw new Exception("Conta não encontrada");

            if (!_passwordHasher.VerifyPassword(senha, conta.SenhaHash))
                throw new UnauthorizedAccessException("Senha inválida");

            conta.Ativa = false;
            conta.DataInativacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ContaInfo?> ObterContaPorIdAsync(string contaId)
        {
            var conta = await ObterContaPorIdentificador(contaId);
            if (conta == null)
                return null;

            return new ContaInfo
            {
                Id = conta.Id.ToString(),
                NumeroConta = conta.NumeroConta,
                NomeTitular = conta.NomeTitular,
                Cpf = "",
                Ativo = conta.Ativa,
                DataCriacao = conta.DataCriacao
            };
        }

        public async Task<ContaInfo?> ObterContaPorCpfAsync(string cpf)
        {
            var cpfHash = _cryptoService.Hash(cpf);
            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.CPFHash == cpfHash);

            if (conta == null)
                return null;

            return new ContaInfo
            {
                Id = conta.Id.ToString(),
                NumeroConta = conta.NumeroConta,
                NomeTitular = conta.NomeTitular,
                Cpf = cpf,
                Ativo = conta.Ativa,
                DataCriacao = conta.DataCriacao
            };
        }

        public async Task<ContaInfo?> ObterContaPorNumeroAsync(string numeroConta)
        {
            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == numeroConta);

            if (conta == null)
                return null;

            return new ContaInfo
            {
                Id = conta.Id.ToString(),
                NumeroConta = conta.NumeroConta,
                NomeTitular = conta.NomeTitular,
                Cpf = "",
                Ativo = conta.Ativa,
                DataCriacao = conta.DataCriacao
            };
        }

        private async Task<ContaCorrente?> ObterContaPorIdentificador(string identificador)
        {
            if (Guid.TryParse(identificador, out Guid idGuid))
            {
                return await _context.ContasCorrentes
                    .FirstOrDefaultAsync(c => c.Id == idGuid);
            }

            var cpfHash = _cryptoService.Hash(identificador);
            var contaPorCPF = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.CPFHash == cpfHash);

            if (contaPorCPF != null)
                return contaPorCPF;

            return await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == identificador);
        }

        private async Task<string> GerarNumeroContaUnico()
        {
            string numeroConta;
            bool existe;

            do
            {
                numeroConta = new Random().Next(100000, 999999).ToString("000000");
                existe = await _context.ContasCorrentes
                    .AnyAsync(c => c.NumeroConta == numeroConta);
            } while (existe);

            return numeroConta;
        }
    }
}