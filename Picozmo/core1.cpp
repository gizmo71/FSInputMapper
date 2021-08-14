#include <Arduino.h>

#include "Picozmo.h"

extern void setup1() {
  sleep_ms(1000);
  // https://www.arduino.cc/reference/en/language/functions/communication/serial/ifserial/
  Serial.begin(115200);
  Serial.println("setup");
}

void serialEvent() {
  while (Serial.available())
    // By default, blocks for up to 1s. https://www.arduino.cc/reference/en/language/functions/communication/serial/settimeout/
    incoming = Serial.read();
    forceUpdate = true;
}

extern void loop1() {
  bool sendNow = sendData;
  if (sendNow) {
    sendData = false;
    //TODO: the buffer isn't infinitely large, so maybe control sending from Controlzmo.
    // https://www.arduino.cc/reference/en/language/functions/communication/serial/flush/ appears to be blocking.
    // https://www.arduino.cc/reference/en/language/functions/communication/serial/availableforwrite/
    Serial.print("pot=");
    Serial.println(pot);

    Serial.print("lightsRunwayTurnoff=");
    Serial.println(s1 == s2 ? "null" : s1 ? "true" : "false");
  }

  sleep_ms(5);
}
