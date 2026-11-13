# Script khởi động nhanh CafePOS
# Chạy: .\run.ps1
# Dừng: Ctrl+C
param(
    [switch]$Build,                   # Thêm -Build để build trước khi chạy
    [string]$Profile = "http"        # Launch profile mặc định: http (port 5088)
)
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

$dotnet = Join-Path $root ".tools\dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) { $dotnet = (Get-Command dotnet -ErrorAction Stop).Source }

if (-not ((& $dotnet --list-runtimes) -match '^Microsoft\.AspNetCore\.App 8\.')) {
    throw "Thiếu ASP.NET Core Runtime 8.x (x64). Cài .NET 8 SDK hoặc ASP.NET Core Runtime 8 rồi chạy lại."
}

$env:DOTNET_CLI_HOME = Join-Path $root ".tools\cli-home"
$env:NUGET_PACKAGES  = Join-Path $root ".tools\nuget"

if ($Build) {
    Write-Host "▶ Building..." -ForegroundColor Cyan
    & $dotnet build (Join-Path $root "CafePOSManagement.sln")
    if ($LASTEXITCODE -ne 0) { throw "Build thất bại." }
}

Write-Host "▶ Starting CafePOS on http://localhost:5088 ..." -ForegroundColor Green
$runArguments = @('run', '--project', (Join-Path $root 'src\CafePOSManagement.Web'), '--launch-profile', $Profile)
if ($Build) { $runArguments += '--no-build' }
& $dotnet @runArguments
if ($LASTEXITCODE -ne 0) { throw "CafePOS dừng với mã lỗi $LASTEXITCODE." }
