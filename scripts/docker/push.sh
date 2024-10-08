#!/bin/bash

arch="all"
label=""

while getopts "a:l:" opt; do
  case ${opt} in
    a ) arch=${OPTARG};;
    l ) label=${OPTARG};;
  esac
done

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
nc='\033[0m' # No Color

image="${name}"
# Form image name
if [ "${label}" != "" ]; then
  appendtag="-${label}"
fi

# Check if we're trying to push production image with uncommitted changes
# Use following to check for new file additions also:
#if [[ -n $(git status -s) ]] && [[ -z $label ]]; then
#if [[ -n $(git diff-index HEAD --) ]] && [[ -z $label ]]; then
#  echo -e "\n✗ ${red}Attemping to push non-Beta Docker images before committing files${nc}\n"
#  exit 0
#fi

echo -e "\n✔︎ ${yellow}Checking registry login...${nc}\n"
docker login ${registry_host} 

pushImage () {

  imageToRun=${registry_group}/${image}:${1}-${majorVersion}${appendtag}

  echo -e "\n✔︎ ${yellow}Pushing ${imageToRun}${nc}\n"

  docker push ${imageToRun}

  if [ $? != 0 ]; then
  echo -e "\n${red}Failed.${nc}"
  exit 1;
fi
}

if [ "${arch}" == "all" ]; then
  pushImage "x64"
else
  pushImage "${arch}"
fi

echo -e "\n${blue}★ ${green}COMPLETE ${blue}★${nc}\n"