// https://www.arduino.cc/reference/en/

#include <Bounce2.h>

//TODO: other things to experiment with:
// https://emalliab.wordpress.com/2021/04/18/raspberry-pi-pico-arduino-core-and-timers/
// https://arduino-pico.readthedocs.io/en/latest/multicore.html

const uint LED_PIN = PICO_DEFAULT_LED_PIN;

volatile byte s1, s2;
volatile short pot;
volatile int incoming;

Bounce b1 = Bounce();
Bounce b2 = Bounce();

void setup() {
  pinMode(LED_PIN, OUTPUT);

  b1.attach(D14, INPUT_PULLUP);
  b2.attach(D15, INPUT_PULLUP);
}

void loop() {
  pot = analogRead(A0);

  b1.update();
  b2.update();

if (b1.changed()) Serial.println("b1 changed");
  s1 = b1.read() == LOW;
if (b2.changed()) Serial.println("b2 changed");
  s2 = b2.read() == LOW;

  if (incoming != -1) {
    digitalWrite(LED_PIN, (incoming & 1) ? HIGH : LOW);
    incoming = -1;
  }

  sleep_ms(5); //TODO: do we really need this?
}

void setup1() {
  sleep_ms(1000);
  // https://www.arduino.cc/reference/en/language/functions/communication/serial/ifserial/
  Serial.begin(115200);
  Serial.println("setup");
}

void serialEvent() {
  while (Serial.available())
    // By default, blocks for up to 1s. https://www.arduino.cc/reference/en/language/functions/communication/serial/settimeout/
    incoming = Serial.read();
}

//TODO: With the input move to serialEvent, do we actually need this to be on the other core?
void loop1() {
  sleep_ms(500);

  //TODO: the buffer isn't infinitely large, so maybe control sending from Controlzmo.
  // https://www.arduino.cc/reference/en/language/functions/communication/serial/flush/ appears to be blocking.
  // https://www.arduino.cc/reference/en/language/functions/communication/serial/availableforwrite/
  Serial.print(pot);
  Serial.print(", ");
  Serial.print(s1);
  Serial.print("/");
  Serial.print(s2);
Serial.print(" avail "); Serial.print(Serial.availableForWrite());
  Serial.println();
}
