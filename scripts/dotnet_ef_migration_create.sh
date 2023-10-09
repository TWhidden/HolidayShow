#!/bin/bash

# Load the common colors, re-usable functions
# Get the full path of the current script
SCRIPT_PATH="$(dirname "$(readlink -f "$0")")"
. ${SCRIPT_PATH}/script-helper.sh

#cd src/dotnet/server

if [ "${1}" == "" ]; then

      message "Project param is missing" $RED
      exit 1;
fi

if [ ! -d "${1}" ]; then 
      message "Project Path doest not exist." $RED
      exit 1;
fi

if [ "${2}" == "" ]; then

      message "Db Context is missing" $RED
      exit 1;
fi

PROJECT_PATH="${1}"
PROJECT_STARTUP=./HolidayShowServer.Core
DB_CONTEXT="${2}"

message "#######################################################"
message " Creating Migration for ${1}"
message "#######################################################"

read -p 'Migration Name: ' migration_name

if [ -z "$migration_name" ]
then
      echo "Migration name cannot be empty"
      exit 1;
fi

dotnet tool restore

errorCheck $? "dotnet tool restore failed"

dotnet restore

errorCheck $? "dotnet restore failed"

export EF_MIGRATION=1

message "PROJECT_STARTUP: ${PROJECT_STARTUP}"
message "PROJECT_PATH: ${PROJECT_PATH}"
message "DB_CONTEXT: ${DB_CONTEXT}"

export PATH="$PATH:$HOME/.dotnet/tools"

dotnet ef migrations add --startup-project ${PROJECT_STARTUP} --project ${PROJECT_PATH} --context ${DB_CONTEXT} $migration_name 

errorCheck $? "dotnet ef migration add failed"

unset EF_MIGRATION
