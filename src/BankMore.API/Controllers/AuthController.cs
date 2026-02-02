using BankMore.API.Exceptions;
using BankMore.API.Models.DTOs;
using BankMore.API.Services;
using BankMore.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] Shared.Models.CadastroRequest request) // ← Agora é Shared.Models
        {
            try
            {
                var result = await _authService.CadastrarUsuario(request);
                return Ok(new
                {
                    NumeroConta = result.NumeroConta,
                    Mensagem = "Conta criada com sucesso"
                });
            }
            catch (DomainException ex) when (ex.ErrorType == "INVALID_DOCUMENT")
            {
                return BadRequest(new ErrorResponse("CPF inválido", "INVALID_DOCUMENT"));
            }
            catch (DomainException ex) when (ex.ErrorType == "DUPLICATE_DOCUMENT")
            {
                return BadRequest(new ErrorResponse("CPF já cadastrado", "DUPLICATE_DOCUMENT"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse(ex.Message, "CADASTRO_ERROR"));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Shared.Models.LoginRequest request) // ← Agora é Shared.Models
        {
            try
            {
                var token = await _authService.Autenticar(request);
                return Ok(new { Token = token });
            }
            catch (ContaInativaException)
            {
                return Unauthorized(new ErrorResponse("Conta inativa", "CONTA_INATIVA"));
            }
            catch (ContaNaoEncontradaException)
            {
                return Unauthorized(new ErrorResponse("Conta não encontrada", "CONTA_NAO_ENCONTRADA"));
            }
            catch (SenhaIncorretaException)
            {
                return Unauthorized(new ErrorResponse("Senha incorreta", "SENHA_INCORRETA"));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ErrorResponse("Credenciais inválidas", "USER_UNAUTHORIZED"));
            }
        }
    }
}