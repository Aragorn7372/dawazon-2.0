#!/bin/bash

# CONFIGURACIÓN

BaseDir="$(cd "$(dirname "$0")" && pwd)"
Env="Local"
ReportsDir="$BaseDir/reports"
FailCount=0

Carpetas=(
  "ControladorAuth"
  "ControladorProductos"
  "ControladorCarrito"
  "ControladorAdmin"
)

# Crear carpeta reports si no existe
if [ ! -d "$ReportsDir" ]; then
  mkdir -p "$ReportsDir"
fi

echo "Iniciando pruebas con Bruno CLI..."
echo "Directorio base: $BaseDir"
echo "Entorno: $Env"
echo "========================================="

# INSTALAR BRUNO CLI SI NO EXISTE

if ! command -v bru &> /dev/null
then
  echo "Instalando Bruno CLI..."
  npm install -g @usebruno/cli
fi

# INSTALAR REPORTER HTML SI NO EXISTE

if ! npm list -g @usebruno/reporter-html &> /dev/null
then
  echo "Instalando reporter HTML..."
  npm install -g @usebruno/reporter-html
fi

# EJECUTAR CARPETAS

Resumen=""

for Carpeta in "${Carpetas[@]}"
do
  RutaCarpeta="$BaseDir/$Carpeta"
  OutputPath="$ReportsDir/report-$Carpeta.html"

  echo ""
  echo "📁 Ejecutando: $Carpeta"

  bru run "$RutaCarpeta" \
    --env "$Env" \
    --reporter-html "$OutputPath"

  ExitCode=$?

  if [ $ExitCode -ne 0 ]; then
    echo "⚠️  Fallos en $Carpeta"
    Estado="FALLÓ"
    FailCount=$((FailCount+1))
  else
    echo "✅ $Carpeta OK"
    Estado="PASÓ"
  fi

  Resumen="$Resumen
<div class=\"card $( [ "$Estado" = "PASÓ" ] && echo "pass" || echo "fail" )\">
<h2>$Carpeta</h2>
<p>Estado: <strong>$Estado</strong></p>
<p><a href='report-$Carpeta.html' target='_blank'>Ver Reporte</a></p>
</div>
"
done

# GENERAR DASHBOARD HTML

IndexPath="$ReportsDir/index.html"

cat > "$IndexPath" <<EOF
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<title>Dashboard Bruno</title>
<style>
body { font-family: Arial; background:#f4f6f9; padding:40px; }
h1 { color:#333; }
.card {
    background:white;
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
<p><strong>Fecha:</strong> $(date)</p>
<p><strong>Entorno:</strong> $Env</p>

$Resumen

<div class="summary">
<h2>Total de Fallos: $FailCount</h2>
</div>

</body>
</html>
EOF

echo ""
echo "Dashboard generado en: $IndexPath"

# RESUMEN FINAL

echo ""
echo "========================================="
if [ $FailCount -eq 0 ]; then
  echo "Todas las pruebas pasaron correctamente!"
else
  echo "$FailCount carpeta(s) fallaron."
fi
echo "========================================="

exit $FailCount