using BankMore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/saldo")]
[Authorize]
public class SaldoController : ControllerBase
{
    private readonly ISaldoService _saldoService;

    public SaldoController(ISaldoService saldoService)
    {
        _saldoService = saldoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterSaldo()
    {
        var contaId = User.FindFirst("contaId")?.Value;

        var saldoInfo = await _saldoService.ObterSaldoCompleto(contaId);

        return Ok(new
        {
            NumeroConta = saldoInfo.NumeroConta,
            NomeTitular = saldoInfo.NomeTitular,
            DataHoraConsulta = DateTime.UtcNow,
            Saldo = saldoInfo.Saldo
        });
    }
}