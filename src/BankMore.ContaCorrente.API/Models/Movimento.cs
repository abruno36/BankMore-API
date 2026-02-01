namespace BankMore.ContaCorrente.API.Models
{
    public class Movimento
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ContaCorrenteId { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public char Tipo { get; set; } 
        public string Descricao { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.UtcNow;
        public string RequestId { get; set; } = string.Empty;
    }

    public class Saldo
    {
        public string ContaCorrenteId { get; set; } = string.Empty;
        public decimal ValorAtual { get; set; }
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }
}