using System.ComponentModel.DataAnnotations;

namespace BankMore.Domain.Entities
{
    public class Idempotencia
    {
        [Key]
        public string IdRequisicao { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}