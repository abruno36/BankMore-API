using System.ComponentModel.DataAnnotations;

namespace BankMore.API.Models.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "CPF ou número da conta é obrigatório")]
        public string Identificador { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; } = string.Empty;
    }
}
