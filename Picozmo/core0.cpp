#include <IoAbstractionWire.h>
#include <Arduino.h>
#include <qdec.h>
#include <SparkFun_Qwiic_Button.h>

#include "IoBounce2.h"
#include "Picozmo.h"

using namespace ::SimpleHacks;

static const uint LED_PIN = PICO_DEFAULT_LED_PIN;

static IoAbstractionRef io23017 = ioFrom23017(0x20);

static Bounce apuMasterBounce;
static Bounce apuStartBounce;

static IoBounce wingIceLightBounce(io23017);
static Bounce noseLightOffBounce;
static Bounce noseLightTakeoffBounce;
static Bounce runwayTurnoffLightBounce;
static Bounce landingLightBounce;
static Bounce strobeLightOffBounce;
static Bounce strobeLightOnBounce;
static Bounce beaconLightBounce;
static Bounce navLightBounce;

static Bounce fcuAltPushBounce;
static const uint16_t fcuAltPinA = D18, fcuAltPinB = D19;
static const pinid_t externalLedFirstPin = 12, externalLedLastPin = 15;
static QDecoder qdec(fcuAltPinA, fcuAltPinB, true);

static QwiicButton qwiicButton;

static critical_section_t isrCritical;
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

  Wire.setSDA(D0);
  Wire.setSCL(D1);
  Wire.begin();

  pinMode(LED_PIN, OUTPUT);

  apuStartBounce.attach(D14, INPUT_PULLUP);
  apuMasterBounce.attach(D15, INPUT_PULLUP);
  runwayTurnoffLightBounce.attach(D13, INPUT_PULLUP); //TODO: MCP23017 pin 2
  noseLightTakeoffBounce.attach(D11, INPUT_PULLUP); //TODO: MCP23017 pin 7
  noseLightOffBounce.attach(D10, INPUT_PULLUP); //TODO: MCP23017 pin 5
  landingLightBounce.attach(D12, INPUT_PULLUP); //TODO: MCP23017 pin 8
  strobeLightOffBounce.attach(D9, INPUT_PULLUP); //TODO: MCP23017 pin 6
  strobeLightOnBounce.attach(D5, INPUT_PULLUP); //TODO: MCP23017 pin 3
  beaconLightBounce.attach(D8, INPUT_PULLUP); //TODO: MCP23017 pin 1
  navLightBounce.attach(D6, INPUT_PULLUP); //TODO: MCP23017 pin 4

  wingIceLightBounce.attach(0, INPUT_PULLUP);

  fcuAltPushBounce.attach(D17, INPUT_PULLUP);
  qdec.begin();
  attachInterrupt(digitalPinToInterrupt(fcuAltPinA), fcuAltRotatedIsr, CHANGE);
  attachInterrupt(digitalPinToInterrupt(fcuAltPinB), fcuAltRotatedIsr, CHANGE);

  qwiicButton.begin();

  for (int i = externalLedFirstPin; i <= externalLedLastPin; ++i) {
    io23017->pinDirection(i, OUTPUT);
  }
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
    beaconLight = beaconLightBounce.read() ? "false" : "true";

  if (assumeChanged || wingIceLightBounce.update())
    wingIceLight = wingIceLightBounce.read() ? "false" : "true";

  if (assumeChanged || navLightBounce.update())
    navLight = navLightBounce.read() ? "false" : "true";

  if (assumeChanged || runwayTurnoffLightBounce.update())
    runwayTurnoffLight= runwayTurnoffLightBounce.read() ? "false" : "true";

  if (assumeChanged || landingLightBounce.update())
    landingLight = landingLightBounce.read() ? "false" : "true";

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

void setExternalLeds(uint8_t value) {
  for (int i = externalLedFirstPin; i <= externalLedLastPin; ++i) {
    io23017->writeValue(i, value);
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
    setExternalLeds(HIGH);
  else if (incoming == 'x')
    setExternalLeds(LOW);
  else if (incoming == 's')
    scanI2C();
  else
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

  ioDeviceSync(io23017); // Why isn't this added to the task manager?
  taskManager.runLoop();

  seviceQwiicButton();

  updateContinuousInputs();
  updateMomentaryInputs();
  updateFromInterrupts();
}
