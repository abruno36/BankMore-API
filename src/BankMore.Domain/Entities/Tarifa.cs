// BankMore.Domain/Entities/Tarifa.cs
using System;

namespace BankMore.Domain.Entities
{
    public class Tarifa
    {
        public Guid Id { get; set; }
        public Guid ContaCorrenteId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataTarifa { get; set; }
        public string Descricao { get; set; } = string.Empty;

        // Navegação
        public virtual ContaCorrente ContaCorrente { get; set; } = null!;
    }
}