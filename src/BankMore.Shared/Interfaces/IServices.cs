using BankMore.Shared.Models;

namespace BankMore.Shared.Interfaces
{
    public interface IAuthService
    {
        Task<string?> AutenticarAsync(string identificador, string senha);
    }

    public interface IContaService
    {
        Task<string> CadastrarContaAsync(string cpf, string senha, string nomeTitular);
        Task<decimal> ConsultarSaldoAsync(string contaId);
        Task<bool> UsuarioTemAcessoContaAsync(string userId, string contaId);
        Task<bool> InativarContaAsync(string contaId, string senha);
        Task<ContaInfo?> ObterContaPorIdAsync(string contaId);
        Task<ContaInfo?> ObterContaPorCpfAsync(string cpf);
        Task<ContaInfo?> ObterContaPorNumeroAsync(string numeroConta);
    }

    public interface ITransacaoService
    {
        Task<CommandResult<decimal>> DepositarAsync(string userId, decimal valor, string descricao, string? idRequisicao = null);
        Task<CommandResult<decimal>> SacarAsync(string userId, decimal valor, string descricao, string? idRequisicao = null);
        Task<CommandResult<MovimentacaoResult>> MovimentarAsync(
            string contaId,
            decimal valor,
            string tipo,
            string? idRequisicao = null,
            string? descricao = null);
    }

    public interface ITokenService
    {
        string GenerateToken(string userId, string email);
        string? ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        bool IsTokenValid(string token);
    }

    public class ContaInfo
    {
        public string Id { get; set; } = string.Empty;
        public string NumeroConta { get; set; } = string.Empty;
        public string NomeTitular { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    public class MovimentacaoResult
    {
        public string MovimentoId { get; set; } = string.Empty;
        public decimal SaldoAtual { get; set; }
        public DateTime DataMovimento { get; set; }
    }
}