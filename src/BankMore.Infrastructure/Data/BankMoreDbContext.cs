using Microsoft.EntityFrameworkCore;
using BankMore.Domain.Entities;

namespace BankMore.Infrastructure.Data;

public class BankMoreDbContext : DbContext
{
    public BankMoreDbContext(DbContextOptions<BankMoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContaCorrente> ContasCorrente { get; set; }
    public DbSet<Movimento> Movimentos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContaCorrente>()
            .HasIndex(c => c.CpfCriptografado)
            .IsUnique();

        modelBuilder.Entity<ContaCorrente>()
            .HasIndex(c => c.NumeroConta)
            .IsUnique();
    }
}