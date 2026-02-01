using MediatR;
using BankMore.Shared.Models;
using BankMore.Shared.Enums;
using BankMore.Shared.Interfaces;

namespace BankMore.API.Features.Contas.Commands
{
    public class CadastrarContaCommand : IRequest<CommandResult<string>>
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string NomeTitular { get; set; } = string.Empty;
    }

    public class CadastrarContaCommandHandler
        : IRequestHandler<CadastrarContaCommand, CommandResult<string>>
    {
        private readonly IContaService _contaService;

        public CadastrarContaCommandHandler(IContaService contaService)
        {
            _contaService = contaService;
        }

        public async Task<CommandResult<string>> Handle(
            CadastrarContaCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (request.Cpf.Length != 11)
                {
                    return CommandResult<string>.FailureResult(
                        "CPF deve ter exatamente 11 dígitos",
                        TipoErro.INVALID_DOCUMENT.ToString());
                }

                if (string.IsNullOrWhiteSpace(request.NomeTitular))
                {
                    return CommandResult<string>.FailureResult(
                        "Nome do titular é obrigatório",
                        TipoErro.INVALID_DOCUMENT.ToString());
                }

                var numeroConta = await _contaService.CadastrarContaAsync(
                    request.Cpf,
                    request.Senha,
                    request.NomeTitular);

                return CommandResult<string>.SuccessResult(numeroConta);
            }
            catch (Exception ex)
            {
                return CommandResult<string>.FailureResult(
                    $"Erro ao cadastrar conta: {ex.Message}",
                    "INTERNAL_ERROR");
            }
        }
    }
}