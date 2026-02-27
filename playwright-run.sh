#!/bin/sh
set -e

SRC_DIR="/src/dawazonPlayWrite"
REPORTS_DIR="/app/reports"
TRX_FILE="$REPORTS_DIR/test-results.trx"
HTML_DIR="$REPORTS_DIR/html"

mkdir -p "$REPORTS_DIR"
mkdir -p "$HTML_DIR"

echo "=== Compilando proyecto ==="
dotnet build "$SRC_DIR/dawazonPlayWrite.csproj" -c Release

echo ""
echo "Ejecutando tests Playwright (.NET + NUnit)..."

# Ejecutar tests
dotnet test "$SRC_DIR/dawazonPlayWrite.csproj" \
    -c Release \
    --no-build \
    -v n \
    --logger "trx;LogFileName=test-results.trx" \
    --results-directory "$REPORTS_DIR" \
    || true

echo ""
echo "=== Archivos generados ==="
ls -la "$REPORTS_DIR/"

echo ""
echo "=== Generando reporte HTML ==="

# Buscar todos los archivos trx
TRX_FILES=$(find "$REPORTS_DIR" -name "*.trx" 2>/dev/null || true)

if [ -n "$TRX_FILES" ]; then
    echo "Archivos TRX encontrados: $TRX_FILES"
    reportgenerator \
        -reports:"$TRX_FILES" \
        -targetdir:"$HTML_DIR" \
        -reporttypes:Html \
        2>&1 || echo "Reportgenerator falló"
    
    echo ""
    echo "=== Contenido HTML ==="
    ls -la "$HTML_DIR/"
else
    echo "No se encontraron archivos trx"
fi

echo ""
echo "=== Tests completados ==="
