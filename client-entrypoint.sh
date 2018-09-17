#!/bin/bash
echo "HOLIDAY SHOW CLIENT CORE"
echo "LISTENING PORT: $PORT"
echo "DB SERVER: $DBSERVER"
echo "DB NAME: $DBNAME"
echo "DB USER: $DBUSER"
echo "STARTING HOLIDAY SHOW SERVER CORE..."
dotnet holidayshowserver.core.dll -p $PORT -d $DBSERVER -n $DBNAME -u $DBUSER -s $DBPASS
