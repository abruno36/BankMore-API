using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using BankMore.Infrastructure.Services;
using BankMore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BankMore.API.Services
{
    public class TransferenciaService : ITransferenciaService
    {
        private readonly BankMoreDbContext _context;
        private readonly ILogger<TransferenciaService> _logger;

        public TransferenciaService(
            BankMoreDbContext context,
            ILogger<TransferenciaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TransferenciaResult> ProcessarTransferenciaAsync(
            TransferenciaRequest request,
            string? idempotencyKey)
        {
            try
            {
                if (!string.IsNullOrEmpty(idempotencyKey))
                {
                    var transferenciaExistente = await _context.Transferencias
                        .FirstOrDefaultAsync(t => t.IdRequisicao == idempotencyKey);

                    if (transferenciaExistente != null)
                    {
                        return new TransferenciaResult
                        {
                            Success = true,
                            Mensagem = "Transferência já processada anteriormente",
                            IdTransacao = transferenciaExistente.Id,
                            DataHora = transferenciaExistente.DataTransferencia
                        };
                    }
                }

                var transferencia = new Transferencia
                {
                    ContaOrigemId = Guid.Parse("A639DC66-871E-44D3-B9EA-9A45BB57EB74"),
                    ContaDestinoId = Guid.Parse("D2F59F23-01D7-4486-8311-1D002C5B29C5"),
                    Valor = request.Valor,
                    Status = "CONCLUIDA",
                    IdRequisicao = idempotencyKey ?? Guid.NewGuid().ToString()
                };

                _context.Transferencias.Add(transferencia);
                await _context.SaveChangesAsync();

                return new TransferenciaResult
                {
                    Success = true,
                    Mensagem = $"Transferência de R$ {request.Valor:F2} realizada com sucesso",
                    IdTransacao = transferencia.Id,
                    DataHora = transferencia.DataTransferencia
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar transferência");
                return new TransferenciaResult
                {
                    Success = false,
                    Mensagem = $"Erro: {ex.Message}",
                    DataHora = DateTime.UtcNow
                };
            }
        }
    }
}