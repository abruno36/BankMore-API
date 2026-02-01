namespace BankMore.Infrastructure.Services
{
    public interface IIdempotencyService
    {
        Task<bool> IsRequestProcessed(string idempotencyKey);
        Task StoreProcessedRequest(string idempotencyKey, string requestType,
            string? contaOrigem, string? contaDestino, decimal? valor,
            string responseData);
        Task<string?> GetCachedResponse(string idempotencyKey);

        Task SaveIdempotencyKeyAsync(string idempotencyKey, string requestType,
            string? contaOrigem, string? contaDestino, decimal? valor,
            string status, string responseData);
    }
}
