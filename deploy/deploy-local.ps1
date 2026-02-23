# ================================================================
# VinCipher — Deploy from local machine to VPS via SSH/SCP
# SSH will prompt for the password each time.
# Usage:  .\deploy\deploy-local.ps1
# ================================================================
$VpsHost = "62.171.155.200"
$VpsUser = "root"

$ErrorActionPreference = "Stop"

# --- Config ---
$Project        = "VinCipher.com/VinCipher.com.csproj"
$Configuration  = "Release"
$PublishDir      = "publish"
$ZipName        = "VinCipher.com.zip"
$RemoteTmp      = "/tmp/vincipher-deploy"

Write-Host ""
Write-Host "=== VinCipher — Local Deployment ===" -ForegroundColor Cyan
Write-Host "  VPS:       $VpsUser@$VpsHost"
Write-Host "  Project:   $Project"
Write-Host "  Auth:      password (SSH will prompt)"
Write-Host ""

# --- 1. Build & Publish ---
Write-Host "[1/5] Publishing project..." -ForegroundColor Yellow
if (Test-Path $PublishDir) { Remove-Item -Recurse -Force $PublishDir }
dotnet publish $Project -c $Configuration -o $PublishDir --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# --- 2. Upload published output to VPS tmp dir ---
Write-Host "[2/5] Uploading published files to VPS..." -ForegroundColor Yellow
$SshOpts = @("-o", "StrictHostKeyChecking=no")

Write-Host "  (enter password for $VpsUser@$VpsHost)" -ForegroundColor DarkGray
ssh @SshOpts "$VpsUser@$VpsHost" "rm -rf $RemoteTmp && mkdir -p $RemoteTmp"
if ($LASTEXITCODE -ne 0) { throw "SSH connection failed — check your VpsHost / password" }

# Upload all files from publish directory
scp @SshOpts -r "$PublishDir\*" "${VpsUser}@${VpsHost}:${RemoteTmp}/"
if ($LASTEXITCODE -ne 0) { throw "SCP upload of published files failed" }

# Upload deploy.sh
scp @SshOpts "deploy/deploy.sh" "${VpsUser}@${VpsHost}:${RemoteTmp}/"
if ($LASTEXITCODE -ne 0) { throw "SCP upload of deploy.sh failed" }

# --- 3. Execute deploy.sh on VPS ---
Write-Host "[3/5] Running deploy.sh on VPS..." -ForegroundColor Yellow
ssh @SshOpts "$VpsUser@$VpsHost" "chmod +x $RemoteTmp/deploy.sh && sudo $RemoteTmp/deploy.sh"
if ($LASTEXITCODE -ne 0) { throw "Remote deploy.sh failed" }

# --- 4. Cleanup local artifacts ---
Write-Host "[4/5] Cleaning up..." -ForegroundColor Yellow
Remove-Item -Recurse -Force $PublishDir

Write-Host ""
Write-Host "=== Deployment complete ===" -ForegroundColor Green
Write-Host "  Site: http://$VpsHost (http://vincipher.com)"
Write-Host ""

