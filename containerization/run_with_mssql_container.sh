#!/bin/bash

containerTool="podman"
normalColor="\033[0;32m"
errorColor="\033[0;31m"
resetColor="\033[0m"
# warningColor="Yellow"
cr_dir_path=$(dirname "$(readlink -f "$0")")
startDelay=9
appName="ogrenci_aidat_sys"
appContainerName="oais_container"
dbContainerName="mssql_container"
podName="oais_pod"
dbPort=1433
appHttpPort="8080:8080"
# appHttpsPort="8989:8989"
cleanup=false
useDb=false
useApp=false
usePod=false
noPreRm=false
rm=false
clean_build=false
appSettingsPath="$cr_dir_path/appsettings_mssql.json"
tempSettingsPath="$cr_dir_path/appsettings_temp.json"
logFilePath="$cr_dir_path/run_with_mssql_container.log"
dockerFile="$cr_dir_path/Dockerfile"
buildCPath="$cr_dir_path/.."
dbPassword=$(grep -oP "(?<=DB_PASSWORD=).*" "$cr_dir_path/.env")
dependencies=("jq" "podman")

checkDependencies() {
    for dep in "${dependencies[@]}"; do
        if ! command -v "$dep" &>/dev/null; then
            echo -e "${errorColor}$dep is not installed. Exiting.${resetColor}" >&2
            exit 1
        fi
    done
}

checkFiles() {
    if [ ! -f "$logFilePath" ]; then
        touch "$logFilePath"
    fi

    if [ ! -f "$appSettingsPath" ]; then
        echo -e "${errorColor}appsettings_mssql.json does not exist. Exiting.${resetColor}" >&2
        exit 1
    fi

    if [ -z "$dbPassword" ]; then
        echo -e "${errorColor}DB_PASSWORD is not set in .env file. Exiting.${resetColor}" >&2
        exit 1
    fi

    if [ ! -f "$dockerFile" ]; then
        echo -e "${errorColor}Dockerfile does not exist. Exiting.${resetColor}" >&2
        exit 1
    fi

    if [ ! -d "$buildCPath/OgrenciAidatSistemi" ]; then
        echo -e "${errorColor}Application source path:{$buildCPath/OgrenciAidatSistemi} does not exist. Exiting.${resetColor}" >&2
        exit 1
    fi

    if [ ! -f "$tempSettingsPath" ]; then
        echo -e "${normalColor}Temporary appsettings.json does not exist. Creating it.${resetColor}" >&2
        cp "$appSettingsPath" "$tempSettingsPath"
        json=$(jq '.ConnectionStrings.DefaultConnection |= sub("<thisIsPassword>"; "'"$dbPassword"'")' "$appSettingsPath")
        echo "$json" >"$tempSettingsPath"
    fi
}

cleanUp() {
    if [ "$noPreRm" != true ]; then
        echo -e "${normalColor}Pre-cleaning.${resetColor}" >&2
        if [ "$usePod" = true ] && $containerTool pod exists "$podName"; then
            echo -e "${normalColor}Pod exists. Removing.${resetColor}" >&2
            $containerTool pod stop "$podName"
            $containerTool pod rm "$podName" -f
        fi
        rmIfExistsContainer "$dbContainerName"
        rmIfExistsContainer "$appContainerName"
        echo -e "${normalColor}Pre-cleaning completed.${resetColor}" >&2
    fi
}

cleanBuild() {
    if [[ "$clean_build" = true ]]; then
        noPreRm=false
        cleanUp

        if [ -f "$tempSettingsPath" ]; then
            rm "$tempSettingsPath"
        fi
        if [ -f "$logFilePath" ]; then
            rm "$tempSettingsPath"
        fi
        $containerTool rmi "${appName}:latest"
        echo "${normalColor}exec:$containerTool build -t ${appName}:latest -f $dockerFile $buildCPath${resetColor}" >&2
        $containerTool build -t "${appName}:latest" -f "$dockerFile" "$buildCPath"
        noPreRm=true
    fi
}

runContainers() {
    echo -e "${normalColor}Running containers. db: $useDb, app: $useApp, pod: $usePod${resetColor}" >&2
    if [ "$usePod" = true ]; then
        runAllContainersWithPod
    else
        [ "$useDb" = true ] && runDbContainer
        [ "$useApp" = true ] && runAppContainer
    fi
}

rmIfExistsContainer() {
    local contname=$1
    echo -e "${normalColor}Checking if container exists.${resetColor}" >&2
    if [ -z "$contname" ]; then
        echo -e "${errorColor}Container name is not set. Exiting.${resetColor}" >&2
        exit 1
    fi
    if $containerTool container exists "$contname"; then
        echo -e "${normalColor}Container exists. Removing.${resetColor}" >&2
        $containerTool container stop "$contname"
        $containerTool container rm "$contname" -f
    fi
}

buildAppNotExists() {
    if ! $containerTool image inspect "${appName}:latest" &>/dev/null; then
        echo "${normalColor}exec:$containerTool build -t ${appName}:latest -f $dockerFile $buildCPath${resetColor}" >&2
        $containerTool build -t "${appName}:latest" -f "$dockerFile" "$buildCPath"
    fi
}

runAppContainer() {
    echo -e "${normalColor}Building application container.${resetColor}" >&2
    buildAppNotExists
    rmIfExistsContainer "$appContainerName"
    $containerTool run --name "$appContainerName" -p "$appHttpPort" -v "${tempSettingsPath}:/app/appsettings.json" -v "${logFilePath}:/app/app.log" -d "${appName}:latest"
    echo -e "${normalColor}Application container is running on port 8080.${resetColor}" >&2
    [ "$useDb" = true ] && sleep $startDelay
}

runAllContainersWithPod() {
    if [ "$noPreRm" != true ] && $containerTool pod exists "$podName"; then
        echo -e "${errorColor}Pod already exists and --pre-rm is not set. Exiting.${resetColor}" >&2
        exit 1
    fi
    $containerTool pod create --name "$podName" -p "$appHttpPort" -p "${dbPort}:${dbPort}" # Fix the pod creation command
    echo -e "${normalColor}Pod is created.${resetColor}" >&2
    echo -e "${normalColor}Running application and database containers with pod.${resetColor}" >&2
    rmIfExistsContainer "$dbContainerName"
    $containerTool run --pod "$podName" --name "$dbContainerName" -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$dbPassword" -d "mcr.microsoft.com/mssql/server"
    rmIfExistsContainer "$appContainerName"
    buildAppNotExists
    $containerTool run --pod "$podName" --name "$appContainerName" -v "${tempSettingsPath}:/app/appsettings.json" -v "${logFilePath}:/app/app.log" -d "${appName}:latest"
    echo -e "${normalColor}Pod is running with application and database containers.${resetColor}" >&2
}

runDbContainer() {
    echo -e "${normalColor}Running SQL Server container.${resetColor}" >&2
    rmIfExistsContainer "$dbContainerName"
    $containerTool run --name "$dbContainerName" -p "${dbPort}:${dbPort}" -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$dbPassword" -d "mcr.microsoft.com/mssql/server"
    echo -e "${normalColor}SQL Server container is running on port 1433.${resetColor}" >&2
}

# Parse script arguments
for arg in "$@"; do
    case $arg in
    --clean)
        echo "Cleaning option detected" >&2
        cleanup=true
        ;;
    --db)
        echo "DB option detected" >&2
        useDb=true
        ;;
    --app)
        echo "App option detected" >&2
        useApp=true
        ;;
    --pod)
        echo "Pod option detected" >&2
        usePod=true
        ;;
    --no-pre-rm)
        echo "No pre-rm option detected" >&2
        noPreRm=true
        ;;
    --clean-build)
        echo "clean_build option detected" >&2
        echo "BE AWARE:this function changes --no-pre-rm to false" >&2
        clean_build=true
        ;;
    *)
        echo "Unknown option: $arg"
        exit 1
        ;;
    esac
done

# Display parsed arguments
echo "Parse result: Cleanup=$cleanup, DB=$useDb, App=$useApp, Pod=$usePod, NoPreRm=$noPreRm, Rm=$rm" >&2

checkDependencies
cleanUp
cleanBuild
checkFiles
runContainers
