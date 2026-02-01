namespace BankMore.Shared.Models
{
    public class Transacao
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ContaId { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty; // "DEPOSITO" ou "SAQUE"
        public string Descricao { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.UtcNow;
    }
}