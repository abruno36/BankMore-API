using Microsoft.EntityFrameworkCore;
using BankMore.Domain.Entities;

namespace BankMore.Infrastructure.Data
{
    public class BankMoreDbContext : DbContext
    {
        public BankMoreDbContext(DbContextOptions<BankMoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContaCorrente> ContasCorrentes { get; set; }
        public DbSet<Movimento> Movimentos { get; set; }
        public DbSet<Transferencia> Transferencias { get; set; }
        public DbSet<Idempotencia> Idempotencias { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContaCorrente>().ToTable("ContasCorrentes");
            modelBuilder.Entity<Movimento>().ToTable("Movimentacoes");
            modelBuilder.Entity<Transferencia>().ToTable("Transferencias");
            modelBuilder.Entity<Idempotencia>().ToTable("Idempotencias");
            modelBuilder.Entity<Tarifa>().ToTable("Tarifas");

            modelBuilder.Entity<ContaCorrente>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.CPFCriptografado).IsUnique();
                entity.HasIndex(c => c.NumeroConta).IsUnique();
                entity.Property(c => c.NumeroConta).HasMaxLength(20);
                entity.Property(c => c.CPFCriptografado).HasMaxLength(11);
                entity.Property(c => c.NomeTitular).HasMaxLength(100);
                entity.Property(c => c.Email).HasMaxLength(100);
                entity.Property(c => c.SenhaHash).IsRequired();
            });

            modelBuilder.Entity<Movimento>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.ContaCorrenteId)
                      .HasColumnName("ContaId"); 
                entity.Property(m => m.Tipo).HasMaxLength(1);
                entity.Property(m => m.Descricao).HasMaxLength(200);
                entity.Property(m => m.DataMovimento).HasColumnName("DataMovimentacao");
            });

            modelBuilder.Entity<Transferencia>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).HasMaxLength(36);
                entity.Property(t => t.ContaOrigemId).HasMaxLength(36);
                entity.Property(t => t.ContaDestinoId).HasMaxLength(36);
                entity.Property(t => t.Status).HasMaxLength(20);
                entity.Property(t => t.IdRequisicao).HasMaxLength(255);

                entity.HasOne(t => t.ContaOrigem)
                      .WithMany()
                      .HasForeignKey(t => t.ContaOrigemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.ContaDestino)
                      .WithMany()
                      .HasForeignKey(t => t.ContaDestinoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<IdempotencyKey>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(255); 
                entity.Property(e => e.RequestType).HasMaxLength(50);
                entity.Property(e => e.ContaOrigem).HasMaxLength(50);
                entity.Property(e => e.ContaDestino).HasMaxLength(50);
                entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.ResponseData).HasColumnType("TEXT"); 
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var novasContas = ChangeTracker.Entries<ContaCorrente>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();

            if (novasContas.Any())
            {

                foreach (var conta in novasContas)
                {
                    if (conta.CPFCriptografado != null &&
                        conta.CPFCriptografado.Length < 20 &&
                        !conta.CPFCriptografado.Contains("="))
                    {

                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}