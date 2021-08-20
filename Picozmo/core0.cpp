#include <Arduino.h>
#include <Bounce2.h>
#include <qdec.h>

#include "Picozmo.h"

const uint LED_PIN = PICO_DEFAULT_LED_PIN;

static Bounce apuMasterBounce = Bounce();
static Bounce apuStartBounce = Bounce();

static Bounce noseLightOffBounce;
static Bounce noseLightTakeoffBounce;
static Bounce runwayTurnoffLightBounce;
static Bounce landingLightBounce;
static Bounce strobeLightOffBounce;
static Bounce strobeLightOnBounce;
static Bounce beaconLightBounce;
static Bounce wingIceLightBounce;
static Bounce navLightBounce;

static Bounce fcuAltPushBounce;
::SimpleHacks::QDecoder qdec(D18, D19, true);

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

  fcuAltPushBounce.attach(D17, INPUT_PULLUP);
  qdec.begin();
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

  switch (qdec.update()) {
  case ::SimpleHacks::QDECODER_EVENT_CCW:
    ++fcuAltDelta;
    break;
  case ::SimpleHacks::QDECODER_EVENT_CW:
    --fcuAltDelta;
    break;
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

  fcuAltPushBounce.update();
  if (fcuAltPushed == false && fcuAltPushBounce.fell()) {
    fcuAltPushed = true;
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
  sleep_ms(1); //TODO: do we really need this?
}
