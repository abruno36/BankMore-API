using System;

namespace BankMore.Domain.Entities;

public class Transferencia
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string Status { get; set; } = "PENDENTE";
    public string? IdRequisicao { get; set; }

    public int ContaOrigemId { get; set; }
    public virtual ContaCorrente? ContaOrigem { get; set; }

    public int ContaDestinoId { get; set; }
    public virtual ContaCorrente? ContaDestino { get; set; }
}