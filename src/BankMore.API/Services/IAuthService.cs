using BankMore.Domain.Entities;

namespace BankMore.API.Services
{
    public interface IAuthService
    {
        Task<ContaCorrente> CadastrarUsuario(Shared.Models.CadastroRequest request);
        Task<string> Autenticar(Shared.Models.LoginRequest request);
        Task<bool> ValidarCredenciais(string identificador, string senha);
    }
}
