#!/bin/bash
# =====================================================
# deploy_gabimecu_api.sh
# Compila y despliega la API en Somee.com vía FTP
# =====================================================

set -e

# === CONFIGURACIÓN ===
PROJECT_DIR="$HOME/.openclaw/workspace/GabImecuApi"
PUBLISH_DIR="$PROJECT_DIR/publish"

# Leer credenciales de variables de entorno
# Define estas variables ANTES de ejecutar el script:
#   export GABIMECU_FTP_HOST="ftp.gabimecu.somee.com"
#   export GABIMECU_FTP_USER="tu_usuario"
#   export GABIMECU_FTP_PASSWORD="tu_password"

FTP_HOST="${GABIMECU_FTP_HOST:?Error: Define GABIMECU_FTP_HOST}"
FTP_USER="${GABIMECU_FTP_USER:?Error: Define GABIMECU_FTP_USER}"
FTP_PASSWORD="${GABIMECU_FTP_PASSWORD:?Error: Define GABIMECU_FTP_PASSWORD}"

# === COMPILAR ===
echo "🔨 Compilando API..."
cd "$PROJECT_DIR"
dotnet restore
dotnet publish -c Release -o "$PUBLISH_DIR"

echo "✅ Compilación exitosa"

# === EMPAQUETAR ===
ARCHIVE="/tmp/gabimecu_api.zip"
rm -f "$ARCHIVE"
cd "$PUBLISH_DIR"
zip -r "$ARCHIVE" ./*

echo "📦 Empaquetado: $ARCHIVE"

# === SUBIR VÍA FTP ===
echo "📤 Subiendo a $FTP_HOST..."
curl -T "$ARCHIVE" \
     "ftp://$FTP_HOST/gabimecu_api.zip" \
     --user "${FTP_USER}:${FTP_PASSWORD}" \
     --ftp-create-dirs

echo ""
echo "✅ Despliegue completado"
echo "⚠️  IMPORTANTE: Debes descomprimir gabimecu_api.zip manualmente"
echo "    en el panel de control de Somee.com (File Manager)"
echo ""
echo "📋 No olvides configurar la variable de entorno en Somee.com:"
echo "    GABIMECU_CONNECTION_STRING"
