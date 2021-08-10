// https://www.arduino.cc/reference/en/

//TODO: other things to experiment with:
// https://emalliab.wordpress.com/2021/04/18/raspberry-pi-pico-arduino-core-and-timers/
// https://arduino-pico.readthedocs.io/en/latest/multicore.html

const uint LED_PIN = PICO_DEFAULT_LED_PIN;
const uint SWITCH1_PIN = D14;
const uint SWITCH2_PIN = D15;

volatile int s1, s2, pot, incoming;

#undef USE_INTERRUPTS

#ifdef USE_INTERRUPTS
void switch1() {
  s1 = digitalRead(SWITCH1_PIN) == LOW;
}

void switch2() {
  s2 = digitalRead(SWITCH2_PIN) == LOW;
}
#endif

void setup() {
  rp2040.idleOtherCore();
  pinMode(LED_PIN, OUTPUT);
  pinMode(SWITCH1_PIN, INPUT_PULLUP);
  pinMode(SWITCH2_PIN, INPUT_PULLUP);
#ifdef USE_INTERRUPTS
  attachInterrupt(digitalPinToInterrupt(SWITCH1_PIN), switch1, CHANGE);
  attachInterrupt(digitalPinToInterrupt(SWITCH2_PIN), switch2, CHANGE);
#endif
  rp2040.resumeOtherCore();
}

void loop() {
  pot = analogRead(A0);
#ifndef USE_INTERRUPTS
  s1 = digitalRead(SWITCH1_PIN) == LOW;
  s2 = digitalRead(SWITCH2_PIN) == LOW;
#endif
  if (incoming != -1) {
    digitalWrite(LED_PIN, (incoming & 1) ? HIGH : LOW);
    incoming = -1;
  }
  sleep_ms(5);
}

void setup1() {
  // https://www.arduino.cc/reference/en/language/functions/communication/serial/ifserial/
  Serial.begin(115200);
  Serial.println("setup");
}

void loop1() {
  sleep_ms(500);
  incoming = Serial.read();
  Serial.print(pot);
  Serial.print("\t");
  Serial.print(s1);
  Serial.print("/");
  Serial.print(s2);
  Serial.print(" read ");
  Serial.println(incoming);
}
