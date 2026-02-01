using BankMore.API.Domain.Repositories;
using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankMore.API.Infrastructure.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly BankMoreDbContext _context;

        public ContaRepository(BankMoreDbContext context)
        {
            _context = context;
        }

        public async Task<ContaCorrente?> ObterPorNumeroAsync(string numeroConta)
        {
            return await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == numeroConta);
        }

        public async Task<ContaCorrente?> ObterPorCPFAsync(string cpf)
        {
            return await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.CPFCriptografado == cpf);
        }

        public async Task AdicionarAsync(ContaCorrente conta)
        {
            await _context.ContasCorrentes.AddAsync(conta);
        }

        public async Task<bool> ExisteCPFAsync(string cpf)
        {
            return await _context.ContasCorrentes
                .AnyAsync(c => c.CPFCriptografado == cpf);
        }

        public async Task<int> SalvarAlteracoesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}