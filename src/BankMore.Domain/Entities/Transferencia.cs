using System.ComponentModel.DataAnnotations;

namespace BankMore.Domain.Entities
{
    public class Transferencia
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public Guid ContaOrigemId { get; set; }

        [Required]
        public Guid ContaDestinoId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [Required]
        public DateTime DataTransferencia { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "PENDENTE"; // PENDENTE, CONCLUIDA, FALHA

        [Required]
        public string IdRequisicao { get; set; } = string.Empty;

        public virtual ContaCorrente ContaOrigem { get; set; } = null!;
        public virtual ContaCorrente ContaDestino { get; set; } = null!;
    }
}