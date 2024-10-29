#!/bin/sh

RED='\033[0;31m'
BLUE='\033[0;34m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No color

echo -e "\n${YELLOW}STARTING APPLICATION.${NC}\n"

echo "HOLIDAY SHOW SERVER CORE"
echo "WEB SERVER LISTENING PORT: $WEBSERVERPORT"
echo "LISTENING PORT: $PORT"
echo "DB SERVER: $DBSERVER"
echo "DB NAME: $DBNAME"
echo "DB USER: $DBUSER"

# Configure the appsettings.json with jq, add or update in the DefaultConnection string
jq --arg dbserver "$DBSERVER" --arg dbname "$DBNAME" --arg dbuser "$DBUSER" --arg dbpass "$DBPASS" '.ConnectionStrings.DefaultConnection = "Server=" + $dbserver + ";Database=" + $dbname + ";User Id=" + $dbuser + ";Password=" + $dbpass + ";encrypt=no;"' /app/appsettings.json > /app/appsettings.json.tmp && mv /app/appsettings.json.tmp /app/appsettings.json

# Configure the appsettings.json with jq to update the default starting port ServerSettings.Port
jq --arg port "$PORT" '.ServerSettings.Port = $port' /app/appsettings.json > /app/appsettings.json.tmp && mv /app/appsettings.json.tmp /app/appsettings.json

# Configure appsettings.json section Kestrel.Endpoints.Http.Url to listen on all interfaces on the port WEBSERVERPORT http://0.0.0.0:$WEBSERVERPORT
jq --arg webserverport "$WEBSERVERPORT" '.Kestrel.Endpoints.Http.Url = "http://0.0.0.0:" + $webserverport' /app/appsettings.json > /app/appsettings.json.tmp && mv /app/appsettings.json.tmp /app/appsettings.json

echo "STARTING HOLIDAY SHOW SERVER ..."

/app/HolidayShowServer -p $PORT -d $DBSERVER -n $DBNAME -u $DBUSER -s $DBPASS
