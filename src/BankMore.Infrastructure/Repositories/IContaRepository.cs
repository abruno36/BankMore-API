using BankMore.Domain.Entities;

namespace BankMore.API.Domain.Repositories
{
    public interface IContaRepository
    {
        Task<ContaCorrente> ObterPorNumeroAsync(string numeroConta);
        Task<ContaCorrente> ObterPorCPFAsync(string cpf);
        Task AdicionarAsync(ContaCorrente conta);
        Task<bool> ExisteCPFAsync(string cpf);
        Task<int> SalvarAlteracoesAsync();
    }
}
