# Configuración
$BaseDir = $PSScriptRoot
$ReportPath = "$BaseDir\bruno-report.json"
$Env = "Local"

Write-Host "Iniciando pruebas de Bruno..." -ForegroundColor Cyan

# 1. Verificar si el CLI de Bruno está instalado
if (!(Get-Command bru -ErrorAction SilentlyContinue)) {
    Write-Host "Bruno CLI no está instalado. Instalándolo globalmente con npm..." -ForegroundColor Yellow
    npm install -g @usebruno/cli
}

# 2. Orden de ejecución: Auth primero para obtener los tokens, luego el resto
$Carpetas = @(
    "ControladorAuth",
    "ControladorProductos",
    "ControladorCarrito",
    "ControladorAdmin"
)

$TotalFailed = 0

foreach ($Carpeta in $Carpetas) {
    $RutaCarpeta = Join-Path $BaseDir $Carpeta
    Write-Host ""
    Write-Host "Ejecutando: $Carpeta" -ForegroundColor Yellow

    bru run $RutaCarpeta --env $Env --output "$BaseDir\report-$Carpeta.json"

    if ($LASTEXITCODE -ne 0) {
        $TotalFailed++
        Write-Host "Fallos en $Carpeta" -ForegroundColor Red
    } else {
        Write-Host "$Carpeta OK" -ForegroundColor Green
    }
}

Write-Host ""
if ($TotalFailed -eq 0) {
    Write-Host "¡Todas las carpetas pasaron exitosamente!" -ForegroundColor Green
} else {
    Write-Host "$TotalFailed carpeta(s) con fallos. Revisa los reportes en $BaseDir" -ForegroundColor Red
}