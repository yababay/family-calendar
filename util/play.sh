#!/bin/bash

DIR=/srv/family-calendar
DEVICE="hw:CARD=U0x19080x1331,DEV=0"
RINGTONE=$DIR/Assets/$1.wav

aplay -D $DEVICE $RINGTONE

