using BankMore.Domain.Entities;
using BankMore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankMore.API.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BankMoreDbContext>();

            await context.Database.EnsureCreatedAsync();

            if (!await context.ContasCorrentes.AnyAsync())
            {
                context.ContasCorrentes.AddRange(
                    new ContaCorrente
                    {
                        Id = Guid.Parse("A639DC66-871E-44D3-B9EA-9A45BB57EB74"),
                        NumeroConta = "000001",
                        NomeTitular = "Titular Origem",
                        Email = "origem@banco.com",
                        Ativa = true,
                        DataCriacao = DateTime.UtcNow
                    },
                    new ContaCorrente
                    {
                        Id = Guid.Parse("D2F59F23-01D7-4486-8311-1D002C5B29C5"),
                        NumeroConta = "839366",
                        NomeTitular = "Titular Destino",
                        Email = "destino@banco.com",
                        Ativa = true,
                        DataCriacao = DateTime.UtcNow
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}