using Microsoft.EntityFrameworkCore;
using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;

namespace BankMore.Infrastructure.Repositories
{
    public interface ITransferenciaRepository
    {
        Task<Transferencia> RegistrarTransferenciaAsync(Transferencia transferencia);
        Task<Transferencia?> ObterPorIdRequisicaoAsync(string idRequisicao);
    }

    public class TransferenciaRepository : ITransferenciaRepository
    {
        private readonly BankMoreDbContext _context;

        public TransferenciaRepository(BankMoreDbContext context)
        {
            _context = context;
        }

        public async Task<Transferencia> RegistrarTransferenciaAsync(Transferencia transferencia)
        {
            _context.Transferencias.Add(transferencia);
            await _context.SaveChangesAsync();
            return transferencia;
        }

        public async Task<Transferencia?> ObterPorIdRequisicaoAsync(string idRequisicao)
        {
            return await _context.Transferencias
                .FirstOrDefaultAsync(t => t.IdRequisicao == idRequisicao);
        }
    }
}