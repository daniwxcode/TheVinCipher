#!/usr/bin/env bash
# ??????????????????????????????????????????????????????????????
# VinCipher — VPS deployment script (lab project — values hardcoded)
# Runs ON the VPS. Called either by:
#   - deploy-local.ps1  (from your Windows machine via SSH)
#   - Azure DevOps pipeline
# Usage: sudo ./deploy.sh
# ??????????????????????????????????????????????????????????????
set -euo pipefail

APP_PATH="/var/www/vincipher"
SERVICE_NAME="vincipher"
DB_CONN="Server=SQL2019-001.adaptivewebhosting.com;Database=hermesey_vinAudit;User Id=VinEasyUser;Password=@Iq42m*s7;MultipleActiveResultSets=True;"
HMAC_SECRET="v1Nc!pH3r_pL4yGr0uNd_\$ecReT_2025#x9Kz"
PG_TOKEN="VinCipherCar-DECODE-ONLY-5AA01EAFC25A31A66C11D6D9626D858A5EA"
VINDECODER_KEY="ca2fcd922501"
VINDECODER_SECRET="86f4cbf2e8"
VINAUDIT_KEY="K683RXZF5DYELYC"

DEPLOY_TMP="/tmp/vincipher-deploy"
BACKUP_PATH="/var/www/vincipher-backup"
ZIP_FILE=$(find "$DEPLOY_TMP" -name "*.zip" | head -1)

if [ -z "$ZIP_FILE" ]; then
    echo "ERROR: No zip file found in $DEPLOY_TMP"
    exit 1
fi

echo "=== VinCipher deployment ==="
echo "  App path:    $APP_PATH"
echo "  Service:     $SERVICE_NAME"
echo "  Artifact:    $ZIP_FILE"
echo ""

# 1. Stop the running service
echo "[1/6] Stopping $SERVICE_NAME..."
systemctl stop "$SERVICE_NAME" 2>/dev/null || true

# 2. Backup current version
echo "[2/6] Backing up current version..."
if [ -d "$APP_PATH" ]; then
    rm -rf "$BACKUP_PATH"
    cp -a "$APP_PATH" "$BACKUP_PATH"
fi

# 3. Extract new version
echo "[3/5] Extracting new version..."
mkdir -p "$APP_PATH"
rm -rf "${APP_PATH:?}"/*
unzip -qo "$ZIP_FILE" -d "$APP_PATH"
# 4. Set permissions
echo "[4/5] Setting permissions..."
chown -R www-data:www-data "$APP_PATH" 2>/dev/null || true
chmod -R 755 "$APP_PATH"

# 5. Start service
echo "[5/5] Starting $SERVICE_NAME..."
systemctl daemon-reload
systemctl start "$SERVICE_NAME"
systemctl enable "$SERVICE_NAME" 2>/dev/null || true

echo ""
echo "=== Deployment complete ==="
systemctl status "$SERVICE_NAME" --no-pager -l || true
