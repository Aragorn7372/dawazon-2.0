#!/bin/sh
# LIMPIAR ARCHIVO SH: removes BOM, CRLF, empty lines, trailing whitespace
# Uso: sh cleanSh.sh <archivo>

# Verificar argumento
if [ -z "$1" ]; then
    echo "Uso: $0 <archivo.sh>"
    exit 1
fi

FILE="$1"

# 1. Remove BOM UTF-8 if present
sed -i '1s/^\xEF\xBB\xBF//' "$FILE"

# 2. Convert CRLF to LF
sed -i 's/\r$//' "$FILE"

# 3. Remove empty lines at start
sed -i '1{/^$/d}' "$FILE"

# 4. Remove trailing whitespace
sed -i 's/[[:space:]]*$//' "$FILE"

echo "Limpieza completada: $FILE"
file "$FILE"
