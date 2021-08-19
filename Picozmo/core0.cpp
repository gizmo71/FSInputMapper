#include <Arduino.h>
#include <Bounce2.h>

#include "Picozmo.h"

const uint LED_PIN = PICO_DEFAULT_LED_PIN;

static Bounce apuMasterBounce = Bounce();
static Bounce apuStartBounce = Bounce();

static Bounce noseLightOffBounce = Bounce();
static Bounce noseLightTakeoffBounce = Bounce();
static Bounce runwayTurnoffLightBounce = Bounce();
static Bounce landingLightBounce = Bounce();
static Bounce strobeLightOffBounce = Bounce();
static Bounce strobeLightOnBounce = Bounce();
static Bounce beaconLightBounce = Bounce();
static Bounce wingIceLightBounce = Bounce();
static Bounce navLightBounce = Bounce();

static Bounce rotBut;
static Bounce rotA;
static Bounce rotB;

void setup() {
  pinMode(LED_PIN, OUTPUT);

  apuStartBounce.attach(D14, INPUT_PULLUP);
  apuMasterBounce.attach(D15, INPUT_PULLUP);
  runwayTurnoffLightBounce.attach(D13, INPUT_PULLUP);
  noseLightTakeoffBounce.attach(D11, INPUT_PULLUP);
  noseLightOffBounce.attach(D10, INPUT_PULLUP);
  landingLightBounce.attach(D12, INPUT_PULLUP);
  strobeLightOffBounce.attach(D9, INPUT_PULLUP);
  strobeLightOnBounce.attach(D5, INPUT_PULLUP);
  beaconLightBounce.attach(D8, INPUT_PULLUP);
  wingIceLightBounce.attach(D7, INPUT_PULLUP);
  navLightBounce.attach(D6, INPUT_PULLUP);

  rotBut.attach(D17, INPUT_PULLUP);
  rotA.attach(D18, INPUT_PULLUP);
  rotB.attach(D19, INPUT_PULLUP);
}

short calculateSpoilerHandle() {
  static short spoilerHandleRawOld = -1000;
  short spoilerHandleRaw = analogRead(A0);
  if (abs(spoilerHandleRaw - spoilerHandleRawOld) <= 20)
    spoilerHandleRaw = spoilerHandleRawOld;
  else
    spoilerHandleRawOld = spoilerHandleRaw;

  if (spoilerHandleRaw > 3800)
    return -1;
  else if (spoilerHandleRaw > 2100)
    return min(max(3100 - spoilerHandleRaw, 0) / 20, 50);
  else
    return min(max(1100 - spoilerHandleRaw, 0) / 20, 50) + 50;
}

void updateContinuousInputs() {
  bool assumeChanged = forceUpdate;
  if (assumeChanged) forceUpdate = false;

  static short spoilerHandleOld = -2;
  short spoilerHandleNew = calculateSpoilerHandle();
  if (assumeChanged || spoilerHandleOld != spoilerHandleNew) {
    spoilerHandle = spoilerHandleOld = spoilerHandleNew;
  }

  if (assumeChanged || strobeLightOffBounce.update() || strobeLightOnBounce.update())
    strobeLight = !strobeLightOffBounce.read() ? "off" : !strobeLightOnBounce.read() ? "on" : "auto";

  if (assumeChanged || beaconLightBounce.update())
    beaconLight = beaconLightBounce.read() ? "true" : "false";

  if (assumeChanged || wingIceLightBounce.update())
    wingIceLight = wingIceLightBounce.read() ? "false" : "true";

  if (assumeChanged || navLightBounce.update())
    navLight = navLightBounce.read() ? "true" : "false";

  if (assumeChanged || runwayTurnoffLightBounce.update())
    runwayTurnoffLight= runwayTurnoffLightBounce.read() ? "false" : "true";

  if (assumeChanged || landingLightBounce.update())
    landingLight = landingLightBounce.read() ? "true" : "false";

  if (assumeChanged || noseLightTakeoffBounce.update() || noseLightOffBounce.update())
    noseLight = !noseLightTakeoffBounce.read() ? "takeoff" : !noseLightOffBounce.read() ? "off" : "taxi";

  if (rotBut.update() || rotA.update() || rotB.update()) {
    Serial.print("# But ");
    Serial.print(rotBut.read());
    Serial.print("  A/B ");
    Serial.print(rotA.read());
    Serial.print('/');
    Serial.println(rotB.read());
  }
}

void updateMomentaryInputs() {
  apuMasterBounce.update();
  if (apuMasterPressed == false && apuMasterBounce.fell()) {
    apuMasterPressed = true;
  }

  apuStartBounce.update();
  if (apuStartPressed == false && apuStartBounce.fell()) {
    apuStartPressed = true;
  }
}

void updateOuputs() {
  if (incoming != -1) {
    digitalWrite(LED_PIN, (incoming & 1) ? HIGH : LOW);
    incoming = -1;
  }
}

void loop() {
  updateContinuousInputs();
  updateMomentaryInputs();
  updateOuputs();
  sleep_ms(5); //TODO: do we really need this?
}
