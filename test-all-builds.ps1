# Script para testar build de todos os microsservicos

Write-Host "Testando build de todos os microsservicos..." -ForegroundColor Cyan

$services = @("DispatchService", "DeliveryService", "InboxService", "NotificationService")
$results = @()

foreach ($service in $services) {
    Write-Host "Buildando $service..." -ForegroundColor Yellow
    
    try {
        Push-Location $service
        $buildResult = dotnet build --verbosity quiet 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "SUCESSO: $service" -ForegroundColor Green
            $results += [PSCustomObject]@{ Service = $service; Status = "SUCESSO" }
        } else {
            Write-Host "FALHOU: $service" -ForegroundColor Red
            Write-Host $buildResult -ForegroundColor Red
            $results += [PSCustomObject]@{ Service = $service; Status = "FALHOU" }
        }
    }
    catch {
        Write-Host "ERRO: $service - $_" -ForegroundColor Red
        $results += [PSCustomObject]@{ Service = $service; Status = "ERRO" }
    }
    finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "RESUMO DOS BUILDS:" -ForegroundColor Cyan
$results | Format-Table -AutoSize

# Testar Contracts tambem
Write-Host "Buildando Contracts..." -ForegroundColor Yellow
try {
    Push-Location "Contracts/GuildaMensageria.Contracts"
    $buildResult = dotnet build --verbosity quiet 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "SUCESSO: Contracts" -ForegroundColor Green
    } else {
        Write-Host "FALHOU: Contracts" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
    }
}
catch {
    Write-Host "ERRO: Contracts - $_" -ForegroundColor Red
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Teste de builds concluido!" -ForegroundColor Cyan