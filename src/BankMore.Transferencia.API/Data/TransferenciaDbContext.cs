using BankMore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankMore.Transferencia.API.Data;

public class TransferenciaDbContext : BankMoreDbContext
{
    public TransferenciaDbContext(DbContextOptions<BankMoreDbContext> options)
        : base(options)
    {
    }
}