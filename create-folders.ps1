# create-folders.ps1
Write-Host "Criando estrutura de pastas..." -ForegroundColor Green

# Domain Layer
Write-Host "Criando pastas Domain..." -ForegroundColor Cyan
mkdir -Force src/BankMore.Domain/Common
mkdir -Force src/BankMore.Domain/Entities
mkdir -Force src/BankMore.Domain/ValueObjects
mkdir -Force src/BankMore.Domain/Enums
mkdir -Force src/BankMore.Domain/Events

# Application Layer
Write-Host "Criando pastas Application..." -ForegroundColor Cyan
mkdir -Force src/BankMore.Application/Abstractions
mkdir -Force src/BankMore.Application/Commands
mkdir -Force src/BankMore.Application/Queries
mkdir -Force src/BankMore.Application/Handlers
mkdir -Force src/BankMore.Application/Behaviors
mkdir -Force src/BankMore.Application/Models
mkdir -Force src/BankMore.Application/Interfaces

# Commands subpastas
mkdir -Force src/BankMore.Application/Commands/Contas
mkdir -Force src/BankMore.Application/Commands/Movimentacoes
mkdir -Force src/BankMore.Application/Commands/Transferencias

# Handlers subpastas
mkdir -Force src/BankMore.Application/Handlers/Contas
mkdir -Force src/BankMore.Application/Handlers/Movimentacoes
mkdir -Force src/BankMore.Application/Handlers/Transferencias

# Infrastructure Layer
Write-Host "Criando pastas Infrastructure..." -ForegroundColor Cyan
mkdir -Force src/BankMore.Infrastructure/Data
mkdir -Force src/BankMore.Infrastructure/Data/Repositories
mkdir -Force src/BankMore.Infrastructure/Security
mkdir -Force src/BankMore.Infrastructure/Services
mkdir -Force src/BankMore.Infrastructure/Messaging

# Tests
Write-Host "Criando pastas de Testes..." -ForegroundColor Cyan
mkdir -Force tests/BankMore.Domain.UnitTests/ValueObjects
mkdir -Force tests/BankMore.Domain.UnitTests/Entities
mkdir -Force tests/BankMore.Application.UnitTests/Commands
mkdir -Force tests/BankMore.Application.UnitTests/Handlers

Write-Host "Pastas criadas com sucesso!" -ForegroundColor Green
Write-Host "Estrutura:" -ForegroundColor Yellow
tree src /F