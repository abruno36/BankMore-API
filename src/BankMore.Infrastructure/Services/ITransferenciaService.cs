using BankMore.Shared.Models;

namespace BankMore.Infrastructure.Services
{
    public interface ITransferenciaService
    {
        Task<TransferenciaResult> RealizarTransferencia(
            Guid contaOrigemId,
            Guid contaDestinoId,
            decimal valor,
            string descricao,
            string token,
            string? idempotencyKey = null);
    }

    public class TransferenciaResult
    {
        public bool Success { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string IdTransacao { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
    }
}