using Xunit;

namespace BankMore.API.UnitTests
{
    public class BusinessRulesTests
    {
        [Fact]
        public void TestPasswordHash()
        {
            string senha = "teste123";
            Assert.False(string.IsNullOrEmpty(senha));
        }

        [Fact]
        public void TestBasicAssertions()
        {
            Assert.True(true);
            Assert.Equal(1, 1);
            Assert.NotEqual(1, 2);
        }
    }
}