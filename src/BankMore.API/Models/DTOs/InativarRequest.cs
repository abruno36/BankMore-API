using System.ComponentModel.DataAnnotations;

namespace BankMore.API.Models.DTOs
{
    public class InativarRequest
    {
        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Confirmação é obrigatória")]
        [Compare("Senha", ErrorMessage = "As senhas não conferem")]
        public string ConfirmacaoSenha { get; set; } = string.Empty;
    }
}