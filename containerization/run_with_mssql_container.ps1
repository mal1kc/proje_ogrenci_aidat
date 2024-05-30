$containerTool = "podman"
$normalColor = 'Green'
$errorColor = 'Red'
# $warningColor = 'Yellow'
$cr_dir_path = Split-Path -Parent $MyInvocation.MyCommand.Definition
$startDelay = 9
$appName = "OgrenciAidatSistemi".ToLower()
$appContainerName = "oai_container"
$dbContainerName = "mssql_container"
$podName = "oai_pod"
$dbPort = 1433
$appHttpPort = "8080:8080"
# $appHttpsPort = "8989:8989"
$cleanup = $false
$useDb = $false
$useApp = $false
$usePod = $false
$noPreRm = $false
$rm = $false
$appSettingsPath = "$cr_dir_path/appsettings_mssql.json"
$tempSettingsPath = "$cr_dir_path/appsettings_temp.json"
$logFilePath = "$cr_dir_path/run_with_mssql_container.log"
$dockerFile = "$cr_dir_path/Dockerfile"
$appSrcPath = "$cr_dir_path/../OgrenciAidatSistemi"
$dbPassword = Get-Content "$cr_dir_path/.env" | Select-String "DB_PASSWORD" | ForEach-Object { $_ -replace "DB_PASSWORD=", "" }
$dependencies = @("jq", "podman")

function CheckDependencies {
    foreach ($dep in $dependencies) {
        if (!(Get-Command $dep -ErrorAction SilentlyContinue)) {
            Write-Host "$dep is not installed. Exiting." -ForegroundColor $errorColor
            exit 1
        }
    }
}

function CheckFiles {
    if (!(Test-Path $logFilePath)) {
        New-Item -ItemType file -Path $logFilePath
    }

    if (!(Test-Path $appSettingsPath)) {
        Write-Host "appsettings_mssql.json does not exist. Exiting." -ForegroundColor $errorColor
        exit 1
    }

    if ($dbPassword -eq "") {
        Write-Host "DB_PASSWORD is not set in .env file. Exiting." -ForegroundColor $errorColor
        exit 1
    }

    if (!(Test-Path $dockerFile)) {
        Write-Host "Dockerfile does not exist. Exiting." -ForegroundColor $errorColor
        exit 1
    }

    if (!(Test-Path $appSrcPath)) {
        Write-Host "Application source path does not exist. Exiting." -ForegroundColor $errorColor
        exit 1
    }

    if (!(Test-Path $tempSettingsPath)) {
        Write-Host "Temporary appsettings.json does not exist. Creating it." -ForegroundColor $normalColor
        Copy-Item -Path $appSettingsPath -Destination $tempSettingsPath
        $json = Get-Content $appSettingsPath -Raw | jq '.ConnectionStrings.DefaultConnection |= sub("<thisIsPassword>"; "' + $dbPassword + '")'
        Set-Content -Path $tempSettingsPath -Value $json
    }
}

function CleanUp {
    if (!$script:noPreRm) {
        Write-Host "Pre-cleaning." -ForegroundColor $normalColor
        if ($script:usePod -and (& $containerTool 'pod' 'exists' $podName)) {
            Write-Host "Pod exists. Removing." -ForegroundColor $normalColor
            & $containerTool 'pod' 'stop' $podName
            & $containerTool 'pod' 'rm' $podName '-f'
        }
        RmIfExistsContainer $dbContainerName
        RmIfExistsContainer $appContainerName
        Write-Host "Pre-cleaning completed." -ForegroundColor $normalColor
    }
}

function RunContainers {
    Write-Host "Running containers. db: $useDb, app: $useApp, pod: $usePod" -ForegroundColor $normalColor
    if ($usePod) {
        RunAllContainersWithPod
    } else {
        if ($useDb) {
            RunDbContainer
        }
        if ($useApp) {
            RunAppContainer
        }
    }
}


function RmIfExistsContainer {
    param(
        [string]$contname
    )
    $containerTool = $script:containerTool
    Write-Host "Checking if container exists." -ForegroundColor $normalColor
    Write-Host "exec: $containerTool container exists $contname" -ForegroundColor $normalColor
    if ($null -eq $contname) {
        Write-Host "Container name is not set. Exiting." -ForegroundColor $errorColor
        exit 1
    }
    if (& $containerTool 'container' 'exists' $contname) {
        Write-Host "Container exists. Removing." -ForegroundColor $normalColor
        & $containerTool 'container' 'stop' $contname
        & $containerTool 'container' 'rm' $contname '-f'
    }
}

function RunAppContainer {
    Write-Host "Building application container." -ForegroundColor $normalColor
    RmIfExistsContainer $appContainerName
    & $containerTool 'run' '--name' $appContainerName '-p' $appHttpPort '-v' "${tempSettingsPath}:/app/appsettings.json" '-v' "${logFilePath}:/app/app.log" '-d' "${appName}:latest"
    Write-Host "Application container is running on port 8080." -ForegroundColor $normalColor
    if ($useDb) {
        Start-Sleep -Seconds $startDelay
    }
}

function RunDbContainer {
    Write-Host "Running SQL Server container." -ForegroundColor $normalColor
    RmIfExistsContainer $dbContainerName
    & $containerTool 'run' '--name' $dbContainerName '-p' "${dbPort}:${dbPort}" '-e' "ACCEPT_EULA=Y" '-e' "SA_PASSWORD=$dbPassword" '-d' "mcr.microsoft.com/mssql/server"
    Write-Host "SQL Server container is running on port 1433." -ForegroundColor $normalColor
}

function RunAllContainersWithPod {
    if ($noPreRm -and (& $containerTool 'pod' 'exists' $podName)) {
        Write-Host "Pod already exists and --pre-rm is not set. Exiting." -ForegroundColor $errorColor
        exit 1
    } else {
        & $containerTool 'pod' 'create' '--name' $podName
        Write-Host "Pod is created." -ForegroundColor $normalColor
        Write-Host "Running application and database containers with pod." -ForegroundColor $normalColor
        RmIfExistsContainer $dbContainerName
        & $containerTool 'run' '--pod' $podName '--name' $dbContainerName '-p' "${dbPort}:${dbPort}" '-e' "ACCEPT_EULA=Y" '-e' "SA_PASSWORD=$dbPassword" '-d' "mcr.microsoft.com/mssql/server"
        RmIfExistsContainer $appContainerName
        & $containerTool 'run' '--pod' $podName '--name' $appContainerName '-p' $appHttpPort '-v' "${tempSettingsPath}:/app/appsettings.json" '-v' "${logFilePath}:/app/app.log" '-d' "${appName}:latest"
        Write-Host "Pod is running with application and database containers." -ForegroundColor $normalColor
    }
}

# Parse script arguments
foreach ($arg in $args) {
    switch ($arg) {
        "--clean" {
            Write-Output "Cleaning option detected"
            $script:cleanup = $true
        }
        "--db" {
            Write-Output "DB option detected"
            $script:useDb = $true
        }
        "--app" {
            Write-Output "App option detected"
            $script:useApp = $true
        }
        "--pod" {
            Write-Output "Pod option detected"
            $script:usePod = $true
        }
        "--no-pre-rm" {
            Write-Output "No pre-rm option detected"
            $script:noPreRm = $true
        }
        "--rm" {
            Write-Output "Rm option detected"
            $script:rm = $true
        }
        default {
            Write-Output "Unknown argument: $arg"
        }
    }
}

# Display parsed arguments
Write-Output "Script arguments: $args"
Write-Output "Parse result: Cleanup=$cleanup, DB=$useDb, App=$useApp, Pod=$usePod, NoPreRm=$noPreRm, Rm=$rm"

CheckDependencies
CheckFiles
CleanUp
RunContainers
