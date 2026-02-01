using BankMore.Infrastructure.Services;
using BankMore.Shared.Models;

namespace BankMore.API.Repositories
{
    public interface ITransferenciaRepository
    {
        Task<TransferenciaResult> RegistrarTransferenciaAsync(
            TransferenciaRequest request,
            string? idempotencyKey);
    }
}
