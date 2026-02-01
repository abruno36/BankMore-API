using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankMore.Infrastructure.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly BankMoreDbContext _context;
        private readonly ILogger<IdempotencyService> _logger;

        public IdempotencyService(BankMoreDbContext context, ILogger<IdempotencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsRequestProcessed(string idempotencyKey)
        {
            return await _context.IdempotencyKeys
                .AnyAsync(k => k.Id == idempotencyKey && k.Status == "PROCESSED");
        }

        public async Task StoreProcessedRequest(string idempotencyKey, string requestType,
            string? contaOrigem, string? contaDestino, decimal? valor, string responseData)
        {
            await SaveIdempotencyKeyAsync(idempotencyKey, requestType, contaOrigem,
                contaDestino, valor, "PROCESSED", responseData);
        }

        public async Task<string?> GetCachedResponse(string idempotencyKey)
        {
            var key = await _context.IdempotencyKeys
                .FirstOrDefaultAsync(k => k.Id == idempotencyKey && k.Status == "PROCESSED");

            return key?.ResponseData;
        }

        public async Task SaveIdempotencyKeyAsync(string idempotencyKey, string requestType,
            string? contaOrigem, string? contaDestino, decimal? valor,
            string status, string responseData)
        {
            var existing = await _context.IdempotencyKeys
                .FirstOrDefaultAsync(k => k.Id == idempotencyKey);

            if (existing != null)
            {
                existing.RequestType = requestType;
                existing.ContaOrigem = contaOrigem;
                existing.ContaDestino = contaDestino;
                existing.Valor = valor;
                existing.Status = status;
                existing.ResponseData = responseData;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                // CRIA nova apenas se não existir
                var key = new IdempotencyKey
                {
                    Id = idempotencyKey,
                    RequestType = requestType,
                    ContaOrigem = contaOrigem,
                    ContaDestino = contaDestino,
                    Valor = valor,
                    Status = status,
                    ResponseData = responseData,
                    CreatedAt = DateTime.UtcNow
                };

                _context.IdempotencyKeys.Add(key);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Chave idempotente salva: {idempotencyKey} - Status: {status}");
        }
    }
}