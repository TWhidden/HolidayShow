#!/bin/bash

command=""
label=""
model=""

red='\033[0;31m'
blue='\033[0;34m'
green='\033[0;32m'
yellow='\033[1;33m'
nc='\033[0m' # No Color

# Linux
if [ -d /storage/docker ]; then
  storageRoot=/storage/docker;
# MacOS
elif [ -d ~/Storage/Docker ]; then
  storageRoot=~/Storage/Docker;
elif [ -d /Volumes/Storage/Docker ]; then
  storageRoot=/Volumes/Storage/Docker;
# Windows/WSL S:
elif [ -d /mnt/s/storage/docker ]; then
  storageRoot=/mnt/s/storage/docker;
# Windows/WSL D:
elif [ -d /mnt/d/storage/docker ]; then
  storageRoot=/mnt/d/storage/docker;
# Windows/WSL C:
elif [ -d /mnt/c/storage/docker ]; then
  storageRoot=/mnt/c/storage/docker;
fi

while getopts "l:c:d:" opt; do
  case ${opt} in
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

imageToRun=${registry_group}/${image}:latest

echo -e "\n✔︎ ${yellow}Running Docker container $name for '${imageToRun}' with storage root '$storageRoot'...${nc}\n"

docker run -it --rm \
  -e 'TZ=America/Los_Angeles' \
  --net bridge \
  -p 5050:5001 \
  -e DBSERVER=10.64.128.100,1401 \
  -e DBNAME=HolidayShow_Dev \
  -e DBUSER=dev \
  -e DBPASS=dev123 \
  ${imageToRun} ${command}


echo -e "\n${blue}★ ${green}COMPLETE ${blue}★${nc}\n"