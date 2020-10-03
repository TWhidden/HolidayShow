#!/bin/bash

package_info=`cat package.json`

grabInfo () {
  info=`echo "$package_info" |grep "\"${1}\""|awk -F'"' '{print $4}'`
  echo $info
}

version=`grabInfo version`

echo -e "\n✔︎ ${yellow}Building React Front End v${version}...${nc}\n"

scriptDir=$(pwd)
echo $scriptDir

reactPath="../../../HolidayShowWeb/ClientApp";
wwwrootPath="../../../HolidayShowWeb/wwwroot/react"

# Update the package version to match the repo version
sed -i "s|\"version\": \".*\"|\"version\": \"${version}\"|" ${reactPath}/package.json

if [[ $? -ne 0 ]]; then
    echo -e "\n${RED}Could not set package version! Aborting process.${NC}"
    exit 1;
fi

cd ${reactPath}
npm install

if [ $? != 0 ]; then
echo -e "\n${red}Error with npm install! Aborting process.${nc}"
exit 1;
fi

npm run build

if [ $? != 0 ]; then
echo -e "\n${red}Error npm build! Aborting process.${nc}"
exit 1;
fi

cd $scriptDir
rm -rf ${wwwrootPath}
mkdir -p ${wwwrootPath}
cp -R ${reactPath}/build/* ${wwwrootPath}