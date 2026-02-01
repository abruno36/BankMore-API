namespace BankMore.Shared.Models
{
    public class MovimentacaoRequest
    {
        public string? IdRequisicao { get; set; }
        public string? NumeroConta { get; set; }
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty; // "C" ou "D"
        public string? Descricao { get; set; }
    }

    public class LoginRequest
    {
        public string Identificador { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public class CadastroRequest
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string NomeTitular { get; set; } = string.Empty;
    }

    public class TransferenciaRequest
    {
        public string NumeroContaDestino { get; set; } = string.Empty;  
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}