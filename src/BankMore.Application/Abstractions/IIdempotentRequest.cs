using MediatR;

namespace BankMore.Application.Abstractions;

public interface IIdempotentRequest<TResponse> : IRequest<TResponse>
{
    string IdRequisicao { get; }
}
