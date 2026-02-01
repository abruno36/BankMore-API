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
            Console.WriteLine($"DEBUG: contaIdUsuario = {contaIdUsuario}");

            var conta = await _context.ContasCorrentes
                .FirstOrDefaultAsync(c => c.NumeroConta == contaIdUsuario);

            if (conta == null)
                throw new Exception($"Conta {contaIdUsuario} não encontrada");

            if (!conta.Ativa)
                throw new Exception("Conta inativa");

            Console.WriteLine($"DEBUG: Conta origem encontrada: {conta.NumeroConta}, ID: {conta.Id}");

            var contaDestino = conta;

            if (!string.IsNullOrEmpty(request.NumeroContaDestino) &&
                request.NumeroContaDestino != conta.NumeroConta)
            {
                Console.WriteLine($"DEBUG: Buscando conta destino: {request.NumeroContaDestino}");

                contaDestino = await _context.ContasCorrentes
                    .FirstOrDefaultAsync(c => c.NumeroConta == request.NumeroContaDestino);

                if (contaDestino == null)
                    throw new Exception($"Conta destino {request.NumeroContaDestino} não encontrada");

                if (!contaDestino.Ativa)
                    throw new Exception("Conta destino inativa");

                if (request.Tipo != "C")
                    throw new Exception("Apenas crédito para contas diferentes");

                Console.WriteLine($"DEBUG: Conta destino encontrada: {contaDestino.NumeroConta}, ID: {contaDestino.Id}");
            }

            if (request.Valor <= 0)
                throw new Exception("Valor deve ser positivo");

            if (request.Tipo != "C" && request.Tipo != "D")
                throw new Exception("Tipo inválido. Use C ou D");

            if (request.Tipo == "D")
            {
                var saldo = await CalcularSaldoAsync(conta.NumeroConta);
                Console.WriteLine($"DEBUG: Saldo da conta {conta.NumeroConta} = {saldo}");

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

            Console.WriteLine($"DEBUG: Criando movimento:");
            Console.WriteLine($"  Tipo: {movimento.Tipo}");
            Console.WriteLine($"  Valor: {movimento.Valor}");
            Console.WriteLine($"  ContaCorrenteId: {movimento.ContaCorrenteId}");
            Console.WriteLine($"  IdRequisicao: {movimento.IdRequisicao}");

            _context.Movimentos.Add(movimento);

            try
            {
                _context.Movimentos.Add(movimento);
                var debugInfo = _context.ChangeTracker.DebugView.LongView;
                Console.WriteLine("=== DEBUG EF CHANGE TRACKER ===");
                Console.WriteLine(debugInfo);
                Console.WriteLine("=== FIM DEBUG ===");

                await _context.SaveChangesAsync();
                Console.WriteLine("DEBUG: Movimento salvo com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG ERRO: {ex.Message}");
                Console.WriteLine($"DEBUG Inner: {ex.InnerException?.Message}");
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
