namespace BankMore.Application.Models.Auth;

public class LoginRequest
{
    public string Identificador { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public string NomeTitular { get; set; } = string.Empty;
    public int NumeroConta { get; set; }
}