# Configuración
$CollectionPath = ".\dawazonBrunoTest" 
$Environment = "Local"              
$ReportPath = ".\bruno-report.json" 

Write-Host "🚀 Iniciando pruebas de Bruno..." -ForegroundColor Cyan

# 1. Verificar si el CLI de Bruno está instalado
if (!(Get-Command bru -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Bruno CLI no está instalado. Instalándolo globalmente con npm..." -ForegroundColor Yellow
    npm install -g @usebruno/cli
}

Write-Host "Ejecutando la colección en: $CollectionPath" -ForegroundColor Cyan

# 2. Ejecutar la colección (quita '--env $Environment' si no usas variables de entorno)
bru run $CollectionPath --env $Environment --output $ReportPath

# 3. Comprobar el resultado
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ ¡Todas las pruebas pasaron exitosamente!" -ForegroundColor Green
} else {
    Write-Host "⚠️ Algunas pruebas fallaron. Revisa el reporte en $ReportPath." -ForegroundColor Red
}