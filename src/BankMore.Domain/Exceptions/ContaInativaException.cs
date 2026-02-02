namespace BankMore.Domain.Exceptions
{
    public class ContaInativaException : UnauthorizedAccessException
    {
        public ContaInativaException() : base("Conta inativa") { }
    }
}

// BankMore.Domain/Exceptions/ContaNaoEncontradaException.cs
namespace BankMore.Domain.Exceptions
{
    public class ContaNaoEncontradaException : UnauthorizedAccessException
    {
        public ContaNaoEncontradaException() : base("Conta não encontrada") { }
    }
}

// BankMore.Domain/Exceptions/SenhaIncorretaException.cs
namespace BankMore.Domain.Exceptions
{
    public class SenhaIncorretaException : UnauthorizedAccessException
    {
        public SenhaIncorretaException() : base("Senha incorreta") { }
    }
}