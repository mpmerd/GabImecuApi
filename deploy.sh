#!/bin/bash
# =====================================================
# deploy_gabimecu_api.sh
# Compila y despliega la API en www.gabimecu.somee.com/api/
# =====================================================

set -e

# === CONFIGURACIÓN ===
PROJECT_DIR="$HOME/.openclaw/workspace/GabImecuApi"
PUBLISH_DIR="$PROJECT_DIR/publish"

# Credenciales desde variables de entorno
FTP_HOST="${GABIMECU_FTP_HOST:?Error: Define GABIMECU_FTP_HOST}"
FTP_USER="${GABIMECU_FTP_USER:?Error: Define GABIMECU_FTP_USER}"
FTP_PASSWORD="${GABIMECU_FTP_PASSWORD:?Error: Define GABIMECU_FTP_PASSWORD}"

# Extraer solo el hostname (sin ftp:// ni ruta)
FTP_SERVER="${FTP_HOST#ftp://}"      # quitar ftp://
FTP_SERVER="${FTP_SERVER#ftps://}"   # quitar ftps://
FTP_SERVER="${FTP_SERVER%%/*}"       # quitar ruta después del host

# La ruta base en el FTP (todo lo que va después del host en FTP_HOST)
FTP_BASE="${FTP_HOST#*${FTP_SERVER}}"
# Quitar / inicial si existe para evitar doble slash
FTP_BASE="${FTP_BASE#/}"

# Ruta final: base + api/
FTP_PATH="${FTP_BASE}/api/"

echo "🔨 Compilando y publicando API..."
cd "$PROJECT_DIR"
rm -rf "$PUBLISH_DIR"
dotnet restore
dotnet publish -c Release -o "$PUBLISH_DIR" --self-contained false
cp "$PROJECT_DIR/web.config" "$PUBLISH_DIR/"

echo "✅ Publicación exitosa en $PUBLISH_DIR"
echo ""
echo "📤 Servidor: $FTP_SERVER"
echo "📤 Ruta:     $FTP_PATH"
echo ""

TOTAL=$(find "$PUBLISH_DIR" -type f | wc -l)
COUNT=0

find "$PUBLISH_DIR" -type f | while read -r file; do
    REL="${file#$PUBLISH_DIR/}"
    COUNT=$((COUNT + 1))
    echo "  [$COUNT/$TOTAL] $REL"
    
    curl -sS -T "$file" \
         "ftp://${FTP_SERVER}/${FTP_PATH}${REL}" \
         --user "${FTP_USER}:${FTP_PASSWORD}" \
         --ftp-create-dirs || {
        echo "❌ Error subiendo $REL"
        exit 1
    }
done

echo ""
echo "✅ Despliegue completado"
echo "🌐 https://www.gabimecu.somee.com/api/"
echo ""
echo "📋 Configura en Somee.com la variable de entorno:"
echo "   GABIMECU_CONNECTION_STRING"
