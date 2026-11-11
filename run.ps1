# Script khởi động nhanh CafePOS (dùng .NET 8 cục bộ trong .tools/dotnet)
# Chạy: .\run.ps1
# Dừng: Ctrl+C
param(
    [switch]$Build,                   # Thêm -Build để build trước khi chạy
    [string]$Profile = "http"        # Launch profile mặc định: http (port 5088)
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
if ($PSScriptRoot -eq (Get-Location).Path) { $root = $PSScriptRoot }

# Nếu chạy từ thư mục gốc dự án
$root = (Get-Location).Path

$dotnet = Join-Path $root ".tools\dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) { $dotnet = (Get-Command dotnet -ErrorAction Stop).Source }

$env:DOTNET_CLI_HOME = Join-Path $root ".tools\cli-home"
$env:NUGET_PACKAGES  = Join-Path $root ".tools\nuget"

if ($Build) {
    Write-Host "▶ Building..." -ForegroundColor Cyan
    & $dotnet build (Join-Path $root "CafePOSManagement.sln") --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build thất bại." }
}

Write-Host "▶ Starting CafePOS on http://localhost:5088 ..." -ForegroundColor Green
& $dotnet run --project (Join-Path $root "src\CafePOSManagement.Web") --no-build --launch-profile $Profile
