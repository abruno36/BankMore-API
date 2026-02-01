using BankMore.API.Models.DTOs;

namespace BankMore.API.Services
{
    public interface IMovimentacaoService
    {
        Task RealizarMovimentacaoAsync(MovimentacaoRequest request, string contaIdUsuario);
    }
}