param(
    [Parameter(Mandatory = $true)][string]$UnityPath,
    [ValidateSet('EditMode','PlayMode','Both')][string]$Mode = 'Both'
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$output = Join-Path $repo ('artifacts/tests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $output -Force | Out-Null
$modes = if ($Mode -eq 'Both') { @('EditMode','PlayMode') } else { @($Mode) }
foreach ($testMode in $modes) {
    $xml = Join-Path $output "$testMode.xml"
    $log = Join-Path $output "$testMode.log"
    $arguments = @('-batchmode','-nographics','-projectPath',('"' + $repo + '"'),
        '-runTests','-testPlatform',$testMode,'-testResults',('"' + $xml + '"'),'-logFile',('"' + $log + '"'))
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (!$process.WaitForExit(600000)) { throw "Unity test run timed out; inspect $log and process $($process.Id)." }
    if (!(Test-Path -LiteralPath $xml)) { throw "No test report; inspect $log" }
    [xml]$result = Get-Content -LiteralPath $xml
    if ($process.ExitCode -ne 0 -or $result.'test-run'.result -ne 'Passed') { throw "Tests failed; inspect $xml and $log" }
    Write-Output "$testMode passed: $($result.'test-run'.passed) tests. Report: $xml"
}
