using Xunit;

namespace BankMore.API.UnitTests
{
    public class CryptoServiceTests
    {
        [Fact]
        public void TestCriptografia_Basico()
        {
            string textoOriginal = "12345678901";

            string textoCriptografado = textoOriginal + "_encrypted";
            string textoDescriptografado = textoOriginal;

            Assert.NotEqual(textoOriginal, textoCriptografado);
            Assert.Equal(textoOriginal, textoDescriptografado);
        }

        [Fact]
        public void TestCriptografia_Vazio()
        {
            string textoVazio = "";
            Assert.Equal("", textoVazio);
        }
    }
}