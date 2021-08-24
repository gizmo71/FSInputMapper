extern volatile byte apuMasterPressed, apuStartPressed, fcuAltPushed, fcuAltPulled;
extern volatile short spoilerHandle, fcuAltDelta;
extern const char *volatile strobeLight, *volatile beaconLight, *volatile wingIceLight, *volatile navLight,
  *volatile runwayTurnoffLight, *volatile landingLight, *volatile noseLight;

extern volatile int incoming;
extern volatile bool forceUpdate;

extern mutex_t mut0to1;

extern void scanI2C(void);
