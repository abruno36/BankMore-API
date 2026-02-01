using MediatR;
using BankMore.Shared.Models;
using BankMore.Shared.Enums;
using BankMore.Shared.Interfaces;

namespace BankMore.API.Features.Contas.Commands
{
    public class LoginCommand : IRequest<CommandResult<string>>
    {
        public string Identificador { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, CommandResult<string>>
    {
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<CommandResult<string>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var token = await _authService.AutenticarAsync(
                    request.Identificador,
                    request.Senha);

                if (string.IsNullOrEmpty(token))
                {
                    return CommandResult<string>.FailureResult(
                        "Credenciais inválidas",
                        TipoErro.USER_UNAUTHORIZED.ToString());
                }

                return CommandResult<string>.SuccessResult(token!);
            }
            catch (Exception ex)
            {
                return CommandResult<string>.FailureResult(
                    $"Erro no login: {ex.Message}",
                    "INTERNAL_ERROR");
            }
        }
    }
}