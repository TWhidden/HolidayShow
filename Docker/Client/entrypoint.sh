#!/bin/bash
echo "HOLIDAY SHOW CLIENT CORE"
echo "CONNECTING TO SERVER: $SERVER:$PORT"
echo "STORAGE PATH: $STORAGE"
echo "DEVICE ID: $DEVICEID"
echo "STARTING HOLIDAY SHOW CLIENT CORE..."

# Set the output to be the audio jack
# 1 = Analog Jack
# 2 = HDMI
# reference: https://www.raspberrypi.org/documentation/configuration/audio-config.md
amixer cset numid=3 1

./HolidayShowClient.Core -s "$SERVER" -p $PORT -d $DEVICEID -a "$STORAGE"
