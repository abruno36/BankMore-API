using MediatR;
using BankMore.Shared.Models;
using BankMore.Shared.Enums;
using BankMore.Shared.Interfaces;

namespace BankMore.API.Features.Contas.Queries
{
    public class ConsultarSaldoQuery : IRequest<CommandResult<decimal>>
    {
        public string Token { get; set; } = string.Empty;
        public string ContaId { get; set; } = string.Empty;
    }

    public class ConsultarSaldoQueryHandler
        : IRequestHandler<ConsultarSaldoQuery, CommandResult<decimal>>
    {
        private readonly IContaService _contaService;
        private readonly ITokenService _tokenService;

        public ConsultarSaldoQueryHandler(
            IContaService contaService,
            ITokenService tokenService)
        {
            _contaService = contaService;
            _tokenService = tokenService;
        }

        public async Task<CommandResult<decimal>> Handle(
            ConsultarSaldoQuery request,
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

                var temAcesso = await _contaService.UsuarioTemAcessoContaAsync(
                    userId!,
                    request.ContaId);

                if (!temAcesso)
                {
                    return CommandResult<decimal>.FailureResult(
                        "Acesso não autorizado a esta conta",
                        TipoErro.USER_UNAUTHORIZED.ToString());
                }

                var saldo = await _contaService.ConsultarSaldoAsync(request.ContaId);

                return CommandResult<decimal>.SuccessResult(saldo);
            }
            catch (Exception ex)
            {
                return CommandResult<decimal>.FailureResult(
                    $"Erro ao consultar saldo: {ex.Message}",
                    "INTERNAL_ERROR");
            }
        }
    }
}