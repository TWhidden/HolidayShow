#!/bin/bash

# Common scripts functions and variables

RED='\033[0;31m'        # ERROR
BLUE='\033[0;34m'       # WARNING
MAGENTA='\033[0;35m'    # QUESTION
GREEN='\033[0;32m'      # SUCCESS
YELLOW='\033[0;33m'     # INFO / DEFAULT
GREY='\033[1;30m'       # DEBUG
NC='\033[0m'            # NORMAL COLOR

# Common Paths
storageRoot="/storage/docker"
hostId=$(hostname)
sourcePath=$(pwd);
dockerComposePath=/usr/local/bin/docker-compose

# Used for whiptail
NEWT_COLORS_3='
    root=white,black
    border=black,lightgray
    window=lightgray,lightgray
    shadow=black,gray
    title=black,lightgray
    button=black,cyan
    actbutton=white,cyan
    compactbutton=black,lightgray
    checkbox=black,lightgray
    actcheckbox=lightgray,cyan
    entry=black,lightgray
    disentry=gray,lightgray
    label=black,lightgray
    listbox=black,lightgray
    actlistbox=black,cyan
    sellistbox=lightgray,black
    actsellistbox=lightgray,black
    textbox=black,lightgray
    acttextbox=black,cyan
    emptyscale=,gray
    fullscale=,cyan
    helpline=white,black
    roottext=lightgrey,black
'

# Set the whiptail colors
export NEWT_COLORS=$NEWT_COLORS_3

# Set +e will allow functions to return non-zero values which is used widely here
# this must be set for this to work (see versionCompare)
set +e

# Usage: message "my message" GREEEN
#        message "my message"
message(){
  echo -e -n "${YELLOW}${2}${1}${NC}\n"
}

# Wait for a file to appear
# Usage: waitForFile filepath
function waitForFile {
  while [ ! -f "${1}" ]
  do sleep 1; echo -en "${BYellow}.${Color_Off}"
  done
  echo ""
}

# Usage: containsElement "search string" arrayName
# Note - we are passing the array name, not its contents
# this will be pulled inside the function
# Use variable CONTAINS_RESULT (1 yes, 0 no) to determine result
function containsElement() {
    search="$1"
    name=$2[@]
    a=(${!name})
    for str in "${a[@]//\"/}"; do
        #message "Search: ${str} === ${search}"
        if [ "${str}" == "${search}" ]; then
            #message "Found"
            CONTAINS_RESULT=1
            return 0;
        fi
    done
    CONTAINS_RESULT=0
}

# Whiptail - Collect information here
# Usage: defaultSelected Value1
function defaultSelected {
    if [ "${1}" == "1" ]; then
        echo "on"
    else
        echo "off"
    fi
}

# Whiptail - match strings to turn on or off
# Usage: defaultSelectedMatch Value1 Value2
function defaultSelectedMatch {
    if [ "${1}" == "${2}" ]; then
        echo "on"
    else
        echo "off"
    fi
}

# For use with Whiptail - Marks if something is selected or not
# Usage: versionSelected Value1 Value2
function versionSelected {
    if [ "${1}" == "${2}" ]; then
        echo "on"
    else
        echo "off"
    fi
}

# Test an IP address for validity:
# Usage:
#      valid_ip IP_ADDRESS
#      if [[ $? -eq 0 ]]; then echo good; else echo bad; fi
#   OR
#      if valid_ip IP_ADDRESS; then echo good; else echo bad; fi
#
function valid_ip()
{
    local  ip=$1
    local  stat=1

    if [[ $ip =~ ^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$ ]]; then
        OIFS=$IFS
        IFS='.'
        ip=($ip)
        IFS=$OIFS
        [[ ${ip[0]} -le 255 && ${ip[1]} -le 255 \
            && ${ip[2]} -le 255 && ${ip[3]} -le 255 ]]
        stat=$?
    fi
    return $stat
}

# Prompt user for a numeric value
# Usage:
# promptNumber "my Title" "My question" "default value"
# return 0 for success
# Variables set:
# PROMPT_RESULT
function promptNumber() {
  if [ $# -ne 3 ]; then
    echo "Usage: promptNumber \"title\" \"question\" \"default value\""
    return 1
  fi

  local title="$1"
  local question="$2"
  local default_value="$3"

  PROMPT_RESULT=$(whiptail --title "$title" --inputbox "$question" 10 60 "$default_value" 3>&1 1>&2 2>&3)

  # Ensure that the result is a number
  if ! [[ $PROMPT_RESULT =~ ^[0-9]+$ ]]; then
    echo "The input must be a numeric value."
    PROMPT_RESULT=""
    return 2
  fi

  return 0
}

# Prompt user for an alphanumeric value
# Usage:
# promptInput "my Title" "My question" "default value" REQUIRED
# If the fourth argument is "REQUIRED", the input must be a non-empty string.
# return 0 for success
# Variables set:
# PROMPT_RESULT
function promptInput() {
  if [ $# -lt 3 ] || [ $# -gt 4 ]; then
    echo "Usage: promptInput \"title\" \"question\" \"default value\" REQUIRED"
    return 1
  fi

  local title="$1"
  local question="$2"
  local default_value="$3"
  local required=""

  if [ $# -eq 4 ] && [ "$4" == "REQUIRED" ]; then
    required="REQUIRED"
  fi

  PROMPT_RESULT=$(whiptail --title "$title" --inputbox "$question" 10 60 "$default_value" 3>&1 1>&2 2>&3)

  if [ -n "$required" ] && [ -z "$PROMPT_RESULT" ]; then
    message "The input is required and must be a non-empty value." $RED
    PROMPT_RESULT=""
    return 2
  fi

  return 0
}


# Prompt user with a yes/no question
# Usage:
# promptUserYesNo "my Title" "My question"
# Returns 0 = yes, 1 = no
function promptUserYesNo() {

    title=${1}
    body=${2}
    
    if (whiptail --title "${title}" --yesno "${body}" 8 78); then
        message "User selected Yes, exit status was $?."
        return 0;
    else
        message "User selected No, exit status was $?."
        return 1;
    fi
}

# Compare two version strings [$1: version string 1 (v1), $2: version string 2 (v2)]
# Return values:
#   0: v1 == v2
#   1: v1 > v2
#   2: v1 < v2
# Based on: https://stackoverflow.com/a/4025065 by Dennis Williamson
# Idea from Stack Overflow: https://stackoverflow.com/questions/4023830/how-to-compare-two-strings-in-dot-separated-version-format-in-bash
function versionCompare() {

    # Trivial v1 == v2 test based on string comparison
    [[ "$1" == "$2" ]] && return 0

    # Local variables
    local regex="^(.*)-r([0-9]*)$" va1=() vr1=0 va2=() vr2=0 len i IFS="."

    # Split version strings into arrays, extract trailing revisions
    if [[ "$1" =~ ${regex} ]]; then
        va1=(${BASH_REMATCH[1]})
        [[ -n "${BASH_REMATCH[2]}" ]] && vr1=${BASH_REMATCH[2]}
    else
        va1=($1)
    fi
    if [[ "$2" =~ ${regex} ]]; then
        va2=(${BASH_REMATCH[1]})
        [[ -n "${BASH_REMATCH[2]}" ]] && vr2=${BASH_REMATCH[2]}
    else
        va2=($2)
    fi

    # Bring va1 and va2 to same length by filling empty fields with zeros
    (( ${#va1[@]} > ${#va2[@]} )) && len=${#va1[@]} || len=${#va2[@]}
    for ((i=0; i < len; ++i)); do
        [[ -z "${va1[i]}" ]] && va1[i]="0"
        [[ -z "${va2[i]}" ]] && va2[i]="0"
    done

    # Append revisions, increment length
    va1+=($vr1)
    va2+=($vr2)
    len=$((len+1))

    # *** DEBUG ***
    #echo "TEST: '${va1[@]} (?) ${va2[@]}'"

    # Compare version elements, check if v1 > v2 or v1 < v2
    for ((i=0; i < len; ++i)); do
        if (( 10#${va1[i]} > 10#${va2[i]} )); then
            return 1
        elif (( 10#${va1[i]} < 10#${va2[i]} )); then
            return 2
        fi
    done

    # All elements are equal, thus v1 == v2
    return 0
}

# Find text between two chars.
# Usage:  textBetween "My String Is Here" "My" "Is"
# Output Var: INBETWEEN
function textBetween(){
    INBETWEEN=$(echo "$1" | grep -o -P "(?<=$2).*(?=$3)")
}

# USAGE:
# whipTailSelection INPUT_MESSAGE OUTPUT_VALUE_YES OUTPUT_VALUE_NO WHIPTAIL_SELECTION_NAME WHIPTAIL_SELECTION_ARRAY
# RETURNS R1 (1 or 0)
# R2 as Yes/No message passesd in
function whipTailSelection() {
    inputMessage="${1}";
    outputYes="${2}";
    outputNo="${3}";
    containsElement "${4}" ${5}
    if [ ${CONTAINS_RESULT} -eq 1 ]; then
        message "  - ${inputMessage} ON" $GREY
        R1=1;
        R2=${outputYes}
    else
        message "  - ${inputMessage} OFF" $GREY
        R1=0;
        R2=${outputNo}
    fi
}

# Simple function to login docker using our support creds
function eConnectDockerLogin(){

    message "===== Setting up Docker Logins =====" $YELLOW
    echo "bQEf2BgXfsWWox28MjUD" | docker login docker.econnect.tv -u support --password-stdin

    errorCheck $? "Incorrect Login for docker.econnect.tv; Can't Proceed!"
    message "===== Docker Login Success =====" $GREEN
}

# Check the return value of a command. If not 0, exit the program with an error message
# Usage: errorCheck $? "Could not read file"
function errorCheck(){
    returnCode=${1}
    message=${2}

    if [ ${returnCode} != 0 ]; then
        # Login was nto successful can't procced with the install
        message ""
        message "=================================================================" $RED
        message "\t${message}" $RED
        message "=================================================================" $RED
        message ""
        exit 1
    fi
}

# Determine if a GPU is available on the system
# Sets variable GPU_ACELL=1 if true, otherwise 0
# Usage: detectGpu
function detectGpu(){
    # Global Functions
    GPU_ACCEL=0
    if [ $(lspci | grep -i nvidia |grep -c -E 'VGA|3D') -gt 0 ]; then
        GPU_ACCEL=1
        message "${1}"
    else
        message "${2}" $BLUE
    fi
}

# Find the host name for a service. used for single services like postgres
function findMeshServiceHostName(){
    HOST_NAME=""
    # Loop over each server in mesh
    for row in $(docker exec ec-mesh ec-mesh-config -m ids -s host | jq -c -r '.[]'); do

        PAYLOAD=$(docker exec ec-mesh ec-mesh-config -s container -h ${row} -c $1)
        if [ $? != 0 ]; then
            continue;
        fi

        HOST_NAME="${row}"
        return 0;
    done
    return 1;
}

# Decrypt a secure pass
function ecDecrypt(){
    DECRYPTED=$(docker run --rm -v /storage/docker:/host-storage:ro docker.econnect.tv/docker/iotubes:debian-x86_64 ec-decrypt $1)
}

# Return the first IP of this server
function getServerFirstIp(){
    thisIps=$(hostname -I)
    stringarray=($thisIps)
    THIS_SERVER_IP_ADDRESS=${stringarray[0]};
}

# Gets the Postgres connection from mesh
# If no params, default is ec-fr-postgres with user admin (ecadmin)
# First Param: container Name
# Second Param: user level (admin or client)
# Thrid Param: Db Name
# Forth Param: Pg Port
function getPostgresConnection(){

    cName="${1}"
    cUserLevel="${2}"
    cDb="${3}"
    cPort="${4}"

    thisIps=$(hostname -I)
    stringarray=($thisIps)

    if [ "${POSTGRES_SERVER_ADDRESS}" == "" ]; then
        POSTGRES_SERVER_ADDRESS=${stringarray[0]}
    fi

    containerName="ec-fr-postgres"
    userLevel="admin"
    PG_DATABASE="facialrec"
    PG_PORT=5432
    
    if [ "${cName}" != "" ]; then
        containerName="${cName}";
    fi

    if [ "${cUserLevel}" == "client" ]; then
        userLevel="client";
    fi

    if [ "${cDb}" != "" ]; then
        PG_DATABASE="${cDb}";
    fi

    if [ "${cPort}" != "" ]; then
        PG_PORT="${cPort}";
    fi

    PROMPT_TITLE="Pg Ip ${containerName}/${PG_DATABASE}";

    #message "Get PG connection details for container ${containerName} with userlevel ${userLevel} for database ${PG_DATABASE}";

#    if [ "${SERVER_MASTER}" != "1" ]; then
        
        promptIPaddress "${PROMPT_TITLE}" "PostGres Db IP" "${POSTGRES_SERVER_ADDRESS}"

        POSTGRES_SERVER_ADDRESS=${IP_ADDRESS_TO_USE}
        POSTGRES_SERVER_ADDRESS_LOCAL=${IP_ADDRESS_LOCAL}

    # else
        
    #     # Validate the server address 
    #     # some systems may have the .env file holding a bad
    #     # address when it was first setup onsite at the eConnect office
    #     # or an IP changed. 
    #     # If the IP does not exist in the local array of IPs, 
    #     # prompt the user to validate it.

    #     containsElement "${POSTGRES_SERVER_ADDRESS}" stringarray

    #     if [ ${CONTAINS_RESULT} -eq 0 ]; then
    #         promptIPaddress "${PROMPT_TITLE}" "PostGres Db IP" "${POSTGRES_SERVER_ADDRESS}"

    #         POSTGRES_SERVER_ADDRESS=${IP_ADDRESS_TO_USE}
    #         POSTGRES_SERVER_ADDRESS_LOCAL=${IP_ADDRESS_LOCAL}
    #     else
    #         POSTGRES_SERVER_ADDRESS_LOCAL=1
    #     fi

    # fi
    # Build the postgres address using the collected information
    if [ ${POSTGRES_SERVER_ADDRESS_LOCAL} -eq 1 ]; then
        PG_CONTAINER_ADDRESS="${containerName}";
        postgresPostFix="";
    else
        PG_CONTAINER_ADDRESS="${containerName}.docker";
        postgresPostFix="?sslmode=verify-ca";
    fi
    #message "getting pass from iotubes"

    #pgPass=$(docker run --rm -v ${storageRoot}:/host-storage:ro docker.econnect.tv/docker/iotubes:debian-x86_64 ec-pass-decrypt ec-fr-postgres ecadmin)
    findMeshServiceHostName ${containerName}
    errorCheck $? "Could not find postgres server!"
    #message "Database server host name: ${HOST_NAME}"
    PG_USERNAME=$(docker exec ec-mesh ec-mesh-config -s container -h ${HOST_NAME} -c ${containerName} -k "${userLevel}" | jq -r .user)
    errorCheck $? "Failed getting username for "${userLevel}"!"
    securePass=$(docker exec ec-mesh ec-mesh-config -s container -h ${HOST_NAME} -c ${containerName} -k "${userLevel}" | jq -r .securePass)
    errorCheck $? "Failed getting secure pass from mesh!"
    ecDecrypt $securePass
    errorCheck $? "Failed decrypting pass!"
    PG_PASS="${DECRYPTED}"
    POSTGRES_SERVER_CONNECTION_STRING="postgres://${PG_USERNAME}:${PG_PASS}@$PG_CONTAINER_ADDRESS:${PG_PORT}/${PG_DATABASE}${postgresPostFix}"
    #message "Connection String: ${POSTGRES_SERVER_CONNECTION_STRING}" $YELLOW
}

# Prompt for an IP address. 
# usage: promptIPaddress "title" "question" "default value"
# variables set:
# IP_ADDRESS_TO_USE (string)
# IP_ADDRESS_LOCAL (0 = false, 1 = true)
function promptIPaddress(){

    PROMPT_TITLE=${1}
    PROMPT_BODY=${2}
    PROMPT_DEFAULT=${3}

    thisIps=$(hostname -I)
    stringarray=($thisIps)

    while true; do
        IP_ADDRESS_TO_USE=`whiptail --inputbox "${PROMPT_BODY}" 10 39 "${PROMPT_DEFAULT}" --title "${PROMPT_TITLE}" 3>&1 1>&2 2>&3`
        errorCheck $? "Cancelled IP Info"
        
        # If no user input, use the first one the list (default)
        if [ "${IP_ADDRESS_TO_USE}" == "" ]; then
            IP_ADDRESS_TO_USE=${stringarray[0]}
            message "Using ${IP_ADDRESS_TO_USE}"
        else
            message "User Entered: [${IP_ADDRESS_TO_USE}]" $GREEN
        fi

        #Validate the address
        valid_ip "${IP_ADDRESS_TO_USE}"
        if [[ $? -eq 0 ]]; then
            IP_VALID=1
        else
            message "Invalid IP address. Try again" $RED
            IP_ADDRESS_TO_USE=${stringarray[0]}
            IP_VALID=0
            continue
        fi
            
        containsElement "${IP_ADDRESS_TO_USE}" stringarray
        if [ ${CONTAINS_RESULT} -eq 1 ]; then
            # Local
            IP_ADDRESS_LOCAL=1
        else
            # remote
            message "Using Remote Ip Address" $YELLOW
            IP_ADDRESS_LOCAL=0
        fi

        if [ ${IP_VALID} == 1 ]; then
            break;  
        fi 
    done

}


function ContainerStop(){
    if [ $(docker ps -a --format '{{.Names}}' | grep -E "^${1}$" -c) -eq 1 ]; then
        message " + Stopping Container ${1}"
        docker stop "${1}"
        errorCheck $? "Could stop container '${1}'"
    fi
}

function ContainerRemove(){
    if [ $(docker ps -a --format '{{.Names}}' | grep -E "^${1}$" -c) -eq 1 ]; then
        message " + Removing Container ${1}"
        docker rm "${1}"
        errorCheck $? "Could remove container '${1}'"
    fi
}

function ContainerStopAndRemove(){
    ContainerStop "${1}"
    ContainerRemove "${1}"
}


# Ensure that the eConnect network is configuered the way we desire it to be setup
# Usage: ensureDockerNetwork econnect
# Note - currently does not accept ip/network params - needs to be added
function ensureDockerNetwork(){
    networkName=$1
    if [ $(docker network ls --format '{{.Name}}' | grep -E "^${networkName}$" -c) -eq 0 ]; then

        message " + Setting up Docker private docker network '${networkName}'"

        docker network create \
        --driver=bridge \
        --subnet=172.21.0.0/16 \
        --ip-range=172.21.1.0/24 \
        --gateway=172.21.0.1 \
        ${networkName}

        errorCheck $? "Could not create docker network `${networkName}`"
    fi
}

# Check if a container exists. 
# Usage: containerExists containerName
# returns 0 (yes), 1 (no)
function containerExists(){
    c=$1
    if [ $(docker ps -a --format '{{.Names}}' | grep -E "^${c}$" -c) -eq 1 ]; then
        return 0;
    else
        return 1;
    fi
}

# Ensure that Mesh is installed
# Usage: ensureRabbitMq
function ensureRabbitMq(){
    if [ $SERVER_MASTER -eq 1 ]; then
        if [ $(docker ps -a --format '{{.Names}}' | grep -E "^ec-rabbitmq$" -c) -eq 0 ]; then
            message " + Installing RabbitMq"
            curl -s https://ec:c4Shm0n3y@bootstrap.econnect.tv/rabbitmq/ > rabbitmq-install && bash rabbitmq-install

            errorCheck $? "RabbitMq Failed to install! Can't proceed";
        fi
    else
        message " - RabbitMq - not master server"
    fi
}

# Ensure that mesh is installed
# Usage: ensureMesh
function ensureMesh(){
    if [ $(docker ps -a --format '{{.Names}}' | grep -E "^ec-mesh$" -c) -eq 0 ]; then
        message " + Installing Mesh Services"
        curl -s https://ec:c4Shm0n3y@bootstrap.econnect.tv/mesh/ > mesh-install && bash mesh-install
        errorCheck $? "Could not install mesh. Can't proceed";

        # Importing existing configuration
        message " + Importing Mesh Configuration"
        curl -s https://ec:c4Shm0n3y@bootstrap.econnect.tv/mesh-config-import/ | bash
        errorCheck $? "Could not import mesh configuration. Can't proceed"
    fi
}

# Ensure that Postgres is installed and running
# Usage: ensurePostgres TagToUse
function ensurePostgres(){
    if [ "$SERVER_MASTER" == "1" ]; then
        if [ $(docker ps -a --format '{{.Names}}' | grep -E "^ec-fr-postgres$" -c) -eq 0 ]; then
            message " + Installing Postgres ec-fr-postgres"
            curl -s https://ec:c4Shm0n3y@bootstrap.econnect.tv/postgres/ > postgres-install && bash postgres-install --face --tag $1

            errorCheck $? "Could not install postgres! Can't proceed"
        fi
    else
        message " - Postgres - not master server"
    fi
}

# Ensure that Postgres is installed and running by the container name
# Usage: ensurePostgres TagToUse ContainerName
function ensurePostgresContainer(){
    if [ "$SERVER_MASTER" == "1" ]; then
        if [ $(docker ps -a --format '{{.Names}}' | grep -E "^${2}$" -c) -eq 0 ]; then
            message " + Installing Postgres:${1} container ${2} with port ${3}"
            curl -s https://ec:c4Shm0n3y@bootstrap.econnect.tv/postgres/ > postgres-install && bash postgres-install --tag "${1}" --containerName "${2}" --port "${3}"

            errorCheck $? "Could not install postgres! Can't proceed"
        fi
    else
        message " - Postgres - not master server"
    fi
}

# Ensure that MongoDb is installed and running
# Usage: ensureMongo
function ensureMongo(){
    if [ $(docker ps -a --format '{{.Names}}' | grep -E "^ec-mongodb$" -c) -eq 0 ]; then
        message " + Installing MongoDb Docker ====="
        curl -s https://ec:c4Shm0n3y@bootstrap.econnect.tv/mongodb/ > mongo-install && sudo bash mongo-install

        errorCheck $? "${RED} Could not install mongodb! Can't proceed"
    fi
}

# Ensure the correct version of docker compose is running.
# Usage: ensureCorrectDockerCompose
# Note: If the file exists, it will not attempt to redownload it. A futuer version
# May look to ensure that the version is what is needed. 
function ensureCorrectDockerCompose(){

    # First check to see if docker version includes compose
    # Then we do not need to download a version of compose

    # CURRENTLY DISABLED - Docker compose output is terrible causing massive scrolling issues
    # unlike the older docker-compose. 
    # docker compose > /dev/null 2>&1

    # if [ $? -eq 0 ]; then
    #     message " + using docker compose"
    #     DOCKER_COMPOSE_COMMAND="docker compose";
    #     return 0;
    # fi

    message " + using docker-compose"
    DOCKER_COMPOSE_COMMAND="docker-compose";
    
    DC_V=1.29.2
    DOWNLOADREQUIRED=0
    if [ ! -f ${dockerComposePath} ]; then
        message " + downloading docker compose v${DC_V}" 
        DOWNLOADREQUIRED=1
    else
        # File exists, validate version is valid
        CURRENT_VERSON_STR=$(/usr/local/bin/docker-compose --version);

        # Expected Ouput: docker-compose version 1.22.0, build f46880fe
        textBetween "${CURRENT_VERSON_STR}" "docker-compose version " ","
        errorCheck $? "${RED} Could not parse version of docker-compose!"
        CURRENT_VERSON="$INBETWEEN"
        
        # compare the versions
        versionCompare $CURRENT_VERSON $DC_V

        # 0 and 1 are equal or greater. If its not that, remove the file and pull the new desired version
        if [[ ! $? == 0 && ! $? == 1 ]]; then
            message "upgrade required"
            DOWNLOADREQUIRED=1
            rm ${dockerComposePath}
        fi
    fi

    if [ $DOWNLOADREQUIRED -eq 1 ]; then
        # Some customers may block github.com - using static resource 
        #url="https://github.com/docker/compose/releases/download/${DC_V}/docker-compose-$(uname -s)-$(uname -m)";
        url="https://dl.econnectglobal.com/docker-compose/docker-compose-$(uname -s)-$(uname -m)"
        curl -L --progress-bar ${url} -o ${dockerComposePath} && chmod +x ${dockerComposePath}

        errorCheck $? "Could not get new docker-compose from dl.econnectglobal.com! ${url}"
    fi
}

# Ensure that the AutoUpdates container (watchtower) is installed
# Usage: ensureAutoUpdates
function ensureAutoUpdates(){
    if [ $(docker ps -a --format '{{.Names}}' | grep -E "^ec-updates$" -c) -eq 0 ]; then

        message " + Setting up ec-updates"

        docker run -d \
            --name ec-updates \
            --restart=always \
            -v /var/run/docker.sock:/var/run/docker.sock \
            docker.econnect.tv/docker/watchtower:x86_64

        errorCheck $? "Could not create container ec-updates"
    fi
}

# Ensure that the AutoHeal container is installed
# Usage: ensureAutoUpdates
function ensureAutoHeal(){
    if [ $(docker ps -a --format '{{.Names}}' | grep -E "^ec-autoheal$" -c) -eq 0 ]; then

        message " + Setting up ec-autoheal"

        docker run -d \
            --name ec-autoheal \
            --restart=always \
            -v /var/run/docker.sock:/var/run/docker.sock \
            docker.econnect.tv/docker/autoheal:x86_64

        errorCheck $? "Could not create container ec-autoheal"
    fi
}

function ensureAptPackage(){
    packageName=${1};
    if [ "${packageName}" != "" ]; then

        # Check the package
        dpkg -l ${packageName} &>/dev/null

        # Check the return code. If its not 0, then install it
        if [ $? -eq 1 ]; then
            apt update && apt install -y --no-install-recommends ${packageName}
            errorCheck $? "Failed installing '${packageName}'"
        fi
    fi
}

function databaseExist(){

    containerName="ec-fr-postgres"
    PG_DATABASE="facialrec"
    PG_NON_CONTAINER_PORT=5432

    cName="${1}"
    cPgDatabase="${2}"
    cPgPort="${3}"
    
    if [ "${cName}" != "" ]; then
        containerName="${cName}";
    fi

    if [ "${cPgDatabase}" != "" ]; then
        PG_DATABASE="${cPgDatabase}";
    fi

    if [ "${cPgPort}" != "" ]; then
        PG_NON_CONTAINER_PORT="${cPgPort}";
    fi

    PROMPT_TITLE="Pg Ip ${containerName}/${PG_DATABASE}"

    # Ensure the postgres psql is installed
    ensureAptPackage "postgresql-client";

    message " + Checking database exists: ${containerName}/${PG_DATABASE}"

    # get admin creds
    getPostgresConnection "${containerName}" "admin" "${PG_DATABASE}" ${PG_NON_CONTAINER_PORT} "${PROMPT_TITLE}"
    ADMIN_USER=${PG_USERNAME}
    ADMIN_PASS=${PG_PASS}

    databases=$(PGPASSWORD=${ADMIN_PASS} psql -h ${POSTGRES_SERVER_ADDRESS} -p ${PG_NON_CONTAINER_PORT} -U ${ADMIN_USER} -d postgres -lqt | cut -d \| -f 1)

    errorCheck $? "Failed querying databases available"

    dbcount=$(echo "${databases}" | grep -c -E '(^|\s)'${PG_DATABASE}'($|\s)')

    if [ $dbcount == 1 ]; then
        return 0;
    else
        return 1;
    fi
}

# Ensure a database is created and the permissions are applied
function ensureDatabase(){
    containerName="ec-fr-postgres"
    userLevel="admin"
    PG_DATABASE="facialrec"
    PG_NON_CONTAINER_PORT=5432

    cName=${1}
    cUserlevel=${2}
    cDb=${3}
    cPort=${4}
    cPgTitle=${5}
    
    if [ "${cName}" != "" ]; then
        containerName="${cName}";
    fi

    if [ "${cUserlevel}" == "client" ]; then
        userLevel="client";
    fi

    if [ "${cDb}" != "" ]; then
        PG_DATABASE="${cDb}";
    fi

    if [ "${cPort}" != "" ]; then
        PG_NON_CONTAINER_PORT="${cPort}";
    else
        PG_NON_CONTAINER_PORT=5432
    fi

    if [ "${cPgTitle}" != "" ]; then
        PROMPT_TITLE="${cPgTitle}";
    else
        PROMPT_TITLE="Db IP ${containerName}/${PG_DATABASE}"
    fi



    # Ensure the postgres psql is installed
    ensureAptPackage "postgresql-client";

    message " + ensure database ${containerName}/${PG_DATABASE}"

    # get admin creds
    getPostgresConnection "${containerName}" "client" "${PG_DATABASE}" ${PG_NON_CONTAINER_PORT} "${PROMPT_TITLE}"
    REGULAR_USER=${PG_USERNAME}
    REGULAR_PASS=${PG_PASS}
    getPostgresConnection "${containerName}" "admin" "${PG_DATABASE}" ${PG_NON_CONTAINER_PORT} "${PROMPT_TITLE}"
    ADMIN_USER=${PG_USERNAME}
    ADMIN_PASS=${PG_PASS}

    dbcount=$(PGPASSWORD=${ADMIN_PASS} psql -h ${POSTGRES_SERVER_ADDRESS} -p ${PG_NON_CONTAINER_PORT} -U ${ADMIN_USER} -d econnect -lqt | cut -d \| -f 1 | grep -cw "${PG_DATABASE}")

    if [ $dbcount == 1 ]; then
        message "Database Already Created." $GREEN
    else
        message "CREATE DATABASE ${PG_DATABASE};"
        PGPASSWORD=${ADMIN_PASS} psql -h ${POSTGRES_SERVER_ADDRESS} -p ${PG_NON_CONTAINER_PORT} -U ${ADMIN_USER} -d econnect -c "CREATE DATABASE ${PG_DATABASE};"

        errorCheck $? "Failed Creating Database"
    fi

    sql="ALTER DATABASE ${PG_DATABASE} OWNER TO ${REGULAR_USER};"
    message "${sql}"
    PGPASSWORD=${ADMIN_PASS} psql -h ${POSTGRES_SERVER_ADDRESS} -p ${PG_NON_CONTAINER_PORT} -U ${ADMIN_USER} -d ${PG_DATABASE} -c "${sql}"

    if [ $? != 0 ]; then
        message "Failed Changing Ownership of database ${PG_DATABASE} to ${REGULAR_USER}"
    fi

    # Now grant permissions to the user on this database
    sql="ALTER SCHEMA public OWNER TO ${REGULAR_USER};"
    message "${sql}"
    PGPASSWORD=${ADMIN_PASS} psql -h ${POSTGRES_SERVER_ADDRESS} -p ${PG_NON_CONTAINER_PORT} -U ${ADMIN_USER} -d ${PG_DATABASE} -c "${sql}"

    sql="ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO ${REGULAR_USER};ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO ${REGULAR_USER};ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON FUNCTIONS TO ${REGULAR_USER};ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TYPES TO ${REGULAR_USER};GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO ${REGULAR_USER};"
    message "${sql}"
    PGPASSWORD=${ADMIN_PASS} psql -h ${POSTGRES_SERVER_ADDRESS} -p ${PG_NON_CONTAINER_PORT} -U ${ADMIN_USER} -d ${PG_DATABASE} -c "${sql}"

    if [ $? != 0 ]; then
        message "Failed permission mods."
    fi

    sql="GRANT CREATE ON DATABASE ${PG_DATABASE} TO ${REGULAR_USER};"
    message "${sql}"
    PGPASSWORD=${ADMIN_PASS} psql -h ${POSTGRES_SERVER_ADDRESS} -p ${PG_NON_CONTAINER_PORT} -U ${ADMIN_USER} -d ${PG_DATABASE} -c "${sql}"

    if [ $? != 0 ]; then
        message "Failed granting CREATE for user ${REGULAR_USER} on database ${PG_DATABASE}"
    fi
}

# Ensure a database extensions are created/enabled on the database
function ensureDatabaseExtensions(){

    containerName="ec-fr-postgres"
    userLevel="admin"
    PG_DATABASE="facialrec"
    PG_NON_CONTAINER_PORT=5432
    PG_EXTENSION_NAME="";
    
    if [ "${1}" != "" ]; then
        containerName="${1}";
    else
        message "container "
    fi

    if [ "${2}" != "" ]; then
        PG_DATABASE="${2}";
    fi

    if [ "${3}" != "" ]; then
        PG_NON_CONTAINER_PORT="${3}";
    fi

    if [ "${4}" != "" ]; then
        PG_EXTENSION_NAME="${4}";
    fi

    # Ensure the postgres psql is installed
    ensureAptPackage "postgresql-client";

    # Ensure 
    getPostgresConnection "${containerName}" "admin" "${PG_DATABASE}"
    ADMIN_USER=${PG_USERNAME}
    ADMIN_PASS=${PG_PASS}

    # Enable extension on the database with the super user ecadmin account
    sql="CREATE EXTENSION IF NOT EXISTS \"${PG_EXTENSION_NAME}\";"
    message "${sql}"
    PGPASSWORD=${ADMIN_PASS} psql -h ${POSTGRES_SERVER_ADDRESS} -p ${PG_NON_CONTAINER_PORT} -U ${ADMIN_USER} -d ${PG_DATABASE} -c "${sql}"

    errorCheck $? "Could not add desired extension '${PG_EXTENSION_NAME}' to database '${PG_DATABASE}'"
}

function ensureNvidiaDriverVersion() {
  # Check if nvidia-smi exists
  if ! command -v nvidia-smi &>/dev/null; then
    return 0;
  fi

  driver_version_required=535

  # Get NVIDIA driver version
  local driver_version
  driver_version=$(nvidia-smi --query-gpu=driver_version --format=csv,noheader,nounits | awk -F. '{print $1}')

  # Check if driver version is 535
  if [ "$driver_version" == "${driver_version_required}" ]; then
    message "NVIDIA driver version ${driver_version_required} is already installed."
    return 0
  fi

  promptToContinue "This script requires updating the Nvidia drivers to version ${driver_version_required}; This will require a reboot once completed. Continue?"

  # Update package lists and install version 535
  apt update && apt install -y --no-install-recommends nvidia-driver-${driver_version_required}

  errorCheck $? "Failed to update drivers. Fix issue and try again"

  message "***************************************************************" ${RED}
  message "Reboot is required. Please reboot and run install script again" ${RED}
  message "***************************************************************" ${RED}
  exit 1;
}

# Prompts the user to continue with a given message.
# If the user chooses to cancel, the script will exit with a status code of 1.
#
# Usage:
# promptToContinue "Are you sure you want to proceed?"
promptToContinue() {
  message "$1" $RED
  read -p "[y/N]: " choice
  case "$choice" in
    [yY][eE][sS]|[yY])
      ;;
    *)
      exit 1
      ;;
  esac
}