param(
    # Dùng: .\scripts\ef.ps1 database update
    # Hoặc: .\scripts\ef.ps1 migrations add TenMigration
    [Parameter(ValueFromRemainingArguments = $true)][string[]]$EfArguments,
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$projectDir = Join-Path $projectRoot 'src\CafePOSManagement.Web'
$outputDir = Join-Path $projectDir "bin\$Configuration\net8.0"
$dotnetPath = Join-Path $projectRoot '.tools\dotnet\dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnetPath)) { $dotnetPath = (Get-Command dotnet -ErrorAction Stop).Source }
$packageRoot = $env:NUGET_PACKAGES
if (-not $packageRoot) {
    $packageRoot = Join-Path $projectRoot '.tools\nuget'
    if (-not (Test-Path -LiteralPath $packageRoot)) { $packageRoot = Join-Path $env:USERPROFILE '.nuget\packages' }
}
$efPath = Join-Path $packageRoot 'microsoft.entityframeworkcore.tools\8.0.33\tools\netcoreapp2.0\any\ef.dll'
$assemblyPath = Join-Path $outputDir 'CafePOSManagement.Web.dll'
if (-not (Test-Path -LiteralPath $efPath) -or -not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Chưa có EF Tools hoặc bản build $Configuration. Restore và build solution trước khi chạy script."
}
& $dotnetPath exec --runtimeconfig (Join-Path $outputDir 'CafePOSManagement.Web.runtimeconfig.json') `
    --depsfile (Join-Path $outputDir 'CafePOSManagement.Web.deps.json') $efPath @EfArguments `
    --assembly $assemblyPath --startup-assembly $assemblyPath --project-dir $projectDir `
    --project (Join-Path $projectDir 'CafePOSManagement.Web.csproj') --working-dir $projectRoot
if ($LASTEXITCODE -ne 0) { throw "EF CLI thất bại (exit code $LASTEXITCODE)." }
