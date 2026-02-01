using System.ComponentModel.DataAnnotations;

namespace BankMore.Domain.Entities
{
    public class ContaCorrente
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string NumeroConta { get; set; } = string.Empty;

        public string CPFCriptografado { get; set; } = string.Empty;

        public string CPFHash { get; set; } = string.Empty;

        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NomeTitular { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public bool Ativa { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataInativacao { get; set; }

        public ContaCorrente(
            string cpf,
            string nomeTitular,
            string email,
            string senhaHash)
        {
            CPFCriptografado = cpf;
            NomeTitular = nomeTitular;
            Email = email;
            SenhaHash = senhaHash;
            NumeroConta = GerarNumeroConta();
        }

        public ContaCorrente() { }

        private string GerarNumeroConta()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString("000000");
        }

        public void Inativar()
        {
            Ativa = false;
            DataInativacao = DateTime.UtcNow;
        }

        public bool ValidarCPF()
        {
            var cpfLimpo = new string(CPFCriptografado.Where(char.IsDigit).ToArray());
            return cpfLimpo.Length == 11;
        }
    }
}