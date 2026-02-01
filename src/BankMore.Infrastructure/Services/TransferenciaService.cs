using BankMore.Infrastructure.Data;
using BankMore.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankMore.Infrastructure.Services
{
    public class TransferenciaService : ITransferenciaService
    {
        private readonly BankMoreDbContext _context;
        private readonly IHttpContaCorrenteService _contaCorrenteService;
        private readonly ILogger<TransferenciaService> _logger;

        public TransferenciaService(
            BankMoreDbContext context,
            IHttpContaCorrenteService contaCorrenteService,
            ILogger<TransferenciaService> logger)
        {
            _context = context;
            _contaCorrenteService = contaCorrenteService;
            _logger = logger;
        }

        public async Task<TransferenciaResult> RealizarTransferencia(
            Guid contaOrigemId,
            Guid contaDestinoId,
            decimal valor,
            string descricao,
            string token,
            string? idempotencyKey = null)
        {
            try
            {
                _logger.LogInformation("Processando transferência para chave: {IdempotencyKey}",
                    idempotencyKey ?? "N/A");

                if (valor <= 0)
                {
                    return new TransferenciaResult
                    {
                        Success = false,
                        Mensagem = "Valor deve ser maior que zero",
                        DataHora = DateTime.UtcNow
                    };
                }

                var debitoSucesso = await _contaCorrenteService.RealizarMovimentacaoAsync(
                    token,
                    idempotencyKey ?? Guid.NewGuid().ToString(),
                    contaOrigemId.ToString(), // GUID como string
                    valor,
                    "D");

                if (!debitoSucesso)
                {
                    return new TransferenciaResult
                    {
                        Success = false,
                        Mensagem = "Falha ao debitar conta origem",
                        DataHora = DateTime.UtcNow
                    };
                }

                var creditoSucesso = await _contaCorrenteService.RealizarMovimentacaoAsync(
                    token,
                    idempotencyKey ?? Guid.NewGuid().ToString(),
                    contaDestinoId.ToString(), // GUID como string
                    valor,
                    "C");

                if (!creditoSucesso)
                {
                    _logger.LogWarning("Falha no crédito, realizando estorno...");

                    await _contaCorrenteService.RealizarMovimentacaoAsync(
                        token,
                        $"{idempotencyKey}-ESTORNO",
                        contaOrigemId.ToString(),
                        valor,
                        "C");

                    return new TransferenciaResult
                    {
                        Success = false,
                        Mensagem = "Falha ao creditar conta destino, estorno realizado",
                        DataHora = DateTime.UtcNow
                    };
                }

                var transferencia = new Domain.Entities.Transferencia
                {
                    ContaOrigemId = contaOrigemId,
                    ContaDestinoId = contaDestinoId,
                    Valor = valor,
                    DataTransferencia = DateTime.UtcNow,
                    Status = "CONCLUIDA",
                    IdRequisicao = idempotencyKey ?? Guid.NewGuid().ToString()
                };

                _context.Transferencias.Add(transferencia);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Transferência concluída: {Origem} → {Destino}, R$ {Valor}",
                    contaOrigemId, contaDestinoId, valor);

                return new TransferenciaResult
                {
                    Success = true,
                    Mensagem = $"Transferência de R$ {valor:F2} realizada com sucesso",
                    IdTransacao = transferencia.Id,
                    DataHora = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na transferência para chave: {IdempotencyKey}",
                    idempotencyKey ?? "N/A");
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