using BankMore.API.Models.DTOs;
using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
    public async Task RealizarMovimentacaoAsync(MovimentacaoRequest request)
    {
        // CONTA FIXA PARA TESTE
        string contaIdUsuario = "000001";

        var conta = await _context.ContasCorrentes
            .FirstOrDefaultAsync(c => c.NumeroConta == contaIdUsuario);

        if (conta == null) throw new Exception($"Conta {contaIdUsuario} não encontrada");
        if (!conta.Ativa) throw new Exception("Conta inativa");

        var contaDestino = conta;

        if (!string.IsNullOrEmpty(request.NumeroContaDestino) &&
            request.NumeroContaDestino != conta.NumeroConta)
        {
            contaDestino = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == request.NumeroContaDestino);

            if (contaDestino == null) throw new Exception($"Conta destino não encontrada");
            if (!contaDestino.Ativa) throw new Exception("Conta destino inativa");
            if (request.Tipo != "C") throw new Exception("Apenas crédito para contas diferentes");
        }

        if (request.Valor <= 0) throw new Exception("Valor deve ser positivo");
        if (request.Tipo != "C" && request.Tipo != "D") throw new Exception("Tipo inválido");

        var sql = @"
        INSERT INTO Movimentacoes (Id, ContaId, Tipo, Valor, Descricao, DataMovimentacao, IdRequisicao)
        VALUES (@Id, @ContaId, @Tipo, @Valor, @Descricao, @DataMovimentacao, @IdRequisicao)
    ";

        await _context.Database.ExecuteSqlRawAsync(sql,
            new SqliteParameter("@Id", Guid.NewGuid().ToString()),
            new SqliteParameter("@ContaId", contaDestino.Id),
            new SqliteParameter("@Tipo", request.Tipo),
            new SqliteParameter("@Valor", request.Valor),
            new SqliteParameter("@Descricao", $"Movimentação {request.Tipo} - Valor: {request.Valor:C}"),
            new SqliteParameter("@DataMovimentacao", DateTime.UtcNow),
            new SqliteParameter("@IdRequisicao", request.IdRequisicao)
        );
    }
}