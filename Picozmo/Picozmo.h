// These communicate from core0 to core1.
extern volatile bool apuMasterPressed, apuStartPressed, baroPushed, baroPulled,
  speedMachToggled, fcuSpeedPushed, fcuSpeedPulled, fcuHeadingPushed, fcuHeadingPulled,
  fcuAltPushed, fcuAltPulled, fcuVsPushed, fcuVsPulled, trkFpaToggled;
extern volatile short spoilerHandle, fcuAltMode, fcuAltDelta, fcuVsDelta, fcuHeadingDelta, fcuSpeedDelta, baroDelta;
extern const char *volatile strobeLight, *volatile beaconLight, *volatile wingIceLight, *volatile navLight,
  *volatile runwayTurnoffLight, *volatile landingLight, *volatile noseLight, *volatile seatBeltSign;

extern const char *baroUnitInHg, *baroUnitHPa, *volatile newBaroUnits, *volatile currentBaroUnits;

// These communicate from core1 to core0.
extern volatile bool forceUpdate, apuMasterOn, apuFault, apuStartOn, apuAvail, fcuAltManaged;
extern volatile char fcuLcdText[4][16];

extern mutex_t mut0to1, mut1to0;

extern void scanI2C(void);
