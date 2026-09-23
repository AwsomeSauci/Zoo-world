param(
    [string]$Unity,
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../Builds/Web'),
    [string]$LogPath = (Join-Path $PSScriptRoot '../TestResults/Web-build.log')
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$version = ((Get-Content (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') |
    Select-String '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value).Trim()
if (!$Unity) { $Unity = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe" }
if (!(Test-Path -LiteralPath $Unity -PathType Leaf)) { throw "Unity $version was not found at $Unity." }

$destination = [IO.Path]::GetFullPath($OutputDirectory)
$log = [IO.Path]::GetFullPath($LogPath)
New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($log)) -Force | Out-Null
$index = Join-Path $destination 'index.html'
if (Test-Path -LiteralPath $index) { Remove-Item -LiteralPath $index -Force }
$arguments = @('-batchmode', '-noaudio', '-quit', '-buildTarget', 'WebGL', '-projectPath', $projectRoot,
    '-executeMethod', 'ZooWorld.Editor.ZooBuild.Web', '--zoo-build-dir', $destination, '-logFile', $log)
$quoted = $arguments | ForEach-Object { '"' + $_ + '"' }
$process = Start-Process -FilePath $Unity -ArgumentList $quoted -WindowStyle Hidden -PassThru -Wait
if ($process.ExitCode -ne 0 -or !(Test-Path -LiteralPath $index -PathType Leaf)) {
    throw "Web build failed. See $log"
}
Write-Output "Web build: $destination"
