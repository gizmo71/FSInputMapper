#include <Arduino.h>
#include <Bounce2.h>
#include <qdec.h>

#include "Picozmo.h"

using namespace ::SimpleHacks;

const uint LED_PIN = PICO_DEFAULT_LED_PIN;

static Bounce apuMasterBounce;
static Bounce apuStartBounce;

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
const uint16_t fcuAltPinA = D18, fcuAltPinB = D19;
static QDecoder qdec(fcuAltPinA, fcuAltPinB, true);

void fcuAltRotated(void) {
  switch (qdec.update()) {
  case QDECODER_EVENT_CCW:
    ++fcuAltDelta;
    break;
  case QDECODER_EVENT_CW:
    --fcuAltDelta;
    break;
  }
}

void setup(void) {
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
  attachInterrupt(digitalPinToInterrupt(fcuAltPinA), fcuAltRotated, CHANGE);
  attachInterrupt(digitalPinToInterrupt(fcuAltPinB), fcuAltRotated, CHANGE);
}

short calculateSpoilerHandle() {
  static short spoilerHandleRawOld = -1000;
  short spoilerHandleRaw = analogRead(A0);
  if (abs(spoilerHandleRaw - spoilerHandleRawOld) <= 20)
    spoilerHandleRaw = spoilerHandleRawOld;
  else
    spoilerHandleRawOld = spoilerHandleRaw;

//TODO: use constrain(x, min, max) instead of min+max... Beware macros!
// https://www.arduino.cc/reference/en/language/functions/math/constrain/
  if (spoilerHandleRaw > 3800)
    return -1;
  else if (spoilerHandleRaw > 2100)
    return min(max(3100 - spoilerHandleRaw, 0) / 20, 50);
  else
    return min(max(1100 - spoilerHandleRaw, 0) / 20, 50) + 50;
}

void updateContinuousInputs(void) {
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

//TODO: monitor FCU into comm var? Do we still need to avoid non-atomic updates? Is there a better way?
// Perhaps https://github.com/Locoduino/RingBuffer, but beware interrupts may not disable across cores...
}

void updateMomentaryInputs(void) {
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

void updateOuputs(void) {
  if (incoming != -1) {
    digitalWrite(LED_PIN, (incoming & 1) ? HIGH : LOW);
    incoming = -1;
  }
}

void loop(void) {
  updateContinuousInputs();
  updateMomentaryInputs();
  updateOuputs();
  sleep_ms(5); //TODO: do we really need this?
}
