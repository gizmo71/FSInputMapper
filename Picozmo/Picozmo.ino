// https://www.arduino.cc/reference/en/

//TODO: other things to experiment with:
// https://emalliab.wordpress.com/2021/04/18/raspberry-pi-pico-arduino-core-and-timers/
// https://arduino-pico.readthedocs.io/en/latest/multicore.html

const uint LED_PIN = PICO_DEFAULT_LED_PIN;
const uint POT_PIN = A0;
const uint SWITCH1_PIN = D14;
const uint SWITCH2_PIN = D15;

volatile int s1, s2, pot, incoming;

// Discussion: https://github.com/earlephilhower/arduino-pico/discussions/276
#define USE_INTERRUPTS 1

#if USE_INTERRUPTS
void switch1(uint gpio, uint32_t events) {
  static int state = 0;
  state = !state;
  digitalWrite(LED_PIN, state ? HIGH : LOW);
}
#endif

void setup() {
  pinMode(LED_PIN, OUTPUT);
  pinMode(SWITCH1_PIN, INPUT_PULLUP);
  pinMode(SWITCH2_PIN, INPUT_PULLUP);
#if USE_INTERRUPTS
  gpio_set_irq_enabled_with_callback(14, GPIO_IRQ_EDGE_RISE | GPIO_IRQ_EDGE_FALL, true, &switch1);
  //gpio_set_irq_enabled_with_callback(15, CHANGE, true, switch1);
#endif
}

void loop() {
  pot = analogRead(POT_PIN);
#if !USE_INTERRUPTS
  s1 = digitalRead(SWITCH1_PIN) == LOW;
  s2 = digitalRead(SWITCH2_PIN) == LOW;
#endif
  if (incoming != -1) {
    //digitalWrite(LED_PIN, (incoming & 1) ? HIGH : LOW);
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
