#!/bin/bash

arch="all"
model=""
label=""
buildReact="0"

while getopts "a:l:s:u:d:" opt; do
  case ${opt} in
    a ) arch=${OPTARG};;
    l ) label=${OPTARG};;
    s ) buildReact=${OPTARG};;
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

red='\033[0;31m'
blue='\033[0;34m'
green='\033[0;32m'
yellow='\033[1;33m'
nc='\033[0m' # No color

echo -e "\n✔︎ ${yellow}Build React: ${buildReact}...${nc}"
if [ "${buildReact}" == "1" ]; then
  export NODE_OPTIONS=--openssl-legacy-provider
  . ../buildreact.sh
fi

# Form image name and node base image to use
if [ "${label}" == "" ]; then
  image="${name}"
  app_version="${version}"
elif [ "${label}" == "beta" ]; then
  image="${name}"
  app_version="${version}"
else
  image="${name}"
  app_version="${version}-${label}"
fi

echo -e "\n✔︎ ${yellow}Publishing linux-${arch} image...${nc}"

dotnet publish ${csproj} -r linux-${arch} -c Release --self-contained -o ./build

if [ $? != 0 ]; then
    echo -e "\n${red}Error building image! Aborting process.${nc}"
    exit 1;
  fi

echo -e "\n✔︎ ${yellow}Building Docker image for ${image} v${version}...${nc}"

buildImage () {

  dockerfile="dockerfile"


  if [ $? != 0 ]; then
    echo -e "\n${red}Error pushing image! Aborting process.${nc}"
    exit 1;
  fi

  if [ "${label}" == "" ]; then
    imageToRun=${registry_group}/${image}:${1}-${majorVersion}
  else
    imageToRun=${registry_group}/${image}:${1}-${majorVersion}-${label}
  fi

  echo -e "\n✔ ${yellow}Building ${1} Docker image:${nc}\n"

  echo -e "\n✔ ${green}${imageToRun}${nc}\n"

  docker build --network host \
    -f ${dockerfile} \
    --no-cache \
    --pull \
    --build-arg app_name="${name}" \
    --build-arg version="${app_version}" \
    --build-arg build_date="$(date +'%m-%d-%y %H:%M:%S %p')" \
    -t ${imageToRun} .

  if [ $? != 0 ]; then
    echo -e "\n${red}Error building image! Aborting process.${nc}"
    exit 1;
  fi

   if [ $? != 0 ]; then
    echo -e "\n${red}Error tagging image! Aborting process.${nc}"
    exit 1;
  fi
}

if [ "${arch}" == "all" ]; then
  buildImage "x64"
  buildImage "x86"
else
  buildImage "${arch}"
fi

echo -e "\n${blue}★ ${green}COMPLETE ${blue}★${nc}\n"