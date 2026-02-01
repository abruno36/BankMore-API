namespace BankMore.Shared.Interfaces
{
    public interface IHttpContaCorrenteService
    {
        Task<bool> RealizarMovimentacaoAsync(
            string token,
            string idempotencyKey,
            string contaId,  
            decimal valor,
            string tipoMovimentacao);
    }
}
