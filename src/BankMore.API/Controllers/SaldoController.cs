using BankMore.API.Services;
using BankMore.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankMore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SaldoController : ControllerBase
    {
        private readonly ISaldoService _saldoService;
        private readonly IContaService _contaService;
        private readonly ILogger<SaldoController> _logger;

        public SaldoController(
            ISaldoService saldoService,
            IContaService contaService,
            ILogger<SaldoController> logger)
        {
            _saldoService = saldoService;
            _contaService = contaService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSaldo()
        {
            try
            {
                var contaClaim = User.FindFirst("contaId") ??
                                User.FindFirst(ClaimTypes.NameIdentifier);

                if (contaClaim == null)
                {
                    return Unauthorized(new { error = "Conta não identificada no token" });
                }

                var conta = await _contaService.ObterContaPorIdAsync(contaClaim.Value);

                if (conta == null)
                {
                    return NotFound(new { error = "Conta não encontrada" });
                }

                var saldo = await _saldoService.CalcularSaldoAsync(conta.Id);

                return Ok(new
                {
                    numeroConta = conta.NumeroConta,
                    nomeTitular = conta.NomeTitular,
                    dataHoraConsulta = DateTime.UtcNow,
                    saldo = saldo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar saldo");
                return StatusCode(500, new { error = "Erro interno ao consultar saldo" });
            }
        }
    }
}