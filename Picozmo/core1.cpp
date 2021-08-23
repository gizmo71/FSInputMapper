#include <Arduino.h>

#include "Picozmo.h"

extern void setup1(void) {
  sleep_ms(1000); // Make sure Core 0 runs its setup first.
  Serial.begin(115200);
  Serial.println("setup");
}

void serialEvent(void) {
  while (Serial.available()) {
    // By default, blocks for up to 1s. https://www.arduino.cc/reference/en/language/functions/communication/serial/settimeout/
    int c = Serial.read();
    if (c == 'F')
      forceUpdate = true;
    else
      incoming = c;
  }
}

void sendContinuous(void) {
  if (spoilerHandle != -2) {
    Serial.print("speedBrakeHandle=");
    Serial.println(spoilerHandle);
    spoilerHandle = -2;
  }

  if (strobeLight) {
    Serial.print("lightsStrobe=\"");
    Serial.print(strobeLight);
    Serial.println('"');
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
    Serial.print("lightsNose=\"");
    Serial.print(noseLight);
    Serial.println('"');
    noseLight = NULL;
  }

  short fcuAltDeltaToSend = fcuAltDelta;
  if (fcuAltDelta) {
    Serial.print("fcuAltDelta=");
    Serial.println(fcuAltDeltaToSend);
    mutex_enter_blocking(&mut0to1);
    fcuAltDelta -= fcuAltDeltaToSend;
    mutex_exit(&mut0to1);
  }
}

//TODO: consider FIFO from https://arduino-pico.readthedocs.io/en/latest/multicore.html
void sendMomentary(void) {
  if (apuMasterPressed) {
    apuMasterPressed = false;
    Serial.println("apuMasterPressed=true");
  }

  if (apuStartPressed) {
    apuStartPressed = false;
    Serial.println("apuStartPressed=true");
  }

  if (fcuAltPushed) {
    fcuAltPushed = false;
    Serial.println("fcuAltPushed=true");
  }

  if (fcuAltPulled) {
    fcuAltPulled = false;
    Serial.println("fcuAltPulled=true");
  }
}

extern void loop1(void) {
  sendContinuous();
  sendMomentary();
  sleep_ms(5);
}
