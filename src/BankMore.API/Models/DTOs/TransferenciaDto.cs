namespace BankMore.API.Models.DTOs
{
    public class TransferenciaRequestDto
    {
        public string NumeroContaDestino { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }

    public class TransferenciaResponseDto
    {
        public bool Success { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string ContaDestino { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string IdTransacao { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
    }
}
