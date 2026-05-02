#!/bin/bash
# =====================================================
# deploy_gabimecu_api.sh
# Compila y despliega la API en www.gabimecu.somee.com/api/
# =====================================================

set -e

# === CONFIGURACIÓN ===
PROJECT_DIR="$HOME/.openclaw/workspace/GabImecuApi"
PUBLISH_DIR="$PROJECT_DIR/publish"
FTP_PATH="/api/"  # Subcarpeta en Somee.com

# Credenciales desde variables de entorno
FTP_HOST="${GABIMECU_FTP_HOST:?Error: Define GABIMECU_FTP_HOST}"
FTP_USER="${GABIMECU_FTP_USER:?Error: Define GABIMECU_FTP_USER}"
FTP_PASSWORD="${GABIMECU_FTP_PASSWORD:?Error: Define GABIMECU_FTP_PASSWORD}"

echo "🔨 Compilando y publicando API..."
cd "$PROJECT_DIR"

# Limpiar publish anterior
rm -rf "$PUBLISH_DIR"

# Restaurar y publicar
dotnet restore
dotnet publish -c Release -o "$PUBLISH_DIR" --self-contained false

# Copiar web.config al publish
cp "$PROJECT_DIR/web.config" "$PUBLISH_DIR/"

echo "✅ Publicación exitosa en $PUBLISH_DIR"
echo ""
echo "📤 Subiendo archivos a $FTP_HOST$FTP_PATH ..."

# Contar archivos
TOTAL=$(find "$PUBLISH_DIR" -type f | wc -l)
COUNT=0

# Subir archivo por archivo
find "$PUBLISH_DIR" -type f | while read -r file; do
    # Ruta relativa dentro del publish
    REL="${file#$PUBLISH_DIR/}"
    COUNT=$((COUNT + 1))
    
    echo "  [$COUNT/$TOTAL] $REL"
    
    curl -sS -T "$file" \
         "ftp://$FTP_HOST$FTP_PATH$REL" \
         --user "${FTP_USER}:${FTP_PASSWORD}" \
         --ftp-create-dirs || {
        echo "❌ Error subiendo $REL"
        exit 1
    }
done

echo ""
echo "✅ Despliegue completado exitosamente"
echo "🌐 API disponible en: https://www.gabimecu.somee.com/api/"
echo ""
echo "📋 Recuerda configurar en el panel de Somee.com:"
echo "   Variable de entorno: GABIMECU_CONNECTION_STRING"
