#!/bin/bash

DIR=/srv/family-calendar
DEVICE="hw:CARD=U0x19080x1331,DEV=0"
#DEVICE="bluealsa:SRV=org.bluealsa,DEV=5C:FB:7C:E0:FC:56,PROFILE=a2dp"

echo "Обрабатывается событие категории \"$1\"" 

case "$1" in
    "Утро" )
        WAV=morning ;;
    "Учеба" )
        WAV=learning ;;
    "Преподавание" )
        WAV=teaching ;;
    "Гитара" )
        WAV=guitar ;;
    "Кино" )
        WAV=kino ;;
    "Английский" )
        WAV=english ;;
    "Прогулка" )
        WAV=walking ;;
    "Ужин" )
        WAV=supper ;;
esac

if [ -n "$WAV" ]
then
    RINGTONE=$DIR/Assets/$WAV.wav
    aplay -D $DEVICE $RINGTONE
fi

