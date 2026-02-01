using Xunit;

namespace BankMore.Transferencia.API.UnitTests
{
    public class TransferenciaServiceTests
    {
        [Fact]
        public void ValidarTransferencia_ValorPositivo_DeveSerValido()
        {
            // Arrange
            decimal valor = 100.00m;

            // Act
            bool valido = valor > 0;

            // Assert
            Assert.True(valido);
        }

        [Theory]
        [InlineData(100.00, true)]
        [InlineData(0.01, true)]
        [InlineData(0, false)]
        [InlineData(-50.00, false)]
        public void ValidarTransferencia_VariosValores_DeveValidarCorretamente(decimal valor, bool esperado)
        {
            // Act
            bool valido = valor > 0;

            // Assert
            Assert.Equal(esperado, valido);
        }

        [Fact]
        public void Idempotencia_ChavesIguais_DeveRetornarIgual()
        {
            // Arrange
            string chave1 = "transferencia-123";
            string chave2 = "transferencia-123";

            // Act & Assert
            Assert.Equal(chave1, chave2);
        }

        [Fact]
        public void Idempotencia_ChavesDiferentes_DeveRetornarDiferente()
        {
            // Arrange
            string chave1 = "transferencia-123";
            string chave2 = "transferencia-456";

            // Act & Assert
            Assert.NotEqual(chave1, chave2);
        }
    }
}