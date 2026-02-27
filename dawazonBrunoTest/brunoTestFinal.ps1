# ==========================================
# CONFIGURACIÓN
# ==========================================

$BaseDir = $PSScriptRoot
$Env = "Local"
$ReportsDir = Join-Path $BaseDir "reports"
$FailCount = 0

$Carpetas = @(
    "ControladorAuth",
    "ControladorProductos",
    "ControladorCarrito",
    "ControladorAdmin"
)

# Crear carpeta reports si no existe
if (-not (Test-Path $ReportsDir)) {
    New-Item -ItemType Directory -Path $ReportsDir | Out-Null
}

Write-Host "Iniciando pruebas con Bruno CLI..."
Write-Host "Directorio base: $BaseDir"
Write-Host "Entorno: $Env"
Write-Host "========================================="

# ==========================================
# INSTALAR BRUNO CLI SI NO EXISTE
# ==========================================

if (-not (Get-Command bru -ErrorAction SilentlyContinue)) {
    Write-Host "Instalando Bruno CLI..."
    npm install -g @usebruno/cli
}

# ==========================================
# INSTALAR REPORTER HTML SI NO EXISTE
# ==========================================

$ReporterInstalled = npm list -g @usebruno/reporter-html 2>$null
if (-not $ReporterInstalled) {
    Write-Host "Instalando reporter HTML..."
    npm install -g @usebruno/reporter-html
}

# ==========================================
# EJECUTAR CARPETAS
# ==========================================

$Resumen = @()

foreach ($Carpeta in $Carpetas) {

    $Ruta = Join-Path $BaseDir $Carpeta
    $RutaCarpeta = Join-Path $BaseDir $Carpeta
    Write-Host ""
    Write-Host "Ejecutando: $Carpeta" -ForegroundColor Yellow

    bru run $RutaCarpeta --env $Env  --format html --output "$BaseDir\report-$Carpeta.html"
    
    # Capturar último HTML generado
    $UltimoHtml = Get-ChildItem -Path $BaseDir -Filter "*.html" |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1

    $NuevoNombre = Join-Path $ReportsDir "report-$Carpeta.html"

    if ($UltimoHtml) {
        Move-Item $UltimoHtml.FullName $NuevoNombre -Force
    }

    if ($LASTEXITCODE -ne 0) {
        $TotalFailed++
        Write-Host "Fallos en $Carpeta" -ForegroundColor Red
    } else {
        Write-Host "$Carpeta OK" -ForegroundColor Green
        $Estado="pass"
    }
    $Resumen += [PSCustomObject]@{
        Carpeta = $Carpeta
        Estado  = $Estado
        Archivo = "report-$Carpeta.html"
    }
}


$IndexPath = Join-Path $ReportsDir "index.html"

$Html = @"
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<title>Dashboard Bruno</title>
<style>
body { font-family: Arial; background:#f4f6f9; padding:40px; }
h1 { color:#333; }
.card {
    background:dark;
    padding:20px;
    margin:15px 0;
    border-radius:8px;
    box-shadow:0 4px 8px rgba(0,0,0,0.1);
}
.pass { border-left:8px solid #28a745; }
.fail { border-left:8px solid #dc3545; }
a { text-decoration:none; font-weight:bold; color:#007bff; }
.summary {
    font-size:18px;
    margin-top:30px;
}
</style>
</head>
<body>

<h1>Dashboard de Pruebas Bruno</h1>
<p><strong>Fecha:</strong> $(Get-Date)</p>
<p><strong>Entorno:</strong> $Env</p>

"@

foreach ($Item in $Resumen) {

    $Clase = if ($Item.Estado -eq "PASÓ") { "card pass" } else { "card fail" }

    if ($Item.Archivo -ne "-") {
        $Link = "<a href='$($Item.Archivo)' target='_blank'>Ver Reporte</a>"
    } else {
        $Link = "-"
    }

    $Html += @"
<div class="$Clase">
<h2>$($Item.Carpeta)</h2>
<p>Estado: <strong>$($Item.Estado)</strong></p>
<p>$Link</p>
</div>
"@
}

$Html += @"
<div class="summary">
<h2>Total de Fallos: $FailCount</h2>
</div>

</body>
</html>
"@

$Html | Out-File $IndexPath -Encoding utf8

Write-Host "`n Dashboard generado en: $IndexPath"

# ==========================================
# RESUMEN FINAL
# ==========================================

Write-Host "`n========================================="
if ($FailCount -eq 0) {
    Write-Host "Todas las pruebas pasaron correctamente!"
}
else {
    Write-Host "$FailCount carpeta(s) fallaron."
}
Write-Host "========================================="

exit $FailCount