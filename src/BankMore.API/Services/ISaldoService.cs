namespace BankMore.API.Services
{
    public interface ISaldoService
    {
        Task<decimal> CalcularSaldoAsync(string numeroConta);
        Task<SaldoResponse> ObterSaldoCompleto(string numeroConta); 
    }

    public class SaldoResponse
    {
        public string NumeroConta { get; set; } = string.Empty;
        public string NomeTitular { get; set; } = string.Empty;
        public DateTime DataHoraConsulta { get; set; }
        public decimal Saldo { get; set; }  
    }
}