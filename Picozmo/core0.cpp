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

static IoAbstractionRef io23017lights = ioFrom23017(0x20);
static IoAbstractionRef io23017board = ioFrom23017(0x21);
//static IoAbstractionRef ioPico = ioUsingArduino();

static IoBounce apuMasterBounce(io23017lights);
static IoBounce apuStartBounce(io23017lights);

static IoBounce seatBeltSignBounce(io23017lights);

static IoBounce wingIceLightBounce(io23017lights);
static IoBounce noseLightOffBounce(io23017lights);
static IoBounce noseLightTakeoffBounce(io23017lights);
static IoBounce runwayTurnoffLightBounce(io23017lights);
static IoBounce landingLightBounce(io23017lights);
static IoBounce strobeLightOffBounce(io23017lights);
static IoBounce strobeLightOnBounce(io23017lights);
static IoBounce beaconLightBounce(io23017lights);
static IoBounce navLightBounce(io23017lights);

#define PPR12 false
#define PPR24 true

static const pinid_t externalLedFirstPin = 4, externalLedLastPin = 7;

static IoBounce baroModeBounce(io23017board);
static IoBounce fcuAltModeBounce(io23017board);
static IoBounce trkFpaBounce(io23017board);
static IoBounce speedMachBounce(io23017board);

static QwiicButton qwiicButton;

static critical_section_t isrCritical;

#define PUSH_PULL_ISR(control, pinA, pinB, is24ppr) \
  static const uint16_t control##PinA = pinA, control##PinB = pinB; \
  static QDecoder control##Qdec(control##PinA, control##PinB, is24ppr); \
  static IoBounce control##PushBounce(io23017board); \
  static IoBounce control##PullBounce(io23017board); \
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

PUSH_PULL_ISR(baro, D2, D3, true)
PUSH_PULL_ISR(fcuSpeed, D5, D4, true)
PUSH_PULL_ISR(fcuHeading, D7, D6, true)
PUSH_PULL_ISR(fcuAlt, D9, D8, true)
PUSH_PULL_ISR(fcuVs, D10, D11, true)

static void initLcd(LiquidCrystal_I2C &lcd) {
  lcd.init();
  // The charset of ours is table 4 from https://www.sparkfun.com/datasheets/LCD/HD44780.pdf#page=17
  lcd.createChar(0, (unsigned char *) "\x1F\x00\x1F\x00\x1F\x00\x1F\x00");
  lcd.createChar(1, (unsigned char *) "\x00\x04\x0E\x1F\x1F\x0E\x04\x00"); // Dot
  lcd.createChar(2, (unsigned char *) "\x00\x1f\x00\x1f\x00\x00\x00\x00");
  lcd.createChar(3, (unsigned char *) "\x00\x00\x1f\x00\x1f\x00\x00\x00");
  lcd.createChar(4, (unsigned char *) "\x00\x00\x00\x0f\x08\x08\x08\x00"); // left level change
  lcd.createChar(5, (unsigned char *) "\x00\x00\x00\x1e\x02\x02\x02\x00"); // right level change
  lcd.createChar(6, (unsigned char *) "\x00\x1f\x00\x00\x1f\x00\x00\x00");
  lcd.createChar(7, (unsigned char *) "\x00\x00\x1f\x00\x00\x00\x1f\x00");
  lcd.home(); // Otherwise the CGRAM might be corrupted by other writes intended for DDRAM.
}

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

  baroModeBounce.attach(0, INPUT_PULLUP);
  PUSH_PULL_INIT(baro, 1, 2)
  PUSH_PULL_INIT(fcuSpeed, 3, 4)
  PUSH_PULL_INIT(fcuHeading, 6, 5)
  PUSH_PULL_INIT(fcuAlt, 8, 7)
  PUSH_PULL_INIT(fcuVs, 9, 10)
  fcuAltModeBounce.attach(11, INPUT_PULLUP);
  trkFpaBounce.attach(12, INPUT_PULLUP);
  speedMachBounce.attach(13, INPUT_PULLUP);

  qwiicButton.begin();

  for (int i = externalLedFirstPin; i <= externalLedLastPin; ++i) {
    io23017lights->pinDirection(i, OUTPUT);
  }

  initLcd(lcdRight);
  initLcd(lcdLeft);
}

static void updateLcds() {
  static int lr = 0;
  lr = (lr + 1) & 63;

  //TODO: get rid of this once we can power the backlight independently.
  lcdRight.setBacklight((lr & 1));
  lcdLeft.setBacklight((lr & 1) == 0);

  LiquidCrystal_I2C &lcdI2C = (lr & 32) == 0 ? lcdLeft : lcdRight;
  char c = ((const char *) fcuLcdText)[lr];
  if ((lr & 15) == 0)
    lcdI2C.setCursor(0, lr & 16 ? 1 : 0);
  lcdI2C.print(c);
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
    newBaroUnits = currentBaroUnits = baroModeBounce.read() ? baroUnitInHg : baroUnitHPa;

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

  trkFpaBounce.update();
  if (!trkFpaToggled && trkFpaBounce.fell())
    trkFpaToggled = true;

  speedMachBounce.update();
  if (!speedMachToggled && speedMachBounce.fell())
    speedMachToggled = true;
}

void updateOuputs(void) {
  digitalWrite(LED_PIN, HIGH);

  qwiicButton.LEDon(fcuAltManaged ? 63 : 0);

  int i = externalLedFirstPin;
  io23017lights->writeValue(i++, apuStartOn ? HIGH : LOW);
  io23017lights->writeValue(i++, apuAvail ? HIGH : LOW);
  io23017lights->writeValue(i++, apuMasterOn ? HIGH : LOW);
  io23017lights->writeValue(i, apuFault ? HIGH : LOW);
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

  // Why aren't these added to the task manager?
  ioDeviceSync(io23017lights);
  ioDeviceSync(io23017board);
  taskManager.runLoop();

  seviceQwiicButton();
  updateLcds();

  updateContinuousInputs();
  updateMomentaryInputs();
  updateFromInterrupts();
}
