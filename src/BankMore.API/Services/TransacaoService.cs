using BankMore.Shared.Interfaces;
using BankMore.Shared.Models;
using BankMore.Shared.Enums;
using BankMore.Infrastructure.Data;
using BankMore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankMore.API.Services
{
    public class TransacaoService : ITransacaoService
    {
        private readonly BankMoreDbContext _context;

        public TransacaoService(BankMoreDbContext context)
        {
            _context = context;
        }

        public async Task<CommandResult<decimal>> DepositarAsync(
            string userId,
            decimal valor,
            string descricao,
            string? idRequisicao = null)
        {
            var result = await MovimentarAsync(userId, valor, "C", idRequisicao, descricao);

            if (!result.Success)
            {
                return CommandResult<decimal>.FailureResult(
                    result.ErrorMessage ?? "Erro desconhecido",
                    result.ErrorType ?? "UNKNOWN_ERROR");
            }

            return CommandResult<decimal>.SuccessResult(result.Data.SaldoAtual);
        }

        public async Task<CommandResult<decimal>> SacarAsync(
            string userId,
            decimal valor,
            string descricao,
            string? idRequisicao = null)
        {
            var result = await MovimentarAsync(userId, valor, "D", idRequisicao, descricao);

            if (!result.Success)
            {
                return CommandResult<decimal>.FailureResult(
                    result.ErrorMessage ?? "Erro desconhecido",
                    result.ErrorType ?? "UNKNOWN_ERROR");
            }

            return CommandResult<decimal>.SuccessResult(result.Data.SaldoAtual);
        }

        public async Task<CommandResult<MovimentacaoResult>> MovimentarAsync(
            string contaId,
            decimal valor,
            string tipo,
            string? idRequisicao = null,
            string? descricao = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(idRequisicao))
                {
                    var movimentoExistente = await _context.Movimentos
                        .FirstOrDefaultAsync(m => m.IdRequisicao == idRequisicao);

                    if (movimentoExistente != null)
                    {
                        var saldo = await CalcularSaldoAsync(contaId);
                        return CommandResult<MovimentacaoResult>.SuccessResult(new MovimentacaoResult
                        {
                            MovimentoId = movimentoExistente.Id,
                            SaldoAtual = saldo,
                            DataMovimento = movimentoExistente.DataMovimento
                        });
                    }
                }

                var conta = await _context.ContasCorrentes
                    .FirstOrDefaultAsync(c => c.Id == Guid.Parse(contaId));

                if (conta == null)
                    return CommandResult<MovimentacaoResult>.FailureResult(
                        "Conta não encontrada",
                        TipoErro.CONTA_NOT_FOUND.ToString());

                if (!conta.Ativa)
                    return CommandResult<MovimentacaoResult>.FailureResult(
                        "Conta inativa",
                        TipoErro.INACTIVE_ACCOUNT.ToString());

                if (valor <= 0)
                    return CommandResult<MovimentacaoResult>.FailureResult(
                        "Valor deve ser positivo",
                        TipoErro.INVALID_VALUE.ToString());

                if (tipo != "C" && tipo != "D")
                    return CommandResult<MovimentacaoResult>.FailureResult(
                        "Tipo inválido (use 'C' ou 'D')",
                        TipoErro.INVALID_TYPE.ToString());

                if (tipo == "D")
                {
                    var saldoAtual = await CalcularSaldoAsync(contaId);
                    if (saldoAtual < valor)
                        return CommandResult<MovimentacaoResult>.FailureResult(
                            "Saldo insuficiente",
                            TipoErro.SALDO_INSUFICIENTE.ToString());
                }

                var movimento = new Movimento
                {
                    ContaCorrenteId = contaId,
                    Tipo = tipo,
                    Valor = valor,
                    Descricao = descricao ?? $"Movimentação {tipo}",
                    DataMovimento = DateTime.UtcNow,
                    IdRequisicao = idRequisicao
                };

                _context.Movimentos.Add(movimento);
                await _context.SaveChangesAsync();

                var novoSaldo = await CalcularSaldoAsync(contaId);

                return CommandResult<MovimentacaoResult>.SuccessResult(new MovimentacaoResult
                {
                    MovimentoId = movimento.Id,
                    SaldoAtual = novoSaldo,
                    DataMovimento = movimento.DataMovimento
                });
            }
            catch (Exception ex)
            {
                return CommandResult<MovimentacaoResult>.FailureResult(
                    $"Erro na movimentação: {ex.Message}",
                    TipoErro.INTERNAL_ERROR.ToString());
            }
        }

        private async Task<decimal> CalcularSaldoAsync(string contaId)
        {
            var movimentos = await _context.Movimentos
                .Where(m => m.ContaCorrenteId == contaId)
                .ToListAsync();

            var creditos = movimentos.Where(m => m.Tipo == "C").Sum(m => m.Valor);
            var debitos = movimentos.Where(m => m.Tipo == "D").Sum(m => m.Valor);

            return creditos - debitos;
        }
    }
}