using MediatR;
using BankMore.Shared.Models;
using BankMore.Shared.Enums;
using BankMore.Shared.Interfaces;

namespace BankMore.API.Features.Contas.Commands
{
    public class DepositarCommand : IRequest<CommandResult<decimal>>
    {
        public string Token { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }

    public class DepositarCommandHandler
        : IRequestHandler<DepositarCommand, CommandResult<decimal>>
    {
        private readonly ITransacaoService _transacaoService;
        private readonly ITokenService _tokenService;

        public DepositarCommandHandler(
            ITransacaoService transacaoService,
            ITokenService tokenService)
        {
            _transacaoService = transacaoService;
            _tokenService = tokenService;
        }

        public async Task<CommandResult<decimal>> Handle(
            DepositarCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken(request.Token);
                if (string.IsNullOrEmpty(userId))
                {
                    return CommandResult<decimal>.FailureResult(
                        "Token inválido ou expirado",
                        TipoErro.USER_UNAUTHORIZED.ToString());
                }

                if (request.Valor <= 0)
                {
                    return CommandResult<decimal>.FailureResult(
                        "Valor deve ser maior que zero",
                        TipoErro.INVALID_VALUE.ToString());
                }

                var result = await _transacaoService.DepositarAsync(
                    userId!,
                    request.Valor,
                    request.Descricao);

                return result;
            }
            catch (Exception ex)
            {
                return CommandResult<decimal>.FailureResult(
                    $"Erro ao depositar: {ex.Message}",
                    "INTERNAL_ERROR");
            }
        }
    }
}