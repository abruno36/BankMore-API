using Xunit;

namespace BankMore.API.UnitTests
{
    public class BasicTests
    {
        [Fact]
        public void TesteBasico_SempreVerdadeiro()
        {
            Assert.True(true);
        }

        [Fact]
        public void TesteNumeros_SomaCorreta()
        {
            int a = 5;
            int b = 3;
            int soma = a + b;

            Assert.Equal(8, soma);
        }

        [Fact]
        public void TesteString_NaoVazia()
        {
            string texto = "Teste";
            Assert.False(string.IsNullOrEmpty(texto));
        }
    }
}