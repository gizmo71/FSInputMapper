// https://www.arduino.cc/reference/en/

//TODO: other things to experiment with:
// https://emalliab.wordpress.com/2021/04/18/raspberry-pi-pico-arduino-core-and-timers/
// https://arduino-pico.readthedocs.io/en/latest/multicore.html

const uint LED_PIN = PICO_DEFAULT_LED_PIN;
const uint POT_PIN = A0;
const uint SWITCH1_PIN = D14;
const uint SWITCH2_PIN = D15;

volatile int s1, s2, pot, incoming;

#define USE_INTERRUPTS 0

#if USE_INTERRUPTS
void switch1() {
  s1 = digitalRead(SWITCH1_PIN) == LOW;
}

void switch2() {
  s2 = digitalRead(SWITCH2_PIN) == LOW;
}
#endif

void setup() {
  pinMode(LED_PIN, OUTPUT);
  pinMode(SWITCH1_PIN, INPUT_PULLUP);
  pinMode(SWITCH2_PIN, INPUT_PULLUP);
#if USE_INTERRUPTS
  switch1();
  attachInterrupt(digitalPinToInterrupt(SWITCH1_PIN), switch1, CHANGE);
  switch2();
  attachInterrupt(digitalPinToInterrupt(SWITCH2_PIN), switch2, CHANGE);
#endif
}

void loop() {
  pot = analogRead(POT_PIN);
#if !USE_INTERRUPTS
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
  sleep_ms(1000);
  // https://www.arduino.cc/reference/en/language/functions/communication/serial/ifserial/
  Serial.begin(115200);
  Serial.println("setup");
}

void loop1() {
  sleep_ms(500);
  incoming = Serial.read();
  Serial.print(pot);
  Serial.print(", ");
  Serial.print(SWITCH1_PIN);
  Serial.print("=");
  Serial.print(s1);
  Serial.print("/");
  Serial.print(SWITCH2_PIN);
  Serial.print("=");
  Serial.print(s2);
#if USE_INTERRUPTS
  Serial.print(" (I)");
#endif
  Serial.print(" read ");
  Serial.println(incoming);
}
