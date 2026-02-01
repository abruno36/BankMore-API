using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using BankMore.Infrastructure.Services;
using BankMore.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BankMore.API.Services
{
    public class TransferenciaOrquestradorService
    {
        private readonly BankMoreDbContext _context;
        private readonly ILogger<TransferenciaOrquestradorService> _logger;

        public TransferenciaOrquestradorService(
            BankMoreDbContext context,
            ILogger<TransferenciaOrquestradorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TransferenciaResult> ProcessarTransferencia(
            TransferenciaRequest request,
            string? idempotenciaKey)
        {
            try
            {
                _logger.LogInformation("Iniciando orquestração de transferência");

                var contaOrigem = await ObterContaOrigemAsync();
                var contaDestino = await ObterContaDestinoAsync(request.NumeroContaDestino);

                var transferencia = await CriarTransferenciaAsync(
                    contaOrigem,
                    contaDestino,
                    request.Valor,
                    idempotenciaKey);

                await RegistrarTransferenciaAsync(transferencia);

                _logger.LogInformation("Transferência {Id} registrada com sucesso", transferencia.Id);

                return new TransferenciaResult
                {
                    Success = true,
                    Mensagem = $"Transferência de R$ {request.Valor:F2} realizada",
                    IdTransacao = transferencia.Id,
                    DataHora = transferencia.DataTransferencia
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na orquestração de transferência");
                throw;
            }
        }

        private async Task<ContaCorrente> ObterContaOrigemAsync()
        {
            // Em produção: extrair do token JWT
            return await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == "000001")
                ?? throw new Exception("Conta origem não encontrada");
        }

        private async Task<ContaCorrente> ObterContaDestinoAsync(string numeroConta)
        {
            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == numeroConta);

            return conta ?? throw new Exception($"Conta destino {numeroConta} não encontrada");
        }

        private async Task<Transferencia> CriarTransferenciaAsync(
            ContaCorrente origem,
            ContaCorrente destino,
            decimal valor,
            string? idempotenciaKey)
        {
            return new Transferencia
            {
                Id = Guid.NewGuid().ToString(),
                ContaOrigemId = origem.Id,
                ContaDestinoId = destino.Id,
                Valor = valor,
                DataTransferencia = DateTime.UtcNow,
                Status = "CONCLUIDA",
                IdRequisicao = idempotenciaKey ?? Guid.NewGuid().ToString()
            };
        }

        private async Task RegistrarTransferenciaAsync(Transferencia transferencia)
        {
            _context.Transferencias.Add(transferencia);
            await _context.SaveChangesAsync();
        }
    }
}