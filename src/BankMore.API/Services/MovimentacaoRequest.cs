using System.ComponentModel.DataAnnotations;

namespace BankMore.API.Models.DTOs
{
    public class MovimentacaoRequest
    {
        public string NumeroContaDestino { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [Required]
        [RegularExpression("^(C|D)$")]
        public string Tipo { get; set; } = string.Empty;

        public string IdRequisicao { get; set; } = Guid.NewGuid().ToString();
    }
}