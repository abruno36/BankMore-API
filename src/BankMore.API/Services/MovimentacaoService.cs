using BankMore.API.Infrastructure;
using BankMore.API.Models.DTOs;
using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankMore.API.Services
{
    public class MovimentacaoService : IMovimentacaoService
    {
        private readonly BankMoreDbContext _context;

        public MovimentacaoService(BankMoreDbContext context)
        {
            _context = context;
        }

        public async Task RealizarMovimentacaoAsync(MovimentacaoRequest request, string contaIdUsuario)
        {
            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == contaIdUsuario);

            if (conta == null)
                throw new Exception($"Conta {contaIdUsuario} não encontrada");

            if (!conta.Ativa)
                throw new Exception("Conta inativa");

            var contaDestino = conta;

            if (!string.IsNullOrEmpty(request.NumeroContaDestino) &&
                request.NumeroContaDestino != conta.NumeroConta)
            {
                contaDestino = await _context.ContasCorrentes
                    .FirstOrDefaultAsync(c => c.NumeroConta == request.NumeroContaDestino);

                if (contaDestino == null)
                    throw new Exception($"Conta destino {request.NumeroContaDestino} não encontrada");

                if (!contaDestino.Ativa)
                    throw new Exception("Conta destino inativa");

                if (request.Tipo != "C")
                    throw new Exception("Apenas crédito para contas diferentes");

            }

            if (request.Valor <= 0)
                throw new Exception("Valor deve ser positivo");

            if (request.Tipo != "C" && request.Tipo != "D")
                throw new Exception("Tipo inválido. Use C ou D");

            if (request.Tipo == "D")
            {
                var saldo = await CalcularSaldoAsync(conta.NumeroConta);

                if (saldo < request.Valor)
                    throw new Exception($"Saldo insuficiente. Saldo: {saldo}, Valor: {request.Valor}");
            }

            var movimento = new Movimento
            {
                Tipo = request.Tipo,
                Valor = request.Valor,
                DataMovimento = DateTime.UtcNow,
                Descricao = $"Movimentação {request.Tipo} - Valor: {request.Valor:C}",
                ContaCorrenteId = contaDestino.Id.ToString(),
                IdRequisicao = request.IdRequisicao
            };

            _context.Movimentos.Add(movimento);

            try
            {
                _context.Movimentos.Add(movimento);
                var debugInfo = _context.ChangeTracker.DebugView.LongView;

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<decimal> CalcularSaldoAsync(string numeroConta)
        {
            var movimentos = await _context.Movimentos
                .Where(m => m.ContaCorrenteId == numeroConta)
                .ToListAsync();

            var creditos = movimentos
                .Where(m => m.Tipo == "C")
                .Sum(m => m.Valor);

            var debitos = movimentos
                .Where(m => m.Tipo == "D")
                .Sum(m => m.Valor);

            return creditos - debitos;
        }
    }
}
