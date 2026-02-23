#!/usr/bin/env bash
# ??????????????????????????????????????????????????????????????
# VinCipher — One-time VPS setup script
# Run this ONCE on a fresh Ubuntu 22.04+ / Debian 12+ VPS.
# Usage: sudo bash vps-setup.sh
# ??????????????????????????????????????????????????????????????
set -euo pipefail

echo "=== VinCipher VPS Setup ==="
echo ""

# 1. Install .NET 10 runtime
echo "[1/6] Installing .NET 10 runtime..."
wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --runtime aspnetcore --channel 10.0 --install-dir /usr/share/dotnet
ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet
dotnet --info

# 2. Install nginx
echo "[2/6] Installing nginx..."
apt-get update -qq
apt-get install -y -qq nginx unzip

# 3. Create app directory and user
echo "[3/6] Creating app directory..."
mkdir -p /var/www/vincipher
mkdir -p /etc/vincipher

# Ensure www-data user exists (created by nginx)
chown -R www-data:www-data /var/www/vincipher
chown -R www-data:www-data /etc/vincipher

# 4. Install systemd service
echo "[4/6] Installing systemd service..."
cp "$(dirname "$0")/vincipher.service" /etc/systemd/system/vincipher.service
systemctl daemon-reload
systemctl enable vincipher

# 5. Install nginx config
echo "[5/6] Configuring nginx..."
cp "$(dirname "$0")/nginx-vincipher.conf" /etc/nginx/sites-available/vincipher
ln -sf /etc/nginx/sites-available/vincipher /etc/nginx/sites-enabled/vincipher
rm -f /etc/nginx/sites-enabled/default

nginx -t
systemctl reload nginx

# 6. Install certbot for TLS (optional — run later)
echo "[6/6] Installing certbot..."
apt-get install -y -qq certbot python3-certbot-nginx

echo ""
echo "=== Setup complete ==="
echo ""
echo "Next steps:"
echo "  1. Point your DNS (vincipher.com) to this server's IP"
echo "  2. Run: sudo certbot --nginx -d vincipher.com -d www.vincipher.com"
echo "  3. Push to master — the pipeline will handle the rest"
echo ""
echo "Useful commands:"
echo "  sudo systemctl status vincipher     # Service status"
echo "  sudo journalctl -u vincipher -f     # Live logs"
echo "  sudo systemctl restart vincipher    # Restart"
echo "  sudo nginx -t && sudo systemctl reload nginx  # Reload nginx"
