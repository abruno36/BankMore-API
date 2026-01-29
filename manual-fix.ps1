# manual-fix.ps1
cd "C:\Projetos\BankMore"  # ou seu caminho real

Write-Host "=== CORRIGINDO ARQUIVOS DO DOMAIN ===" -ForegroundColor Green

# 1. Corrigir TipoMovimento.cs
Write-Host "1. Corrigindo TipoMovimento.cs..." -ForegroundColor Yellow
@'
namespace BankMore.Domain.Enums;

public enum TipoMovimento
{
    Credito = 'C',
    Debito = 'D'
}

public static class TipoMovimentoExtensions
{
    public static char ToChar(this TipoMovimento tipo)
    {
        return (char)tipo;
    }
    
    public static TipoMovimento FromChar(char c)
    {
        switch (c)
        {
            case 'C': return TipoMovimento.Credito;
            case 'D': return TipoMovimento.Debito;
            default: throw new ArgumentException("Tipo de movimento inválido: " + c);
        }
    }
}
'@ | Out-File -FilePath "src/BankMore.Domain/Enums/TipoMovimento.cs" -Encoding UTF8 -Force

# 2. Corrigir ContaCorrente.cs (linha problemática)
Write-Host "2. Corrigindo ContaCorrente.cs..." -ForegroundColor Yellow
$contaCorrentePath = "src/BankMore.Domain/Entities/ContaCorrente.cs"
if (Test-Path $contaCorrentePath) {
    # Ler conteúdo
    $content = Get-Content $contaCorrentePath -Raw
    
    # Substituir aspas problemáticas
    $content = $content -replace '""', '"'
    $content = $content -replace '“', '"'
    $content = $content -replace '”', '"'
    $content = $content -replace '‘', "'"
    $content = $content -replace '’', "'"
    
    # Salvar com encoding correto
    [System.IO.File]::WriteAllText((Resolve-Path $contaCorrentePath), $content, [System.Text.Encoding]::UTF8)
}

# 3. Verificar e compilar
Write-Host "3. Compilando projeto..." -ForegroundColor Cyan
dotnet build src/BankMore.Domain

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SUCCESSO! Domain Layer compilado corretamente." -ForegroundColor Green
} else {
    Write-Host "❌ Ainda há erros. Vamos ver os detalhes:" -ForegroundColor Red
    dotnet build src/BankMore.Domain --verbosity detailed
}