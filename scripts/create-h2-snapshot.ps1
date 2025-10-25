param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$AppName,

    [Parameter(Mandatory = $false, Position = 1)]
    [string]$VolumeName = "keycloak_h2"
)

function Ensure-FlyCliExists {
    if (-not (Get-Command fly -ErrorAction SilentlyContinue)) {
        Write-Error "fly CLI non trovato nel PATH." -ErrorAction Stop
    }
}

function Get-VolumeId {
    param(
        [string]$App,
        [string]$Name
    )

    $volumes = fly volumes list --app $App | Select-String -Pattern $Name

    foreach ($volume in $volumes) {
        $parts = $volume.ToString().Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
        if ($parts[-1] -eq $Name) {
            return $parts[0]
        }
    }

    throw "Impossibile trovare un volume chiamato '$Name' per l'app '$App'."
}

Ensure-FlyCliExists

$volumeId = Get-VolumeId -App $AppName -Name $VolumeName

fly volumes snapshots create $volumeId | Out-Null

Write-Host "Snapshot creato per il volume $volumeId ($VolumeName) dell'app $AppName."
