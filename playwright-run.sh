#!/bin/sh
set -e

# Configuración para Docker
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://0.0.0.0:8080
export DAWAZON_BASE_URL=http://app:8080
export DAWAZON_HEADLESS=true

SRC_DIR="/src/dawazonPlayWrite"
REPORTS_DIR="/app/reports"
TRX_FILE="$REPORTS_DIR/test-results.trx"
HTML_DIR="$REPORTS_DIR/html"

mkdir -p "$REPORTS_DIR"
mkdir -p "$HTML_DIR"

echo "=== Compilando proyecto ==="
dotnet build "$SRC_DIR/dawazonPlayWrite.csproj"

echo ""
echo "Ejecutando tests Playwright (.NET + NUnit)..."
echo "Base URL: $DAWAZON_BASE_URL"

# Ejecutar tests
dotnet test "$SRC_DIR/dawazonPlayWrite.csproj" --settings "$SRC_DIR/playwright.runsettings" \
    --no-build \
    --logger "trx;LogFileName=test-results.trx" \
    --results-directory "$REPORTS_DIR" \
    || true

echo ""
echo "=== Archivos generados ==="
ls -la "$REPORTS_DIR/"

echo ""
echo "=== Generando reporte HTML desde TRX ==="

# Verificar si hay archivos trx
TRX_FILES=$(find "$REPORTS_DIR" -name "*.trx" 2>/dev/null || true)

if [ -n "$TRX_FILES" ]; then
    echo "Archivos TRX encontrados: $TRX_FILES"
    
    # Crear HTML básico desde el TRX
    cat > "$HTML_DIR/index.html" << 'HTMLEOF'
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Playwright Test Results</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 20px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; }
        h1 { color: #333; }
        .summary { display: flex; gap: 20px; margin: 20px 0; }
        .stat { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); text-align: center; min-width: 120px; }
        .stat h3 { margin: 0 0 10px 0; color: #666; font-size: 14px; text-transform: uppercase; }
        .stat .value { font-size: 36px; font-weight: bold; }
        .passed .value { color: #28a745; }
        .failed .value { color: #dc3545; }
        .skipped .value { color: #ffc107; }
        table { width: 100%; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        th, td { padding: 12px 15px; text-align: left; }
        th { background: #343a40; color: white; }
        tr:nth-child(even) { background: #f8f9fa; }
        .status-passed { color: #28a745; font-weight: bold; }
        .status-failed { color: #dc3545; font-weight: bold; }
        .status-skipped { color: #ffc107; font-weight: bold; }
        .error-message { color: #dc3545; font-size: 12px; max-width: 400px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    </style>
</head>
<body>
    <div class="container">
        <h1>Playwright E2E Test Results</h1>
        <div class="summary">
HTMLEOF

    # Contar resultados
    PASSED=$(grep -o 'outcome="Passed"' "$TRX_FILES" | wc -l)
    FAILED=$(grep -o 'outcome="Failed"' "$TRX_FILES" | wc -l)
    SKIPPED=$(grep -o 'outcome="Skipped"' "$TRX_FILES" | wc -l)
    TOTAL=$((PASSED + FAILED + SKIPPED))
    
    echo "  <div class='stat passed'><h3>Passed</h3><div class='value'>$PASSED</div></div>" >> "$HTML_DIR/index.html"
    echo "  <div class='stat failed'><h3>Failed</h3><div class='value'>$FAILED</div></div>" >> "$HTML_DIR/index.html"
    echo "  <div class='stat skipped'><h3>Skipped</h3><div class='value'>$SKIPPED</div></div>" >> "$HTML_DIR/index.html"
    echo "  <div class='stat'><h3>Total</h3><div class='value'>$TOTAL</div></div>" >> "$HTML_DIR/index.html"

    cat >> "$HTML_DIR/index.html" << 'HTMLEOF'
        </div>
        <table>
            <thead>
                <tr>
                    <th>Test Name</th>
                    <th>Duration</th>
                    <th>Status</th>
                    <th>Message</th>
                </tr>
            </thead>
            <tbody>
HTMLEOF

    # Extraer resultados
    grep -oP 'testName="[^"]+"[^>]*>' "$TRX_FILES" | while read -r line; do
        TESTNAME=$(echo "$line" | grep -oP 'testName="\K[^"]+')
        DURATION=$(echo "$line" | grep -oP 'duration="\K[0-9:]+')
        OUTCOME=$(echo "$line" | grep -oP 'outcome="\K[^"]+')
        MESSAGE=$(grep -oP "testName=\"$TESTNAME\"[^>]*>.*?<ErrorInfo>.*?<Message>\K[^<]+" "$TRX_FILES" 2>/dev/null | head -1 || echo "")
        
        if [ -n "$TESTNAME" ]; then
            STATUS_CLASS=""
            case $OUTCOME in
                Passed) STATUS_CLASS="status-passed" ;;
                Failed) STATUS_CLASS="status-failed" ;;
                Skipped) STATUS_CLASS="status-skipped" ;;
            esac
            
            ESCAPED_MSG=$(echo "$MESSAGE" | sed 's/&/\&amp;/g; s/</\&lt;/g; s/>/\&gt;/g' | head -c 100)
            
            cat >> "$HTML_DIR/index.html" << EOF
                <tr>
                    <td>$TESTNAME</td>
                    <td>$DURATION</td>
                    <td class="$STATUS_CLASS">$OUTCOME</td>
                    <td class="error-message" title="$MESSAGE">$ESCAPED_MSG</td>
                </tr>
EOF
        fi
    done

    cat >> "$HTML_DIR/index.html" << 'HTMLEOF'
            </tbody>
        </table>
    </div>
</body>
</html>
HTMLEOF

    echo ""
    echo "=== Contenido HTML ==="
    ls -la "$HTML_DIR/"
    echo ""
    echo "Reporte generado: file://$HTML_DIR/index.html"
else
    echo "No se encontraron archivos trx"
fi

echo ""
echo "=== Tests completados ==="
