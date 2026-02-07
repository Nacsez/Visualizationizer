param(
    [switch]$ProbeLinux
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

Push-Location $repoRoot
try {
    Write-Host "Publishing Windows x64 single-file release..."
    dotnet publish Visualizationizer1.0.csproj `
        -c Release `
        -r win-x64 `
        --self-contained true `
        /p:PublishSingleFile=true `
        -o bin\Release\net6.0-windows\win-x64\publish

    Write-Host "Windows publish complete."
    Write-Host "Output: bin\\Release\\net6.0-windows\\win-x64\\publish"

    if ($ProbeLinux) {
        Write-Host "Probing Linux native publish support..."
        $linuxLogDir = Join-Path $repoRoot "artifacts"
        New-Item -ItemType Directory -Force -Path $linuxLogDir | Out-Null
        $linuxLogPath = Join-Path $linuxLogDir "linux-publish-probe.log"

        dotnet publish Visualizationizer1.0.csproj `
            -c Release `
            -r linux-x64 `
            --self-contained true `
            /p:PublishSingleFile=true `
            -o bin\Release\net6.0-windows\linux-x64\publish *>&1 | Tee-Object -FilePath $linuxLogPath
        $probeSucceeded = ($LASTEXITCODE -eq 0)

        if ($probeSucceeded) {
            Write-Host "Linux publish succeeded."
        }
        else {
            Write-Warning "Linux publish probe failed. See artifacts\\linux-publish-probe.log."
            Write-Host "Current project is windows-targeted and may require refactor for Linux support."
        }
    }
}
finally {
    Pop-Location
}
