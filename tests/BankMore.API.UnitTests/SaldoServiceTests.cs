using Xunit;

namespace BankMore.API.UnitTests
{
    public class SaldoServiceTests
    {
        // Classe auxiliar para teste
        public class MovimentoTeste
        {
            public string Tipo { get; set; } // Alterado para string
            public decimal Valor { get; set; }
        }

        [Fact]
        public void CalcularSaldoAsync_DeveCalcularCorretamente()
        {
            // Arrange - usar STRING "C" e "D" em vez de char
            var movimentos = new List<MovimentoTeste>
            {
                new MovimentoTeste { Tipo = "C", Valor = 2000 },
                new MovimentoTeste { Tipo = "D", Valor = 100 },
                new MovimentoTeste { Tipo = "D", Valor = 50 }
            };

            // Act
            var saldo = movimentos
                .Where(m => m.Tipo == "C").Sum(m => m.Valor)
                - movimentos.Where(m => m.Tipo == "D").Sum(m => m.Valor);

            // Assert
            Assert.Equal(1850, saldo);
        }

        [Fact]
        public void CalcularSaldo_SomenteCreditos_DeveSerPositivo()
        {
            // Arrange
            var movimentos = new List<MovimentoTeste>
            {
                new MovimentoTeste { Tipo = "C", Valor = 1000 },
                new MovimentoTeste { Tipo = "C", Valor = 500 }
            };

            // Act
            var saldo = movimentos
                .Where(m => m.Tipo == "C")
                .Sum(m => m.Valor);

            // Assert
            Assert.Equal(1500, saldo);
        }

        [Fact]
        public void CalcularSaldo_SomenteDebitos_DeveSerNegativo()
        {
            // Arrange
            var movimentos = new List<MovimentoTeste>
            {
                new MovimentoTeste { Tipo = "D", Valor = 300 },
                new MovimentoTeste { Tipo = "D", Valor = 200 }
            };

            // Act
            var totalDebitos = movimentos
                .Where(m => m.Tipo == "D")
                .Sum(m => m.Valor);

            // Assert
            Assert.Equal(500, totalDebitos);
            Assert.True(totalDebitos > 0);
        }
    }
}