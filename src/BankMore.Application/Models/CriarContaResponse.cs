public class CriarContaResponse
{
    public CriarContaResponse(string numeroConta, string nomeTitular, DateTime dataCriacao)
    {
        NumeroConta = numeroConta;
        NomeTitular = nomeTitular;
        DataCriacao = dataCriacao;
    }

    public string NumeroConta { get; }
    public string NomeTitular { get; }
    public DateTime DataCriacao { get; }
}

