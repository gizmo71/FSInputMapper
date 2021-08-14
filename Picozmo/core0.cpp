#include <Arduino.h>
#include <Bounce2.h>

#include "Picozmo.h"

const uint LED_PIN = PICO_DEFAULT_LED_PIN;

static Bounce b1 = Bounce();
static Bounce b2 = Bounce();

void setup() {
  pinMode(LED_PIN, OUTPUT);

  b1.attach(D14, INPUT_PULLUP);
  b2.attach(D15, INPUT_PULLUP);
}

void updateInputs() {
  bool assumeChanged = forceUpdate;
  if (assumeChanged) forceUpdate = false;

  //TODO: consider what happens near the extremes.
  short potNew = analogRead(A0);
  if (assumeChanged || abs(pot - potNew) > 40) {
    pot = potNew;
    sendData = true;
  }

  b1.update();
  b2.update();
  if (assumeChanged || b1.changed() || b2.changed()) {
    s1 = b1.read();
    s2 = b2.read();
    sendData = true;
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
    updateInputs();
  }
  updateOuputs();
  sleep_ms(5); //TODO: do we really need this?
}
