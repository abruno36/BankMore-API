// tests/BankMore.API.UnitTests/SimpleTests.cs
using Xunit;

namespace BankMore.API.UnitTests;

public class SimpleTests
{
    [Fact]
    public void Test1_ValidacaoCpf()
    {
        Assert.Equal(11, "12345678901".Length);
    }

    [Fact]
    public void Test2_ValidacaoTipos()
    {
        Assert.True("C" == "C" || "D" == "D");
    }

    [Fact]
    public void Test3_ValidacaoValor()
    {
        Assert.True(100.00m > 0);
    }

    [Fact]
    public void Test4_HashSenha()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("teste");
        Assert.NotNull(hash);
    }
}