using System.ComponentModel.DataAnnotations;

namespace BankMore.Domain.Entities
{
    public class Movimento
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ContaCorrenteId { get; set; } = string.Empty;

        [Required]
        [MaxLength(1)]
        public string Tipo { get; set; } = string.Empty; 

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [MaxLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public DateTime DataMovimento { get; set; } = DateTime.UtcNow;

         public string? IdRequisicao { get; set; }

        //public virtual ContaCorrente ContaCorrente { get; set; } = null!;
    }
}