#include <Arduino.h>
#include <Bounce2.h>

#include "Picozmo.h"

const uint LED_PIN = PICO_DEFAULT_LED_PIN;

static Bounce apuMasterBounce = Bounce();
static Bounce apuStartBounce = Bounce();

void setup() {
  pinMode(LED_PIN, OUTPUT);

  apuStartBounce.attach(D14, INPUT_PULLUP);
  apuMasterBounce.attach(D15, INPUT_PULLUP);
}

void updateSpoilerHandle() {
  static short spoilerHandleRawOld = -1000;
  short spoilerHandleRaw = analogRead(A0);
  if (abs(spoilerHandleRaw - spoilerHandleRawOld) > 20) {
    spoilerHandleRawOld = spoilerHandleRaw;
    if (spoilerHandleRaw > 3800)
      spoilerHandle = -1;
    else if (spoilerHandleRaw > 2100)
      spoilerHandle = min(max(3100 - spoilerHandleRaw, 0) / 20, 50);
    else
      spoilerHandle = min(max(1100 - spoilerHandleRaw, 0) / 20, 50) + 50;
  }
}

void updateContinuousInputs() {
  bool assumeChanged = forceUpdate;
  if (assumeChanged) forceUpdate = false;

  updateSpoilerHandle();
  static short spoilerHandleOld = -2;
  if (assumeChanged || spoilerHandleOld != spoilerHandle) {
    spoilerHandleOld = spoilerHandle;
    sendData = true;
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
  if (!sendData) {
    //TODO: some sort of mutex to stop sending whilst an update might be happening? Do we need to copy the data?
    updateContinuousInputs();
  }
  updateMomentaryInputs();
  updateOuputs();
  sleep_ms(5); //TODO: do we really need this?
}
