using System.ComponentModel.DataAnnotations;

namespace BankMore.Domain.Entities
{
    public class IdempotencyKey
    {
        [Key]
        public string Id { get; set; } = string.Empty; 

        [Required]
        public string RequestType { get; set; } = string.Empty; 

        public string? ContaOrigem { get; set; }
        public string? ContaDestino { get; set; }
        public decimal? Valor { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty; 

        public string? ResponseData { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}