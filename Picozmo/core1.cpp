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

void sendContinuous() {
  bool sendNow = sendData;
  if (sendNow) {
    sendData = false;

    Serial.print("speedBrakeHandle=");
    Serial.println(spoilerHandle);
  }
}

void sendMomentary() {
  if (apuMasterPressed) {
    Serial.println("apuMasterPressed=true");
    apuMasterPressed = false;
  }

  if (apuStartPressed) {
    Serial.println("apuStartPressed=true");
    apuStartPressed = false;
  }
}

extern void loop1() {
  sendContinuous();
  sendMomentary();
  sleep_ms(5);
}
