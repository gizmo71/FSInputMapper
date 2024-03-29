#include <Arduino.h>

#include "Picozmo.h"

extern void setup1(void) {
  sleep_ms(1000); // Make sure Core 0 runs its setup first.
  Serial.setTimeout(100);
  Serial.begin(115200);
}

// First character is 'S'tandard, Q'N'H or Q'F'E.
volatile char desiredBaro[6] = "N2992";

void process(String name, String value) {
  if (name == "ApuFault")
    apuFault = value == "True";
  else if (name == "ApuMasterOn")
    apuMasterOn = value == "True";
  else if (name == "ApuAvail")
    apuAvail = value == "True";
  else if (name == "ApuStartOn")
    apuStartOn = value == "True";
  else if (name == "FcuAltManaged")
    fcuAltManaged = value == "True";
  else if (name == "baro" && value.length() == 5) {
    strcpy((char *) desiredBaro, value.c_str());
  } else if (name == "fcuTL" && value.length() == 16) {
    memcpy((void *) fcuLcdText[0], value.c_str(), 16);
  } else if (name == "fcuBL" && value.length() == 16) {
    memcpy((void *) fcuLcdText[1], value.c_str(), 16);
  } else if (name == "fcuTR" && value.length() == 16) {
    memcpy((void *) fcuLcdText[2], value.c_str(), 16);
  } else if (name == "fcuBR" && value.length() == 16) {
    memcpy((void *) fcuLcdText[3], value.c_str(), 16);
  } else {
    Serial.print("# Don't know what '");
    Serial.print(name);
    Serial.print("' is to set it to '");
    Serial.print(value);
    Serial.println("'");
  }
}

void serialEvent(void) {
  while (Serial.available()) {
    String s = Serial.readStringUntil('\n');
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

  if (seatBeltSign) {
    Serial.print("seatBeltSign=");
    Serial.println(seatBeltSign);
    seatBeltSign = NULL;
  }

  if (fcuAltMode) {
    Serial.print("fcuAltIncrement=");
    Serial.println(fcuAltMode);
    fcuAltMode = 0;
  }

  {
    short fcuSpeedDeltaToSend = fcuSpeedDelta;
    if (fcuSpeedDelta) {
      Serial.print("fcuSpeedDelta=");
      Serial.println(fcuSpeedDeltaToSend);
      mutex_enter_blocking(&mut0to1);
      fcuSpeedDelta -= fcuSpeedDeltaToSend;
      mutex_exit(&mut0to1);
    }
  }

  {
    short fcuHeadingDeltaToSend = fcuHeadingDelta;
    if (fcuHeadingDelta) {
      Serial.print("fcuHeadingDelta=");
      Serial.println(fcuHeadingDeltaToSend);
      mutex_enter_blocking(&mut0to1);
      fcuHeadingDelta -= fcuHeadingDeltaToSend;
      mutex_exit(&mut0to1);
    }
  }

  {
    short fcuAltDeltaToSend = fcuAltDelta;
    if (fcuAltDelta) {
      Serial.print("fcuAltDelta=");
      Serial.println(fcuAltDeltaToSend);
      mutex_enter_blocking(&mut0to1);
      fcuAltDelta -= fcuAltDeltaToSend;
      mutex_exit(&mut0to1);
    }
  }

  {
    short fcuVsDeltaToSend = fcuVsDelta;
    if (fcuVsDelta) {
      Serial.print("fcuVsDelta=");
      Serial.println(fcuVsDeltaToSend);
      mutex_enter_blocking(&mut0to1);
      fcuVsDelta -= fcuVsDeltaToSend;
      mutex_exit(&mut0to1);
    }
  }

  {
    short baroDeltaToSend = baroDelta;
    if (baroDeltaToSend) {
      mutex_enter_blocking(&mut0to1);
      baroDelta -= baroDeltaToSend;
      mutex_exit(&mut0to1);
      short single = baroDeltaToSend < 0 ? -1 : 1;
      while (baroDeltaToSend) {
        Serial.print("baroKnob=\"");
        Serial.print(currentBaroUnits);
        Serial.print(single < 0 ? "Dec" : "Inc");
        Serial.println('"');
        baroDeltaToSend -= single;
      }
    }
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

  if (fcuSpeedPushed) {
    fcuSpeedPushed = false;
    Serial.println("fcuSpeedPushed=true");
  }

  if (fcuSpeedPulled) {
    fcuSpeedPulled = false;
    Serial.println("fcuSpeedPulled=true");
  }

  if (fcuHeadingPushed) {
    fcuHeadingPushed = false;
    Serial.println("fcuHeadingPushed=true");
  }

  if (fcuHeadingPulled) {
    fcuHeadingPulled = false;
    Serial.println("fcuHeadingPulled=true");
  }

  if (fcuAltPushed) {
    fcuAltPushed = false;
    Serial.println("fcuAltPushed=true");
  }

  if (fcuAltPulled) {
    fcuAltPulled = false;
    Serial.println("fcuAltPulled=true");
  }

  if (fcuVsPushed) {
    fcuVsPushed = false;
    Serial.println("fcuVsPushed=true");
  }

  if (fcuVsPulled) {
    fcuVsPulled = false;
    Serial.println("fcuVsPulled=true");
  }

  if (baroPulled) {
    baroPulled = false;
    Serial.println("baroKnob=\"pull\"");
  }

  if (baroPushed) {
    baroPushed = false;
    Serial.println("baroKnob=\"push\"");
  }

  if (newBaroUnits) {
    newBaroUnits = NULL;
    Serial.print("baroKnob=\"");
    Serial.print(currentBaroUnits);
    Serial.println("\"");
  }

  if (trkFpaToggled) {
    Serial.println("trkFpaToggled=true");
    trkFpaToggled = false;
  }

  if (speedMachToggled) {
    Serial.println("speedMachToggled=true");
    speedMachToggled = false;
  }
}

extern void loop1(void) {
  sendContinuous();
  sendMomentary();
  sleep_ms(5);
}
