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
  if (spoilerHandle != -2) {
    Serial.print("speedBrakeHandle=");
    Serial.println(spoilerHandle);
    spoilerHandle = -2;
  }

  if (strobeLight) {
    Serial.print("lightsStrobe=");
    Serial.println(strobeLight);
    strobeLight = NULL;
  }

  if (beaconLight) {
    Serial.print("beaconLight=");
    Serial.println(beaconLight);
    beaconLight = NULL;
  }

  if (wingIceLight) {
    Serial.print("wingIceLight=");
    Serial.println(wingIceLight);
    wingIceLight = NULL;
  }

  if (navLight) {
    Serial.print("lightsNavLogo=");
    Serial.println(navLight);
    navLight = NULL;
  }

  if (runwayTurnoffLight) {
    Serial.print("lightsRunwayTurnoff=");
    Serial.println(runwayTurnoffLight);
    runwayTurnoffLight = NULL;
  }

  if (landingLight) {
    Serial.print("lightsLanding=");
    Serial.println(landingLight);
    landingLight = NULL;
  }

  if (noseLight) {
    Serial.print("lightsNose=");
    Serial.println(noseLight);
    noseLight = NULL;
  }
}

void sendMomentary() {
  if (apuMasterPressed) {
    apuMasterPressed = false;
    Serial.println("apuMasterPressed=true");
  }

  if (apuStartPressed) {
    apuStartPressed = false;
    Serial.println("apuStartPressed=true");
  }
}

extern void loop1() {
  sendContinuous();
  sendMomentary();
  sleep_ms(5);
}
