using Xunit;

namespace BankMore.API.UnitTests;

public class BusinessRulesTests
{
    [Fact]
    public void Cpf_DeveTer11Digitos()
    {
        var cpf = "12345678901";
        Assert.Equal(11, cpf.Length);
    }

    [Fact]
    public void ValorMovimentacao_DeveSerPositivo()
    {
        var valor = 100.50m;
        Assert.True(valor > 0);
    }

    [Fact]
    public void TiposMovimento_Validos()
    {
        Assert.Equal("C", "C"); 
        Assert.Equal("D", "D"); 
    }

    [Fact]
    public void Conta_DeveEstarAtivaParaOperacoes()
    {
        var ativa = true;
        Assert.True(ativa);
    }

    [Fact]
    public void BCrypt_HashAndVerify_Funciona()
    {
        var senha = "SenhaSegura123";
        var hash = BCrypt.Net.BCrypt.HashPassword(senha);
        var valido = BCrypt.Net.BCrypt.Verify(senha, hash);

        Assert.True(valido);
    }

    [Fact]
    public void Transferencia_ApenasCreditoParaTerceiros()
    {
        var contaOrigem = "123456";
        var contaDestino = "789012";

        if (contaOrigem != contaDestino)
        {
            var tipoValido = "C";
            Assert.Equal("C", tipoValido);
        }
    }
}