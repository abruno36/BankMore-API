using BankMore.API.Models.DTOs;
using BankMore.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContaController : ControllerBase
    {
        private readonly IContaService _contaService;

        public ContaController(IContaService contaService)
        {
            _contaService = contaService;
        }

        [HttpPost("inativar")]
        public async Task<IActionResult> Inativar([FromBody] InativarRequest request)
        {
            try
            {
                var contaIdClaim = User.FindFirst("contaId")?.Value;

                if (string.IsNullOrEmpty(contaIdClaim))
                    return Unauthorized(new { message = "Token inválido" });

                if (request.Senha != request.ConfirmacaoSenha)
                    return BadRequest(new { message = "Senha e confirmação não conferem" });

                await _contaService.InativarContaAsync(contaIdClaim, request.Senha);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("não encontrada"))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Senha inválida" });
            }
        }

        [HttpGet("dados")]
        public async Task<IActionResult> ObterDados()
        {
            try
            {
                var contaId = User.FindFirst("contaId")?.Value;

                if (string.IsNullOrEmpty(contaId))
                    return Unauthorized(new { message = "Token inválido" });

                var conta = await _contaService.ObterContaPorIdAsync(contaId);

                if (conta == null)
                    return NotFound(new { message = "Conta não encontrada" });

                return Ok(conta);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("validar/{numeroConta}")]
        public async Task<IActionResult> ValidarConta(string numeroConta)
        {
            try
            {
                var conta = await _contaService.ObterContaPorNumeroAsync(numeroConta);
                var contaValida = conta != null && conta.Ativo;

                return Ok(new
                {
                    contaValida,
                    message = contaValida ? "Conta válida" : "Conta inválida ou inativa"
                });
            }
            catch (Exception)
            {
                return Ok(new { contaValida = false, message = "Conta não encontrada" });
            }
        }
    }
}