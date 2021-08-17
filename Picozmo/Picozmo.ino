// https://www.arduino.cc/reference/en/

#include "Picozmo.h"

//TODO: other things to experiment with:
// https://emalliab.wordpress.com/2021/04/18/raspberry-pi-pico-arduino-core-and-timers/
// https://arduino-pico.readthedocs.io/en/latest/multicore.html

volatile byte apuMasterPressed, apuStartPressed;
volatile short spoilerHandle = -2;
const char *volatile strobeLight, *volatile beaconLight, *volatile wingIceLight, *volatile navLight,
  *volatile runwayTurnoffLight, *volatile landingLight, *volatile noseLight;
volatile int incoming;
volatile bool forceUpdate = true;
