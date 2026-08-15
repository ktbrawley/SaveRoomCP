const int sensorPin = A0;  // analog input pin

void setup() {
  Serial.begin(9600);
}

void loop() {
  int value = analogRead(sensorPin);  // reads 0–1023
  Serial.print("Analog value: ");
  Serial.println(value);
  delay(200);  // small delay so output isn't overwhelming
}