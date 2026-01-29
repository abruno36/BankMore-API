# create-application-files.ps1
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
cd $scriptPath

Write-Host "Criando arquivos da Application Layer..." -ForegroundColor Green

# ========== MODELS ==========
Write-Host "Criando Models..." -ForegroundColor Cyan

@'
using System;

namespace BankMore.Application.Models;

public class Result<T> : Result
{
    public T Data { get; init; }

    protected internal Result(T data, bool success, string errorCode, string errorMessage)
        : base(success, errorCode, errorMessage)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(data, true, null, null);
    public static new Result<T> Failure(string errorCode, string errorMessage) 
        => new(default, false, errorCode, errorMessage);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    protected Result(bool success, string errorCode, string errorMessage)
    {
        IsSuccess = success;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string errorCode, string errorMessage) 
        => new(false, errorCode, errorMessage);
}
'@ | Out-File "src/BankMore.Application/Models/Result.cs" -Encoding UTF8 -Force

# Models/Auth/
mkdir -Force src/BankMore.Application/Models/Auth

@'
namespace BankMore.Application.Models.Auth;

public class LoginRequest
{
    public string Identificador { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public string NomeTitular { get; set; } = string.Empty;
    public int NumeroConta { get; set; }
}
'@ | Out-File "src/BankMore.Application/Models/Auth/AuthModels.cs" -Encoding UTF8 -Force

# ========== ABSTRAÇÕES ==========
Write-Host "Criando Abstractions..." -ForegroundColor Cyan

@'
using MediatR;

namespace BankMore.Application.Abstractions;

public interface IIdempotentRequest<TResponse> : IRequest<TResponse>
{
    string IdRequisicao { get; }
}
'@ | Out-File "src/BankMore.Application/Abstractions/IIdempotentRequest.cs" -Encoding UTF8 -Force

# ========== INTERFACES ==========
Write-Host "Criando Interfaces..." -ForegroundColor Cyan

@'
namespace BankMore.Application.Interfaces;

public interface ISecurityService
{
    (string Hash, string Salt) HashSenha(string senha);
    bool ValidarSenha(string senha, string hash, string salt);
    string CriptografarCPF(string cpf);
    string DecriptografarCPF(string cpfCriptografado);
}
'@ | Out-File "src/BankMore.Application/Interfaces/ISecurityService.cs" -Encoding UTF8 -Force

@'
namespace BankMore.Application.Interfaces;

public interface ITokenService
{
    string GerarToken(Guid contaId, int numeroConta, string nome);
    bool ValidarToken(string token);
    Guid ObterContaIdDoToken(string token);
}
'@ | Out-File "src/BankMore.Application/Interfaces/ITokenService.cs" -Encoding UTF8 -Force

@'
namespace BankMore.Application.Interfaces;

public interface INumeroContaService
{
    Task<int> GerarProximoNumeroAsync();
}
'@ | Out-File "src/BankMore.Application/Interfaces/INumeroContaService.cs" -Encoding UTF8 -Force

@'
using BankMore.Domain.Entities;

namespace BankMore.Application.Interfaces;

public interface IContaCorrenteRepository
{
    Task<ContaCorrente?> ObterPorIdAsync(Guid id);
    Task<ContaCorrente?> ObterPorNumeroAsync(int numero);
    Task<ContaCorrente?> ObterPorCpfCriptografadoAsync(string cpfCriptografado);
    Task AdicionarAsync(ContaCorrente conta);
    Task AtualizarAsync(ContaCorrente conta);
    Task<bool> ExisteNumeroAsync(int numero);
}
'@ | Out-File "src/BankMore.Application/Interfaces/IContaCorrenteRepository.cs" -Encoding UTF8 -Force

Write-Host "Application Layer criada! Feche e reabra o VS." -ForegroundColor Green