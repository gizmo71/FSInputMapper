// https://www.arduino.cc/reference/en/

#include "Picozmo.h"

//TODO: other things to experiment with:
// https://emalliab.wordpress.com/2021/04/18/raspberry-pi-pico-arduino-core-and-timers/
// https://arduino-pico.readthedocs.io/en/latest/multicore.html
// https://github.com/muwerk/muwerk and https://github.com/muwerk/ustd

volatile byte apuMasterPressed, apuStartPressed, fcuAltPushed;
volatile short spoilerHandle = -2, fcuAltDelta;
const char *volatile strobeLight, *volatile beaconLight, *volatile wingIceLight, *volatile navLight,
  *volatile runwayTurnoffLight, *volatile landingLight, *volatile noseLight;
volatile int incoming;
volatile bool forceUpdate = true;
