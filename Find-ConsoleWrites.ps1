# Find-ConsoleWrites.ps1
param([string]$path = ".")

Write-Host "🔍 Buscando Console.WriteLine em $path" -ForegroundColor Cyan

$files = Get-ChildItem -Path $path -Recurse -Filter *.cs

$totalEncontrados = 0

foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $consoleLines = @()
    
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match "Console\.(WriteLine|Write|Error\.WriteLine)") {
            $consoleLines += [PSCustomObject]@{
                LineNumber = $i + 1
                Content = $lines[$i].Trim()
                File = $file.Name
            }
            $totalEncontrados++
        }
    }
    
    if ($consoleLines.Count -gt 0) {
        Write-Host "`n📄 $($file.FullName)" -ForegroundColor Yellow
        foreach ($item in $consoleLines) {
            Write-Host "  Line $($item.LineNumber): $($item.Content)" -ForegroundColor Green
        }
    }
}

Write-Host "`n📊 RESUMO:" -ForegroundColor Cyan
Write-Host "  Total de arquivos .cs analisados: $($files.Count)" -ForegroundColor White
Write-Host "  Total de Console.Write encontrados: $totalEncontrados" -ForegroundColor White

if ($totalEncontrados -gt 0) {
    Write-Host "`n⚠️  RECOMENDAÇÃO:" -ForegroundColor Red
    Write-Host "  Considere substituir por:" -ForegroundColor Yellow
    Write-Host "  - _logger.LogInformation() para logs" -ForegroundColor Green
    Write-Host "  - Retornar JSON apropriado para APIs" -ForegroundColor Green
    Write-Host "  - Remover se for debug temporário" -ForegroundColor Green
}

Write-Host "`n✅ Busca concluída!" -ForegroundColor Green
