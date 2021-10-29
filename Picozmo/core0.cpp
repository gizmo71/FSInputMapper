#include <IoAbstractionWire.h> // https://www.thecoderscorner.com/products/arduino-libraries/io-abstraction/ioabstraction-pins-io-expanders-shiftreg/
#include <Arduino.h>
#include <qdec.h>
#include <SparkFun_Qwiic_Button.h>
#include <LiquidCrystal_I2C.h>

#include "IoBounce2.h"
#include "Picozmo.h"

using namespace ::SimpleHacks;

static LiquidCrystal_I2C lcdRight(0x27, 16, 2);
static LiquidCrystal_I2C lcdLeft(0x26, 16, 2);

static const uint LED_PIN = PICO_DEFAULT_LED_PIN;

static IoAbstractionRef io23017 = ioFrom23017(0x20);

static IoBounce apuMasterBounce(io23017);
static IoBounce apuStartBounce(io23017);

static IoBounce seatBeltSignBounce(io23017);

static IoBounce wingIceLightBounce(io23017);
static IoBounce noseLightOffBounce(io23017);
static IoBounce noseLightTakeoffBounce(io23017);
static IoBounce runwayTurnoffLightBounce(io23017);
static IoBounce landingLightBounce(io23017);
static IoBounce strobeLightOffBounce(io23017);
static IoBounce strobeLightOnBounce(io23017);
static IoBounce beaconLightBounce(io23017);
static IoBounce navLightBounce(io23017);

#define PPR12 false
#define PPR24 true

static const pinid_t externalLedFirstPin = 4, externalLedLastPin = 7;

static Bounce baroModeBounce;
static Bounce fcuAltModeBounce;

static QwiicButton qwiicButton;

static critical_section_t isrCritical;

#define PUSH_PULL_ISR(control, pinA, pinB, is24ppr) \
  static const uint16_t control##PinA = pinA, control##PinB = pinB; \
  static QDecoder control##Qdec(control##PinA, control##PinB, is24ppr); \
  static Bounce control##PushBounce; \
  static Bounce control##PullBounce; \
  static short control##DeltaIsr; \
  void control##RotatedIsr(void) { \
    switch (control##Qdec.update()) { \
    case QDECODER_EVENT_CCW: \
      ++control##DeltaIsr; \
      break; \
    case QDECODER_EVENT_CW: \
      --control##DeltaIsr; \
      break; \
    } \
  }

PUSH_PULL_ISR(fcuSpeed, D11, D10, true)
PUSH_PULL_ISR(fcuHeading, D19, D18, true)
PUSH_PULL_ISR(fcuAlt, D20, D21, true)
PUSH_PULL_ISR(fcuVs, D8, D9, true)
PUSH_PULL_ISR(baro, D6, D7, false)

void setup(void) {
  critical_section_init(&isrCritical);
  mutex_init(&mut0to1);
  mutex_init(&mut1to0);

  Wire.setSDA(D0);
  Wire.setSCL(D1);
  //Wire.setClock(10000); // Standard: 100000, fast: 400000, slow: 10000
  Wire.begin();

  pinMode(LED_PIN, OUTPUT);

  seatBeltSignBounce.attach(1, INPUT_PULLUP);

  apuStartBounce.attach(2, INPUT_PULLUP);
  apuMasterBounce.attach(3, INPUT_PULLUP);

  beaconLightBounce.attach(13, INPUT_PULLUP);
  landingLightBounce.attach(9, INPUT_PULLUP);
  navLightBounce.attach(8, INPUT_PULLUP);
  noseLightTakeoffBounce.attach(10, INPUT_PULLUP);
  noseLightOffBounce.attach(11, INPUT_PULLUP);
  runwayTurnoffLightBounce.attach(12, INPUT_PULLUP);
  strobeLightOffBounce.attach(15, INPUT_PULLUP);
  strobeLightOnBounce.attach(14, INPUT_PULLUP);
  wingIceLightBounce.attach(0, INPUT_PULLUP);

#define PUSH_PULL_INIT(control, pushPin, pullPin) \
  control##PushBounce.attach(pushPin, INPUT_PULLUP); \
  control##PullBounce.attach(pullPin, INPUT_PULLUP); \
  control##Qdec.begin(); \
  attachInterrupt(digitalPinToInterrupt(control##PinA), control##RotatedIsr, CHANGE); \
  attachInterrupt(digitalPinToInterrupt(control##PinB), control##RotatedIsr, CHANGE);

  PUSH_PULL_INIT(fcuSpeed, D13, D12)
  PUSH_PULL_INIT(fcuHeading, D16, D17)
  PUSH_PULL_INIT(fcuAlt, D28, D15)
  PUSH_PULL_INIT(fcuVs, D14, D22)
  PUSH_PULL_INIT(baro, D5, D2)

  fcuAltModeBounce.attach(D4, INPUT_PULLUP);
  baroModeBounce.attach(D3, INPUT_PULLUP);

  qwiicButton.begin();

  for (int i = externalLedFirstPin; i <= externalLedLastPin; ++i) {
    io23017->pinDirection(i, OUTPUT);
  }

  lcdRight.init();
  lcdRight.noBacklight();
  lcdRight.setCursor(0, 0);
  lcdRight.print("ALT -LVL/CH- V/S");
  lcdRight.setCursor(0, 1);
  lcdRight.print("1234567890ABCDEF");

  lcdLeft.init();
  lcdLeft.noBacklight();
  lcdLeft.setCursor(0, 0);
  lcdLeft.print("SPD    HDG   LAT");
  lcdLeft.setCursor(0, 1);
  lcdLeft.print("1234567890abcdef");
}

void updateLcds() {
  static int lr = 0;
  lr = (lr + 1) & 3;
  lcdRight.setBacklight(lr == 0);
  lcdLeft.setBacklight(lr == 2);

  mutex_enter_blocking(&mut1to0);
  if (fcuLcdText[0][0]) {
    lcdLeft.setCursor(0, 0);
    lcdLeft.printstr((const char *) fcuLcdText[0]);
    fcuLcdText[0][0] = '\0';
  }
  mutex_exit(&mut1to0);

  mutex_enter_blocking(&mut1to0);
  if (fcuLcdText[1][0]) {
    lcdLeft.setCursor(0, 1);
    lcdLeft.printstr((const char *) fcuLcdText[1]);
    fcuLcdText[1][0] = '\0';
  }
  mutex_exit(&mut1to0);

  mutex_enter_blocking(&mut1to0);
  if (fcuLcdText[2][0]) {
    lcdRight.setCursor(0, 0);
    lcdRight.printstr((const char *) fcuLcdText[2]);
    fcuLcdText[2][0] = '\0';
  }
  mutex_exit(&mut1to0);

  mutex_enter_blocking(&mut1to0);
  if (fcuLcdText[3][0]) {
    lcdRight.setCursor(0, 1);
    lcdRight.printstr((const char *) fcuLcdText[3]);
    fcuLcdText[3][0] = '\0';
  }
  mutex_exit(&mut1to0);
}

short calculateSpoilerHandle() {
#if 0
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
#else
  return 0;
#endif
}

void updateFromInterrupts(void) {
#define PUSH_PULL_UPDATE_FROM_ISR(control) { \
  critical_section_enter_blocking(&isrCritical); \
  short control##DeltaTemp = control##DeltaIsr; \
  control##DeltaIsr = 0; \
  critical_section_exit(&isrCritical); \
  if (control##DeltaTemp) {\
    mutex_enter_blocking(&mut0to1); \
    control##Delta += control##DeltaTemp; \
    mutex_exit(&mut0to1); \
  } \
}

  PUSH_PULL_UPDATE_FROM_ISR(fcuSpeed)
  PUSH_PULL_UPDATE_FROM_ISR(fcuHeading)
  PUSH_PULL_UPDATE_FROM_ISR(fcuAlt)
  PUSH_PULL_UPDATE_FROM_ISR(fcuVs)
  PUSH_PULL_UPDATE_FROM_ISR(baro)
}

const char *booleanAsJson(bool b) {
  return b ? "true" : "false";
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
    beaconLight = booleanAsJson(beaconLightBounce.read());

  if (assumeChanged || wingIceLightBounce.update())
    wingIceLight = booleanAsJson(!wingIceLightBounce.read());

  if (assumeChanged || navLightBounce.update())
    navLight = booleanAsJson(navLightBounce.read());

  if (assumeChanged || runwayTurnoffLightBounce.update())
    runwayTurnoffLight = booleanAsJson(!runwayTurnoffLightBounce.read());

  if (assumeChanged || landingLightBounce.update())
    landingLight = booleanAsJson(!landingLightBounce.read());

  if (assumeChanged || seatBeltSignBounce.update())
    seatBeltSign = booleanAsJson(seatBeltSignBounce.read());

  if (assumeChanged || noseLightTakeoffBounce.update() || noseLightOffBounce.update())
    noseLight = !noseLightTakeoffBounce.read() ? "takeoff" : !noseLightOffBounce.read() ? "off" : "taxi";

  if (assumeChanged || baroModeBounce.update())
    newBaroUnits = currentBaroUnits = baroModeBounce.read() ? baroUnitHPa : baroUnitInHg;

  if (assumeChanged || fcuAltModeBounce.update())
    fcuAltMode = fcuAltModeBounce.read() ? 1000 : 100;
}

void updateMomentaryInputs(void) {
#define UPDATE_PUSH_PULL(control) \
  control##PullBounce.update(); \
  if (!control##Pulled && control##PullBounce.fell()) \
    control##Pulled = true; \
  control##PushBounce.update(); \
  if (!control##Pushed && control##PushBounce.fell()) \
    control##Pushed = true;

  UPDATE_PUSH_PULL(baro)
  UPDATE_PUSH_PULL(fcuSpeed)
  UPDATE_PUSH_PULL(fcuHeading)
  UPDATE_PUSH_PULL(fcuAlt)
  UPDATE_PUSH_PULL(fcuVs)

  apuMasterBounce.update();
  if (!apuMasterPressed && apuMasterBounce.fell())
    apuMasterPressed = true;

  apuStartBounce.update();
  if (!apuStartPressed && apuStartBounce.fell())
    apuStartPressed = true;
}

void updateOuputs(void) {
  digitalWrite(LED_PIN, HIGH);

  qwiicButton.LEDon(fcuAltManaged ? 63 : 0);

  int i = externalLedFirstPin;
  io23017->writeValue(i++, apuStartOn ? HIGH : LOW);
  io23017->writeValue(i++, apuAvail ? HIGH : LOW);
  io23017->writeValue(i++, apuMasterOn ? HIGH : LOW);
  io23017->writeValue(i, apuFault ? HIGH : LOW);
}

void seviceQwiicButton(void) {
  static bool wasPressed = false;
  bool isPressed = qwiicButton.isPressed();
  if (!fcuAltPulled && isPressed && !wasPressed) {
//    fcuAltPulled = true;
  }
  wasPressed = isPressed;
}

void loop(void) {
  updateOuputs();

  ioDeviceSync(io23017); // Why isn't this added to the task manager?
  taskManager.runLoop();

  seviceQwiicButton();
  updateLcds();

  updateContinuousInputs();
  updateMomentaryInputs();
  updateFromInterrupts();
}
