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

# --- 2. Zip ---
Write-Host "[2/5] Creating zip archive..." -ForegroundColor Yellow
if (Test-Path $ZipName) { Remove-Item -Force $ZipName }
Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipName -Force
Write-Host "  -> $ZipName ($('{0:N1} MB' -f ((Get-Item $ZipName).Length / 1MB)))"

# --- 3. Upload zip + deploy.sh to VPS ---
Write-Host "[3/5] Uploading to VPS..." -ForegroundColor Yellow
$SshOpts = @("-o", "StrictHostKeyChecking=no")

# Create remote tmp dir & clean it
Write-Host "  (enter password for $VpsUser@$VpsHost)" -ForegroundColor DarkGray
ssh @SshOpts "$VpsUser@$VpsHost" "rm -rf $RemoteTmp && mkdir -p $RemoteTmp"
if ($LASTEXITCODE -ne 0) { throw "SSH connection failed — check your VpsHost / password" }

# Upload zip
scp @SshOpts $ZipName "${VpsUser}@${VpsHost}:${RemoteTmp}/"
if ($LASTEXITCODE -ne 0) { throw "SCP upload of zip failed" }

# Upload deploy.sh
scp @SshOpts "deploy/deploy.sh" "${VpsUser}@${VpsHost}:${RemoteTmp}/"
if ($LASTEXITCODE -ne 0) { throw "SCP upload of deploy.sh failed" }

# --- 4. Execute deploy.sh on VPS ---
Write-Host "[4/5] Running deploy.sh on VPS..." -ForegroundColor Yellow
ssh @SshOpts "$VpsUser@$VpsHost" "chmod +x $RemoteTmp/deploy.sh && sudo $RemoteTmp/deploy.sh"
if ($LASTEXITCODE -ne 0) { throw "Remote deploy.sh failed" }

# --- 5. Cleanup local artifacts ---
Write-Host "[5/5] Cleaning up..." -ForegroundColor Yellow
Remove-Item -Recurse -Force $PublishDir
Remove-Item -Force $ZipName

Write-Host ""
Write-Host "=== Deployment complete ===" -ForegroundColor Green
Write-Host "  Site: http://$VpsHost (http://vincipher.com)"
Write-Host ""
