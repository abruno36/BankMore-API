using BankMore.API.Models.DTOs;
using BankMore.Domain.Entities;

namespace BankMore.API.Services
{
    public interface IAuthService
    {
        Task<ContaCorrente> CadastrarUsuario(CadastroRequest request);
        Task<string> Autenticar(LoginRequest request);
        Task<bool> ValidarCredenciais(string identificador, string senha);
    }
}
