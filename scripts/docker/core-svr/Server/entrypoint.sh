#!/bin/sh


RED='\033[0;31m'
BLUE='\033[0;34m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No color

echo -e "\n${YELLOW}STARTING APPLICATION.${NC}\n"

echo "HOLIDAY SHOW SERVER CORE"
echo "LISTENING PORT: $PORT"
echo "DB SERVER: $DBSERVER"
echo "DB NAME: $DBNAME"
echo "DB USER: $DBUSER"
echo "STARTING HOLIDAY SHOW SERVER CORE..."

/app/holidayshowserver.core -p $PORT -d $DBSERVER -n $DBNAME -u $DBUSER -s $DBPASS
