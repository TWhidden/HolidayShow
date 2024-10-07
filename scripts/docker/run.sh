#!/bin/bash

command=""
label=""
model=""
arch="x64"

red='\033[0;31m'
blue='\033[0;34m'
green='\033[0;32m'
yellow='\033[1;33m'
nc='\033[0m' # No Color

if [ "${HS_DB_SERVER}" == "" ]; then
  echo -e "\n${red}Env vars are not set for Database Connection.${nc}";
  exit 1;
fi

while getopts "a:l:c:d:" opt; do
  case ${opt} in
    a ) arch=${OPTARG};;
    l ) label=${OPTARG};;
    c ) command=${OPTARG};;
    d ) model=${OPTARG}
  esac
done

if [ "${model}" == "" ]; then
    echo -e "\n${red}Error no path defined.${nc}"
    exit 1;
fi

package_info=`cat package.json`

grabInfo () {
  info=`echo "$package_info" |grep "\"${1}\""|awk -F'"' '{print $4}'`
  echo $info
}

name=`grabInfo name`
version=`grabInfo version`
majorVersion=$(echo v${version}|egrep -o 'v[0-9]*')

# Source skeleton config variables
. ./skeleton.cfg

# Form image name
if [ "${label}" == "" ]; then
  image="${name}"
else
  image="${name}/${label}"
fi

imageToRun=${registry_group}/${image}:${arch}-${majorVersion}

echo -e "\n✔︎ ${yellow}Running Docker container $name for '${imageToRun}' with storage root '$storageRoot'...${nc}\n"

docker run -it --rm \
  -e 'TZ=America/Los_Angeles' \
  --net bridge \
  -p 5050:5001 \
  -e DBSERVER=${HS_DB_SERVER} \
  -e DBNAME=${HS_DB_CATALOG} \
  -e DBUSER=${HS_DB_UN} \
  -e DBPASS=${HS_DB_PW} \
  ${imageToRun} ${command}

echo -e "\n${blue}★ ${green}COMPLETE ${blue}★${nc}\n"