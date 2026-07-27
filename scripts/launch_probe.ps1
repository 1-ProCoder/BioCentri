$ErrorActionPreference = 'Stop'
$exe = 'C:\Users\Princ\BioCentri\app\BioCentri.App\bin\Debug\net8.0-windows10.0.19041.0\BioCentri.App.exe'
Write-Output "EXE: $exe"
Write-Output "Exists: $([System.IO.File]::Exists($exe))"

# Kill any stale instances first
Get-Process -Name 'BioCentri.App' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Launch
$p = Start-Process -FilePath $exe -PassThru
Write-Output "Started PID=$($p.Id)"
Start-Sleep -Seconds 8
$alive = -not $p.HasExited
if ($p.HasExited) {
    Write-Output "EXITED early with code: $($p.ExitCode)"
} else {
    Get-Process -Id $p.Id -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Output "ALIVE PID=$($_.Id) MainWindowHandle=$($_.MainWindowHandle) Responding=$($_.Responding) Title='$($_.MainWindowTitle)'"
    }
}
Write-Output "TotalBioCentriAppProcesses: $((Get-Process -Name 'BioCentri.App' -ErrorAction SilentlyContinue).Count)"

# Cleanup
Get-Process -Name 'BioCentri.App' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Output "DONE"
