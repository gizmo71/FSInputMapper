// https://www.arduino.cc/reference/en/

#include "Picozmo.h"

//TODO: other things to experiment with:
// https://emalliab.wordpress.com/2021/04/18/raspberry-pi-pico-arduino-core-and-timers/
// https://github.com/muwerk/muwerk and https://github.com/muwerk/ustd

volatile bool apuMasterPressed, apuStartPressed, baroPushed, baroPulled, trkFpaToggled, speedMachToggled,
  fcuAltPushed, fcuAltPulled, fcuVsPushed, fcuVsPulled, fcuSpeedPushed, fcuSpeedPulled, fcuHeadingPushed, fcuHeadingPulled;
volatile short spoilerHandle = -2, fcuAltMode, fcuAltDelta, fcuVsDelta, fcuHeadingDelta, fcuSpeedDelta, baroDelta;
const char *volatile strobeLight, *volatile beaconLight, *volatile wingIceLight, *volatile navLight,
  *volatile runwayTurnoffLight, *volatile landingLight, *volatile noseLight, *volatile seatBeltSign;

const char *baroUnitInHg = "inHg", *baroUnitHPa = "hPa", *volatile newBaroUnits, *volatile currentBaroUnits;

volatile bool forceUpdate, apuMasterOn, apuFault, apuStartOn, apuAvail, fcuAltManaged = true;
volatile char fcuLcdText[4][16];

mutex_t mut0to1, mut1to0;
