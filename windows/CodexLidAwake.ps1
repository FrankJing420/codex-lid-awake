#Requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Enable', 'Disable', 'Restore', 'Status')]
    [string]$Action = 'Enable',

    [ValidateRange(1, 24)]
    [int]$Hours = 2
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$SubgroupGuid = '4f971e89-eebd-4455-a8de-9e59040e7347' # Power buttons and lid
$LidActionGuid = '5ca83367-6e45-459f-a27b-476b1d01c936'
$StateDirectory = Join-Path $env:ProgramData 'CodexLidAwake'
$StatePath = Join-Path $StateDirectory 'state.json'
$InstalledScript = Join-Path $StateDirectory 'CodexLidAwake.ps1'
$RestoreTaskName = 'CodexLidAwake-Restore'

function Assert-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Run PowerShell as Administrator, then run this script again.'
    }
}

function Get-ActiveSchemeGuid {
    $result = (& powercfg.exe /getactivescheme | Out-String)
    $match = [regex]::Match($result, '[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}')
    if (-not $match.Success) {
        throw 'Could not determine the active Windows power scheme.'
    }
    return $match.Value
}

function Get-LidActionValues {
    param([Parameter(Mandatory)] [string]$SchemeGuid)

    # Query powercfg instead of the registry: some valid power plans inherit this
    # setting and therefore do not have an explicit registry key.
    $result = (& powercfg.exe /query $SchemeGuid $SubgroupGuid $LidActionGuid | Out-String)
    if ($LASTEXITCODE -ne 0) {
        throw "powercfg could not read the lid-close setting (exit code $LASTEXITCODE)."
    }

    # The final two hexadecimal indexes in this setting-specific query are always
    # the current AC and DC values. This avoids depending on localized labels.
    $indexes = [regex]::Matches($result, '(?i)\b0x([0-9a-f]+)\b')
    if ($indexes.Count -lt 2) {
        throw 'powercfg returned an unexpected lid-close setting format.'
    }

    return [pscustomobject]@{
        AcValue = [Convert]::ToUInt32($indexes[$indexes.Count - 2].Groups[1].Value, 16)
        DcValue = [Convert]::ToUInt32($indexes[$indexes.Count - 1].Groups[1].Value, 16)
    }
}

function Set-LidActionValue {
    param(
        [Parameter(Mandatory)] [string]$SchemeGuid,
        [Parameter(Mandatory)] [uint32]$AcValue,
        [Parameter(Mandatory)] [uint32]$DcValue
    )

    & powercfg.exe /setacvalueindex $SchemeGuid $SubgroupGuid $LidActionGuid $AcValue
    if ($LASTEXITCODE -ne 0) { throw "powercfg failed while setting the AC lid action (exit code $LASTEXITCODE)." }

    & powercfg.exe /setdcvalueindex $SchemeGuid $SubgroupGuid $LidActionGuid $DcValue
    if ($LASTEXITCODE -ne 0) { throw "powercfg failed while setting the battery lid action (exit code $LASTEXITCODE)." }
}

function Set-ActiveSchemeIfStillCurrent {
    param([Parameter(Mandatory)] [string]$SavedSchemeGuid)

    if ((Get-ActiveSchemeGuid) -eq $SavedSchemeGuid) {
        & powercfg.exe /setactive $SavedSchemeGuid
        if ($LASTEXITCODE -ne 0) { throw "powercfg failed while reapplying the active scheme (exit code $LASTEXITCODE)." }
    }
}

function Remove-RecoveryTask {
    Unregister-ScheduledTask -TaskName $RestoreTaskName -Confirm:$false -ErrorAction SilentlyContinue
}

function Install-RecoveryTask {
    param([Parameter(Mandatory)] [datetime]$RestoreAt)

    Copy-Item -LiteralPath $PSCommandPath -Destination $InstalledScript -Force
    $taskArguments = "-NoProfile -ExecutionPolicy Bypass -File `"$InstalledScript`" -Action Restore"
    $taskAction = New-ScheduledTaskAction -Execute 'PowerShell.exe' -Argument $taskArguments
    $timeTrigger = New-ScheduledTaskTrigger -Once -At $RestoreAt
    $startupTrigger = New-ScheduledTaskTrigger -AtStartup

    Register-ScheduledTask -TaskName $RestoreTaskName -Action $taskAction -Trigger @($timeTrigger, $startupTrigger) -User 'SYSTEM' -RunLevel Highest -Force | Out-Null
}

function Restore-LidAction {
    if (-not (Test-Path -LiteralPath $StatePath)) {
        Remove-RecoveryTask
        return
    }

    $state = Get-Content -LiteralPath $StatePath -Raw | ConvertFrom-Json
    Set-LidActionValue -SchemeGuid $state.SchemeGuid -AcValue ([uint32]$state.AcValue) -DcValue ([uint32]$state.DcValue)
    Set-ActiveSchemeIfStillCurrent -SavedSchemeGuid $state.SchemeGuid
    Remove-Item -LiteralPath $StatePath -Force
    Remove-RecoveryTask
    Write-Host 'Normal lid-close behavior has been restored.'
}

switch ($Action) {
    'Status' {
        if (Test-Path -LiteralPath $StatePath) {
            $state = Get-Content -LiteralPath $StatePath -Raw | ConvertFrom-Json
            Write-Host "Active until $($state.RestoreAt) (local time)."
        }
        else {
            Write-Host 'Not active.'
        }
        break
    }

    'Enable' {
        Assert-Administrator
        if (Test-Path -LiteralPath $StatePath) {
            throw "Codex Lid Awake is already active. Run with -Action Disable to restore it first."
        }

        $schemeGuid = Get-ActiveSchemeGuid
        $savedValues = Get-LidActionValues -SchemeGuid $schemeGuid
        $acValue = $savedValues.AcValue
        $dcValue = $savedValues.DcValue
        $restoreAt = (Get-Date).AddHours($Hours)
        $state = [pscustomobject]@{
            SchemeGuid = $schemeGuid
            AcValue = $acValue
            DcValue = $dcValue
            RestoreAt = $restoreAt.ToString('o')
        }

        New-Item -ItemType Directory -Path $StateDirectory -Force | Out-Null
        $state | ConvertTo-Json | Set-Content -LiteralPath $StatePath -Encoding UTF8
        Install-RecoveryTask -RestoreAt $restoreAt

        try {
            # 0 means "Do nothing" for the lid-close power action.
            Set-LidActionValue -SchemeGuid $schemeGuid -AcValue 0 -DcValue 0
            Set-ActiveSchemeIfStillCurrent -SavedSchemeGuid $schemeGuid
            Write-Host "Enabled. Closing the lid will keep Windows awake until $restoreAt."
        }
        catch {
            Restore-LidAction
            throw
        }
        break
    }

    'Disable' {
        Assert-Administrator
        Restore-LidAction
        break
    }

    'Restore' {
        Restore-LidAction
        break
    }
}
