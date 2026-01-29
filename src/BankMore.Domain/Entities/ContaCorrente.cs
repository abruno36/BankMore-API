namespace BankMore.Domain.Entities;

public class ContaCorrente
{
    public int Id { get; set; }
    public string NumeroConta { get; set; } = string.Empty;
    public string CpfCriptografado { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string NomeTitular { get; set; } = "Cliente BankMore";
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}