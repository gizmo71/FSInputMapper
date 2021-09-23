// These communicate from core0 to core1.
extern volatile byte apuMasterPressed, apuStartPressed, baroPushed, baroPulled,
  fcuAltPushed, fcuAltPulled, fcuVsPushed, fcuVsPulled;
extern volatile short spoilerHandle, fcuAltDelta, fcuVsDelta, baroDelta;
extern const char *volatile strobeLight, *volatile beaconLight, *volatile wingIceLight, *volatile navLight,
  *volatile runwayTurnoffLight, *volatile landingLight, *volatile noseLight, *volatile baroUnits;

// These communicate from core1 to core0.
extern volatile bool forceUpdate, apuMasterOn, apuFault, apuStartOn, apuAvail, fcuAltManaged;

extern mutex_t mut0to1, mut1to0;

extern void scanI2C(void);
