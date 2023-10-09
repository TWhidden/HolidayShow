#!/bin/bash
echo "HOLIDAY SHOW CLIENT CORE"
echo "CONNECTING TO SERVER: $SERVER:$PORT"
echo "STORAGE PATH: $STORAGE"
echo "DEVICE ID: $DEVICEID"

# Set the output to be the audio jack
# 1 = Analog Jack
# 2 = HDMI
# reference: https://www.raspberrypi.org/documentation/configuration/audio-config.md
echo "Setting Analog Output for audio..."
amixer cset numid=3 1

# set the volume level of the PCM device to 100%
echo "Setting Volume Level to 100%..."
amixer set PCM 100%

echo "Starting HolidayShowClient.Core..."
./HolidayShowClient.Core -s "$SERVER" -p $PORT -d $DEVICEID -a "$STORAGE"
