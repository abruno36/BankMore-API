// BankMore.Domain/Entities/Idempotencia.cs
using System;

namespace BankMore.Domain.Entities
{
    public class Idempotencia
    {
        public Guid Id { get; set; }
        public string IdRequisicao { get; set; }
        public string Resultado { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataExpiracao { get; set; }
    }
}