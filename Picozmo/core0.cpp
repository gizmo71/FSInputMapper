#include <IoAbstractionWire.h>
#include <Arduino.h>
#include <Bounce2.h>
#include <qdec.h>
#include <SparkFun_Qwiic_Button.h>

#include "Picozmo.h"

using namespace ::SimpleHacks;

const uint LED_PIN = PICO_DEFAULT_LED_PIN;

Bounce apuMasterBounce;
Bounce apuStartBounce;

Bounce noseLightOffBounce;
Bounce noseLightTakeoffBounce;
Bounce runwayTurnoffLightBounce;
Bounce landingLightBounce;
Bounce strobeLightOffBounce;
Bounce strobeLightOnBounce;
Bounce beaconLightBounce;
Bounce wingIceLightBounce;
Bounce navLightBounce;

Bounce fcuAltPushBounce;
const uint16_t fcuAltPinA = D18, fcuAltPinB = D19;
QDecoder qdec(fcuAltPinA, fcuAltPinB, true);

QwiicButton qwiicButton;

IoAbstractionRef io23017;

critical_section_t isrCritical;
static short fcuAltDeltaIsr;

void fcuAltRotatedIsr(void) {
  switch (qdec.update()) {
  case QDECODER_EVENT_CCW:
    ++fcuAltDeltaIsr;
    break;
  case QDECODER_EVENT_CW:
    --fcuAltDeltaIsr;
    break;
  }
}

void setup(void) {
  critical_section_init(&isrCritical);
  mutex_init(&mut0to1);

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
  attachInterrupt(digitalPinToInterrupt(fcuAltPinA), fcuAltRotatedIsr, CHANGE);
  attachInterrupt(digitalPinToInterrupt(fcuAltPinB), fcuAltRotatedIsr, CHANGE);

  Wire.setSDA(D0);
  Wire.setSCL(D1);
  Wire.begin();

  qwiicButton.begin();

  io23017 = ioFrom23017(0x20);
  for (int i = 0; i < 8; ++i)
    io23017->pinDirection(i, INPUT_PULLUP);
  ioDevicePinMode(io23017, 8, OUTPUT);
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

void updateFromInterrupts(void) {
  {
    critical_section_enter_blocking(&isrCritical);
    short fcuAltDeltaTemp = fcuAltDeltaIsr;
    fcuAltDeltaIsr = 0;
    critical_section_exit(&isrCritical);

    mutex_enter_blocking(&mut0to1);
    fcuAltDelta += fcuAltDeltaTemp;
    mutex_exit(&mut0to1);
  }
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
  if (incoming == 'L')
    digitalWrite(LED_PIN, HIGH);
  else if (incoming == 'l')
    digitalWrite(LED_PIN, LOW);
  else if (incoming == 'A')
    qwiicButton.LEDon(255);
  else if (incoming == 'a')
    qwiicButton.LEDon(0);
  else if (incoming == 'X')
    ioDeviceDigitalWrite(io23017, 8, HIGH);
  else if (incoming == 'x')
    ioDeviceDigitalWrite(io23017, 8, LOW);
  else if (incoming == 's') {
    scanI2C();
  } else
    return;
  incoming = -1;
}

void seviceQwiicButton(void) {
  static bool wasPressed = false;
  bool isPressed = qwiicButton.isPressed();
  if (!fcuAltPulled && isPressed && !wasPressed) {
    fcuAltPulled = true;
  }
  wasPressed = isPressed;
}

void loop(void) {
  updateOuputs();
  ioDeviceSync(io23017);
  seviceQwiicButton();
  updateContinuousInputs();
  updateMomentaryInputs();
  updateFromInterrupts();
}
