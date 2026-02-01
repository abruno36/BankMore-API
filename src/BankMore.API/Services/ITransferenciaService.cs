using BankMore.Infrastructure.Services;
using BankMore.Shared.Models;

namespace BankMore.API.Services
{
    public interface ITransferenciaService
    {
        Task<TransferenciaResult> ProcessarTransferenciaAsync(
            TransferenciaRequest request,
            string? idempotencyKey);
    }
}