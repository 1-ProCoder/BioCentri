$ErrorActionPreference = "Continue"
function Step($m) { Write-Host "`n=== $m ===" -ForegroundColor Cyan }

Set-Location "C:\Users\Princ\BioCentri"

Step "Kill stale BioCentri processes"
Get-Process -Name "BioCentri*" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3
$alive = (Get-Process -Name "BioCentri*" -ErrorAction SilentlyContinue | Measure-Object).Count
Write-Host ("alive after kill: " + $alive)

Step "Wipe bin/obj for all 3 projects"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue `
    app\BioCentri.App\bin, app\BioCentri.App\obj, `
    app\BioCentri.Core\bin, app\BioCentri.Core\obj, `
    app\BioCentri.Tests\bin, app\BioCentri.Tests\obj
Write-Host "wiped"

Step "Clean + restore + build"
dotnet clean app\BioCentri.sln -c Debug --nologo 2>&1 | Select-Object -Last 2
dotnet restore app\BioCentri.sln --nologo 2>&1 | Select-Object -Last 2
dotnet build app\BioCentri.sln -c Debug --nologo 2>&1 | Select-Object -Last 8

Step "Recheck post-clean BAML"
$baml = "app\BioCentri.App\obj\Debug\net8.0-windows10.0.19041.0\src\components\auth\AuthenticationOverlay.baml"
if (Test-Path $baml) {
    $hits = Select-String -Path $baml -Pattern "AuthRoot" -ErrorAction SilentlyContinue | Measure-Object
    Write-Host ("AuthRoot hits in regenerated BAML: " + $hits.Count)
    $tgt = Select-String -Path $baml -Pattern "TargetName" -ErrorAction SilentlyContinue
    if ($tgt) { Write-Host "--- TargetName references in BAML ---"; $tgt | ForEach-Object { Write-Host $_.Line } } else { Write-Host "no TargetName references in BAML" }
} else {
    Write-Host "BAML missing -- investigate"
}

Step "Test suite"
dotnet test app\BioCentri.sln --nologo --no-build 2>&1 | Select-String -Pattern "Passed!|Failed!" | Select-Object -First 3

Step "Wipe sidecar logs + launch EXE"
Get-Process -Name "BioCentri*" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Remove-Item "$env:TEMP\biocentri-xaml-error-*.log" -ErrorAction SilentlyContinue
$exe = "C:\Users\Princ\BioCentri\app\BioCentri.App\bin\Debug\net8.0-windows10.0.19041.0\BioCentri.App.exe"
Write-Host ("EXE: " + $exe + " exists: " + (Test-Path $exe))
if (-not (Test-Path $exe)) { exit 1 }
$p = Start-Process -FilePath $exe -PassThru
Write-Host ("Started PID=" + $p.Id)
Start-Sleep -Seconds 10
Get-Process -Name "BioCentri.App" -ErrorAction SilentlyContinue |
    Select-Object Id, MainWindowTitle, MainWindowHandle, Responding | Format-List | Out-String | Write-Host

Step "Sidecar log check"
$logs = Get-ChildItem "$env:TEMP\biocentri-xaml-error-*.log" -ErrorAction SilentlyContinue
if ($logs) {
    Write-Host "FOUND sidecar log(s):" -ForegroundColor Yellow
    foreach ($l in $logs) {
        Write-Host $l.FullName
        Get-Content $l.FullName | Out-String | Write-Host
    }
} else {
    Write-Host "NONE -- XAML parse succeeded with clean rebuild" -ForegroundColor Green
}

Step "Cleanup processes"
Get-Process -Name "BioCentri*" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "cleaned"
