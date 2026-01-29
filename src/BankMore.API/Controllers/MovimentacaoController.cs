using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankMore.Infrastructure.Data;
using BankMore.Domain.Entities;

namespace BankMore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimentacaoController : ControllerBase
{
    private readonly BankMoreDbContext _context;

    public MovimentacaoController(BankMoreDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RealizarMovimentacao([FromBody] MovimentacaoRequest request)
    {
        var contaId = User.FindFirst("contaId")?.Value;
        var numeroContaToken = User.FindFirst("numeroConta")?.Value;

        if (string.IsNullOrEmpty(contaId))
            return Unauthorized(new { message = "Token inválido", errorType = "INVALID_TOKEN" });

        var conta = await _context.ContasCorrente
            .FirstOrDefaultAsync(c => c.Id == int.Parse(contaId));

        if (conta == null)
            return BadRequest(new { message = "Conta não encontrada", errorType = "INVALID_ACCOUNT" });

        if (!conta.Ativo)
            return BadRequest(new { message = "Conta inativa", errorType = "INACTIVE_ACCOUNT" });

        var contaDestino = conta;
        if (!string.IsNullOrEmpty(request.NumeroContaDestino) && request.NumeroContaDestino != numeroContaToken)
        {
            contaDestino = await _context.ContasCorrente
                .FirstOrDefaultAsync(c => c.NumeroConta == request.NumeroContaDestino);

            if (contaDestino == null)
                return BadRequest(new { message = "Conta destino não encontrada", errorType = "INVALID_ACCOUNT" });

            if (!contaDestino.Ativo)
                return BadRequest(new { message = "Conta destino inativa", errorType = "INACTIVE_ACCOUNT" });

            if (request.Tipo != "C")
                return BadRequest(new { message = "Apenas crédito para contas diferentes", errorType = "INVALID_TYPE" });
        }

        if (request.Valor <= 0)
            return BadRequest(new { message = "Valor deve ser positivo", errorType = "INVALID_VALUE" });

        if (request.Tipo != "C" && request.Tipo != "D")
            return BadRequest(new { message = "Tipo inválido. Use C ou D", errorType = "INVALID_TYPE" });

        if (request.Tipo == "D")
        {
            var movimentosConta = await _context.Movimentos
                .Where(m => m.ContaCorrenteId == conta.Id)
                .ToListAsync();

            var creditos = movimentosConta
                .Where(m => m.Tipo == "C")
                .Sum(m => m.Valor);

            var debitos = movimentosConta
                .Where(m => m.Tipo == "D")
                .Sum(m => m.Valor);

            var saldo = creditos - debitos;

            if (saldo < request.Valor)
                return BadRequest(new { message = "Saldo insuficiente", errorType = "INSUFFICIENT_BALANCE" });
        }

        var movimento = new Movimento
        {
            Tipo = request.Tipo,
            Valor = request.Valor,
            DataMovimento = DateTime.UtcNow,
            Descricao = $"Movimentação {request.Tipo}",
            ContaCorrenteId = contaDestino.Id
        };

        _context.Movimentos.Add(movimento);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class MovimentacaoRequest
{
    public string NumeroContaDestino { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
}