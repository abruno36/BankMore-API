# setup.ps1 (Windows PowerShell)
Write-Host "Criando estrutura BankMore..." -ForegroundColor Green

# Criar diretórios
mkdir -Force src, tests, docker | Out-Null

# Criar Solution
dotnet new sln -n BankMore

# Criar projetos
dotnet new classlib -n BankMore.Domain -o src/BankMore.Domain -f net8.0
dotnet new classlib -n BankMore.Application -o src/BankMore.Application -f net8.0
dotnet new classlib -n BankMore.Infrastructure -o src/BankMore.Infrastructure -f net8.0
dotnet new webapi -n ContaCorrente.API -o src/ContaCorrente.API -f net8.0
dotnet new webapi -n Transferencia.API -o src/Transferencia.API -f net8.0

# Criar projetos de teste
dotnet new xunit -n BankMore.Domain.UnitTests -o tests/BankMore.Domain.UnitTests -f net8.0
dotnet new xunit -n BankMore.Application.UnitTests -o tests/BankMore.Application.UnitTests -f net8.0
dotnet new xunit -n ContaCorrente.API.IntegrationTests -o tests/ContaCorrente.API.IntegrationTests -f net8.0

# Adicionar todos à Solution
Get-ChildItem -Path src, tests -Directory | ForEach-Object {
    dotnet sln add $_.FullName
}

Write-Host "Estrutura criada com sucesso!" -ForegroundColor Green