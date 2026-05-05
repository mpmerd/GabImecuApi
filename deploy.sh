#!/bin/bash
# =====================================================
# deploy.sh — Despliega GabImecuApi en somee.com/api/
# Uso: ./deploy.sh
# =====================================================

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
PUBLISH_DIR="$PROJECT_DIR/publish"

FTP_SERVER="155.254.246.43"
FTP_USER="mpmerd"
FTP_PASSWORD="syzpoZ-birxek-tymre0"
FTP_PATH="www.gabimecu.somee.com/api/"

CURL_OPTS="--user ${FTP_USER}:${FTP_PASSWORD} --ftp-create-dirs --retry 3 --retry-delay 2 --connect-timeout 30"

# --- Paso 1: Compilar (solo win-x64, sin runtimes innecesarios) ---
echo "==> Compilando API (win-x64)..."
cd "$PROJECT_DIR"
rm -rf "$PUBLISH_DIR"
dotnet publish -c Release -o "$PUBLISH_DIR" --nologo 2>&1 | tail -5
if [ $? -ne 0 ]; then echo "ERROR: Falló la compilación."; exit 1; fi
echo "OK — $(find "$PUBLISH_DIR" -type f | wc -l | tr -d ' ') archivos"

# --- Paso 2: Poner app_offline.htm para que IIS libere los DLLs ---
echo "==> Poniendo app offline en IIS..."
echo "<html><body>Actualizando, vuelve en un momento...</body></html>" > /tmp/app_offline.htm
curl -sS -T /tmp/app_offline.htm \
    "ftp://${FTP_SERVER}/${FTP_PATH}app_offline.htm" \
    $CURL_OPTS
echo "OK — IIS detuvo la app"
sleep 3

# --- Paso 3: Limpiar archivos innecesarios del servidor (subidos en deployments anteriores) ---
echo "==> Limpiando archivos innecesarios del servidor..."
ARCHIVOS_BORRAR=(
    "runtimes/win-arm/native/Microsoft.Data.SqlClient.SNI.dll"
    "runtimes/win-arm64/native/Microsoft.Data.SqlClient.SNI.dll"
    "runtimes/win-x86/native/Microsoft.Data.SqlClient.SNI.dll"
    "runtimes/unix/lib/net6.0/Microsoft.Data.SqlClient.dll"
    "runtimes/unix/lib/net6.0/System.Drawing.Common.dll"
    "runtimes/win/lib/net6.0/Microsoft.Data.SqlClient.dll"
    "runtimes/win/lib/net6.0/Microsoft.Win32.SystemEvents.dll"
    "runtimes/win/lib/net6.0/System.Drawing.Common.dll"
    "runtimes/win/lib/net6.0/System.Runtime.Caching.dll"
    "runtimes/win/lib/net6.0/System.Security.Cryptography.ProtectedData.dll"
    "runtimes/win/lib/net6.0/System.Windows.Extensions.dll"
    "GabImecuApi.staticwebassets.endpoints.json"
    "appsettings.Development.json"
)
for f in "${ARCHIVOS_BORRAR[@]}"; do
    curl -sS "ftp://${FTP_SERVER}/" \
        --user "${FTP_USER}:${FTP_PASSWORD}" \
        -Q "DELE ${FTP_PATH}${f}" \
        --connect-timeout 15 2>/dev/null \
        && echo "  Borrado: $f" || echo "  (no existia): $f"
done

# --- Paso 4: Subir todos los archivos ---
TOTAL=$(find "$PUBLISH_DIR" -type f | wc -l | tr -d ' ')
COUNT=0
echo "==> Subiendo $TOTAL archivos..."

find "$PUBLISH_DIR" -type f | sort | while read -r file; do
    REL="${file#$PUBLISH_DIR/}"
    COUNT=$((COUNT + 1))

    curl -sS -T "$file" \
        "ftp://${FTP_SERVER}/${FTP_PATH}${REL}" \
        $CURL_OPTS 2>&1
    if [ $? -ne 0 ]; then
        echo "  [REINTENTO] $REL"
        sleep 2
        curl -sS -T "$file" \
            "ftp://${FTP_SERVER}/${FTP_PATH}${REL}" \
            $CURL_OPTS 2>&1 || echo "  [ERROR] $REL"
    else
        echo "  [$COUNT/$TOTAL] OK: $REL"
    fi
done

# --- Paso 5: Eliminar app_offline.htm para reactivar la app ---
echo "==> Reactivando app en IIS..."
curl -sS "ftp://${FTP_SERVER}/" \
    --user "${FTP_USER}:${FTP_PASSWORD}" \
    -Q "DELE ${FTP_PATH}app_offline.htm" \
    --connect-timeout 30
echo "OK"

echo ""
echo "==> Deploy completado."
echo "    Verifica: https://www.gabimecu.somee.com/api/catalogos/categorias"
