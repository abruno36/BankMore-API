# dependencies.ps1

# BankMore.Application
cd src/BankMore.Application
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package Microsoft.Extensions.Logging.Abstractions
cd ../..

# BankMore.Infrastructure  
cd src/BankMore.Infrastructure
dotnet add package Dapper
dotnet add package Microsoft.Data.Sqlite
dotnet add package Microsoft.Extensions.Caching.Memory
dotnet add package Microsoft.Extensions.Configuration
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package BCrypt.Net-Next
cd ../..

# ContaCorrente.API
cd src/ContaCorrente.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Microsoft.Extensions.Http.Polly
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
dotnet add reference ../BankMore.Application
dotnet add reference ../BankMore.Infrastructure
cd ../..

# Transferencia.API
cd src/Transferencia.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.Extensions.Http.Polly
dotnet add reference ../BankMore.Application
dotnet add reference ../BankMore.Infrastructure
cd ../..

# Projetos de Teste
cd tests/BankMore.Domain.UnitTests
dotnet add reference ../../src/BankMore.Domain
dotnet add package xunit
dotnet add package Moq
dotnet add package FluentAssertions
cd ../..

cd tests/BankMore.Application.UnitTests
dotnet add reference ../../src/BankMore.Application
dotnet add reference ../../src/BankMore.Domain
dotnet add package xunit
dotnet add package Moq
dotnet add package FluentAssertions
cd ../..

cd tests/ContaCorrente.API.IntegrationTests
dotnet add reference ../../src/ContaCorrente.API
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package xunit
cd ../..

Write-Host "Dependências instaladas!" -ForegroundColor Green