extern volatile byte apuMasterPressed, apuStartPressed, fcuAltPushed;
extern volatile short spoilerHandle, fcuAltDelta;
extern const char *volatile strobeLight, *volatile beaconLight, *volatile wingIceLight, *volatile navLight,
  *volatile runwayTurnoffLight, *volatile landingLight, *volatile noseLight;

extern volatile int incoming;
extern volatile bool forceUpdate;
