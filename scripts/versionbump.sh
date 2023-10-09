#/bin/bash

BUMP_TYPE=${1}
PROJECT=${2}
PROD=${3}

echo -e "INPUT: BUMP: ${BUMP_TYPE}; PROJECT: ${PROJECT}; PROD: ${PROD}"



if [ "${BUMP_TYPE}" = "" ]; then
    echo -e "Missing Bump Type.\n"
    exit 1;
fi

if [ "${PROJECT}" = "" ]; then
    echo -e "Missing Project Type.\n"
    exit 1;
fi

CURRENT_VER=$(cat package.json | jq -r .version)

DIR="$(dirname "$0")"

NEXT_VER=$(${DIR}/semver.sh bump ${BUMP_TYPE} ${CURRENT_VER})

if [ $? != 0 ]; then
    echo -e "Could not bump version.\n"
    exit 1;
fi

RELEASETYPE="Beta";

if [ "${PROD}" = "Release" ]; then
    echo -e "Building Prod.\n"
    RELEASETYPE="${PROD}";

    echo "${BUMP_TYPE} for ${PROJECT}\n"

    data=$(jq --arg variable "${NEXT_VER}" '.version = $variable' < package.json)

    if [ $? != 0 ]; then
        echo -e "Failed Updating.\n"
        exit 1;
    fi
else
    EC_BETA=$(date -u +"%Y%m%d%H%M%S")
    NEXT_VER=${NEXT_VER}-beta${EC_BETA}
    
    data=$(jq --arg variable "${EC_BETA}" '.nextversion = $variable' < package.json)

    if [ $? != 0 ]; then
        echo -e "Failed Updating.\n"
        exit 1;
    fi
fi

echo "NEXT: ${NEXT_VER}"

MSG="${RELEASETYPE} ${PROJECT} v${NEXT_VER}"

echo "COMMIT MESSAGE: $MSG"

echo "\nCONTINUE?  [y] or [n]"

read c;

if [ "${c}" = "y" ]; then

    echo ${data} | jq . > package.json
    git commit -m "${MSG}" -- ${PWD}
    git tag "v${NEXT_VER}"

    git pull && git push && git push --tags

else
    echo "aborted. not pushed";
fi

