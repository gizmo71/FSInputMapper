#include <Arduino.h>

#include "Picozmo.h"

extern void setup1(void) {
  sleep_ms(1000); // Make sure Core 0 runs its setup first.
  Serial.setTimeout(100);
  Serial.begin(115200);
}

void process(String name, String value) {
  if (name == "ApuFault")
    apuFault = value == "true";
  else if (name == "ApuMasterOn")
    apuMasterOn = value == "true";
  else if (name == "ApuAvail")
    apuAvail = value == "true";
  else if (name == "ApuStartOn")
    apuStartOn = value == "true";
  else if (name == "FcuAltManaged")
    fcuAltManaged = value == "true";
  else {
    Serial.print("# Don't know what ");
    Serial.print(name);
    Serial.print(" is to set it to ");
    Serial.println(value);
  }
}

void serialEvent(void) {
  while (Serial.available()) {
    String s = Serial.readString();
    s.trim();
    int split = s.indexOf("=");
    if (s == "SyncInputs")
      forceUpdate = true;
    else if (s == "Scan")
      scanI2C();
    else if (split > 0 && split < s.length() - 1)
      process(s.substring(0, split), s.substring(split + 1));
    else {
      Serial.print("# unknown instruction ");
      Serial.println(s);
    }
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
