param(
    [string]$Unity,
    [ValidateSet('All', 'EditMode', 'PlayMode')][string]$Suite = 'All',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../TestResults')
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$version = ((Get-Content (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') |
    Select-String '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value).Trim()
if (!$Unity) { $Unity = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe" }
if (!(Test-Path -LiteralPath $Unity -PathType Leaf)) { throw "Unity $version was not found at $Unity." }

$destination = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$platforms = if ($Suite -eq 'All') { @('EditMode', 'PlayMode') } else { @($Suite) }
foreach ($platform in $platforms) {
    $reportPath = Join-Path $destination "$platform.xml"
    $logPath = Join-Path $destination "$platform.log"
    if (Test-Path -LiteralPath $reportPath) { Remove-Item -LiteralPath $reportPath -Force }
    $arguments = @('-batchmode', '-noaudio', '-projectPath', $projectRoot, '-logFile', $logPath,
        '-runTests', '-testPlatform', $platform, '-assemblyNames', "ZooWorld.$($platform)Tests",
        '-testResults', $reportPath)
    if ($platform -eq 'EditMode') { $arguments += '-nographics' }
    $quoted = $arguments | ForEach-Object { '"' + $_ + '"' }
    $process = Start-Process -FilePath $Unity -ArgumentList $quoted -WindowStyle Hidden -PassThru -Wait
    if ($process.ExitCode -ne 0) { throw "Unity tests failed. See $logPath" }
    if (!(Test-Path -LiteralPath $reportPath -PathType Leaf)) { throw "Missing test report: $reportPath" }
    [xml]$report = Get-Content -LiteralPath $reportPath -Raw
    $result = $report.'test-run'
    if ($result.result -ne 'Passed' -or [int]$result.total -le 0 -or
        [int]$result.passed -ne [int]$result.total -or [int]$result.skipped -ne 0) {
        throw "$platform did not pass. See $reportPath"
    }
    Write-Output "$platform : $($result.passed)/$($result.total) passed"
}
