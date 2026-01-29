namespace BankMore.Domain.Entities;

public class Movimento
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // "C" = Crédito, "D" = Débito
    public decimal Valor { get; set; }
    public DateTime DataMovimento { get; set; } = DateTime.UtcNow;
    public string Descricao { get; set; } = string.Empty;
    public int ContaCorrenteId { get; set; }
}