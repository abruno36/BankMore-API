using BankMore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankMore.API.Services
{
    public class SaldoService : ISaldoService
    {
        private readonly BankMoreDbContext _context;

        public SaldoService(BankMoreDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalcularSaldoAsync(string numeroConta)
        {
            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == numeroConta);

            if (conta == null && Guid.TryParse(numeroConta, out var guid))
            {
                conta = await _context.ContasCorrentes
                    .FirstOrDefaultAsync(c => c.Id == guid);
            }

            if (conta == null) return 0;

            var movimentos = await _context.Movimentos
                .Where(m => m.ContaCorrenteId.ToUpper() == conta.Id.ToString().ToUpper())
                .ToListAsync();

            var creditos = movimentos
                .Where(m => m.Tipo == "C")
                .Sum(m => m.Valor);

            var debitos = movimentos
                .Where(m => m.Tipo == "D")
                .Sum(m => m.Valor);

            return creditos - debitos;
        }

        public async Task<SaldoResponse> ObterSaldoCompleto(string numeroConta) 
        {
            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == numeroConta);

            if (conta == null)
                throw new Exception("Conta não encontrada");

            if (!conta.Ativa)
                throw new Exception("Conta inativa");

            var saldo = await CalcularSaldoAsync(numeroConta);

            return new SaldoResponse
            {
                NumeroConta = conta.NumeroConta,
                NomeTitular = conta.NomeTitular,
                DataHoraConsulta = DateTime.UtcNow,
                Saldo = saldo  
            };
        }
    }
}
