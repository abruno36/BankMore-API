using BankMore.Application.Abstractions;
using BankMore.Application.Interfaces;
using BankMore.Domain.Interfaces;
using MediatR;

public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IIdempotenciaRepository _idempotenciaRepository;

    public IdempotencyBehavior(IIdempotenciaRepository idempotenciaRepository)
    {
        _idempotenciaRepository = idempotenciaRepository;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Verificar se é uma request idempotente
        if (request is IIdempotentRequest<TResponse> idempotentRequest)
        {
            // Verificar se já temos resultado para esta requisição
            var resultadoExistente = await _idempotenciaRepository
                .ObterResultadoAsync<TResponse>(idempotentRequest.IdRequisicao); // ← CORREÇÃO AQUI

            if (resultadoExistente != null)
                return resultadoExistente;
        }

        // Executar a requisição
        return await next();
    }
}