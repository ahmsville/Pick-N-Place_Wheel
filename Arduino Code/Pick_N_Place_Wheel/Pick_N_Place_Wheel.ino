#include <MagRotaryEncoding.h>
#include <AdvCapTouch.h>
#include <FastLED.h>
/*

   ....By Ahmsville...
*/

//Interrupt based detection
MagRotaryEncoder wheel1 = MagRotaryEncoder(A0, A1, 0, 1); // create new encoding instance and specify the arduino pins connected to the hall effect sensors, and the pins use for interrupts
MagRotaryEncoder wheel2 = MagRotaryEncoder(A2, A3, 5, 10); // create new encoding instance and specify the arduino pins connected to the hall effect sensors, and the pins use for interrupts

//MagRotaryEncoder wheel1 = MagRotaryEncoder(A0, A1); // create new encoding instance and specify the arduino pins connected to the hall effect sensors, and the pins use for interrupts
//MagRotaryEncoder wheel2 = MagRotaryEncoder(A2, A3); // create new encoding instance and specify the arduino pins connected to the hall effect sensors, and the pins use for interrupts

volatile int activesensor1Interrupt = 1; //used for switching active ISR
volatile int activesensor2Interrupt = 1; //used for switching active ISR

void ISR1 () {
  if (activesensor1Interrupt == 1) { //sensor 1 interrupt is active.
        activesensor1Interrupt = wheel1.sensor1_INT();  
  }
}
void ISR2 () {
  if (activesensor1Interrupt == 2) { //sensor 1 interrupt is active.
       activesensor1Interrupt = wheel1.sensor2_INT();
  }
}


void ISR3 () {
  if (activesensor2Interrupt == 1) { //sensor 1 interrupt is active.
        activesensor2Interrupt = wheel2.sensor1_INT();  
  }
}
void ISR4 () {
  if (activesensor2Interrupt == 2) { //sensor 1 interrupt is active.
       activesensor2Interrupt = wheel2.sensor2_INT();
  }
}

void setinterrupt() {
  pinMode(wheel1.get_sensorINTpin(1), INPUT);
  attachInterrupt(digitalPinToInterrupt(wheel1.get_sensorINTpin(1)), ISR1, RISING);

  pinMode(wheel1.get_sensorINTpin(2), INPUT);
  attachInterrupt(digitalPinToInterrupt(wheel1.get_sensorINTpin(2)), ISR2, RISING);

  pinMode(wheel2.get_sensorINTpin(1), INPUT);
  attachInterrupt(digitalPinToInterrupt(wheel2.get_sensorINTpin(1)), ISR3, RISING);

  pinMode(wheel2.get_sensorINTpin(2), INPUT);
  attachInterrupt(digitalPinToInterrupt(wheel2.get_sensorINTpin(2)), ISR4, RISING);

}



int inByte;         // incoming SerialUSB byte
String inString = "";

int touchtype;

int Wheel1_Slot = 1;
int Wheel1_SlotCount = 48;
int retstep1;
bool Wheel1reset = false;
String wheel1outstring = "w1x";

int Wheel2_Slot = 1;
int Wheel2_SlotCount = 24;
int retstep2;
bool Wheel2reset = false;
String wheel2outstring = "w2x";

int COUNT = 0;

#define NUM_LEDS 2

CRGBArray<NUM_LEDS> leds;

void setup() {

  setinterrupt();

  wheel1.set_haptics(9, 50, 255); //set haptic feedback variables (arduino pwm pin, duration of haptics(ms), pwn strength from 0-255)
  wheel1.initialize_encoder();
  wheel2.set_haptics(9, 50, 255); //set haptic feedback variables (arduino pwm pin, duration of haptics(ms), pwn strength from 0-255)
  wheel2.initialize_encoder();
  wheel1.set_encoderResolution(Wheel1_SlotCount);
  //wheel1.setToStart();
  wheel2.set_encoderResolution(Wheel2_SlotCount);
  // wheel2.setToStart();
  wheel2.setResistorDivider(10, 1.2, 3.3);
  wheel1.setResistorDivider(10, 1.2, 3.3);

  SerialUSB.begin(250000);

  //pinMode(8, INPUT);//wheel 1 button
  //pinMode(9, INPUT);//wheel 2 button

  FastLED.addLeds<NEOPIXEL, 3>(leds, NUM_LEDS);
  leds[0] = CRGB::Blue;
  leds[1] = CRGB::Blue;
  delay(1000);
  FastLED.show();

  leds[0] = CRGB::Green;
  leds[1] = CRGB::Green;
  delay(1000);
  FastLED.show();

  leds[0] = CRGB::Red;
  leds[1] = CRGB::Red;
  delay(1000);
  FastLED.show();
}

void loop() {

  retstep2 = wheel2.detect_rotation();  // function returns a signed integer based on rotation step detected
  retstep1 = wheel1.detect_rotation();  // function returns a signed integer based on rotation step detected
  setLedState();
  /*
    if (retstep1 != 0 || retstep2 != 0) {
      SerialUSB.print("\t");
      SerialUSB.print(retstep1);
      SerialUSB.print("\t");
      SerialUSB.println(retstep2);
    }
    /*
    if (retstep1 == -1 || retstep2 == -1) {
      digitalWrite(10, 1);
    } else {
      digitalWrite(10, 0);
    }
  */
  sendwheelposition();

  connecttoPC ();



  /*********************************************************************************************************************/
  //TOUCH DETECTION
  // touchtype = touchpad.detect_touchFromNoise(0);  //function return 1-4 based on the input detected, 1 = singletap, 2 = doubletap, 3 = shortpress, 4 = longpress
  if (touchtype == 1) {
    resetwheel1();
  }
  else if (touchtype == 2) {
    resetwheel2();
  }


  /*****************************************************LedRing Animations***************************************************************/

}
void resetwheel1() {

  Wheel1_Slot = wheel1.setToStart();
  wheel1outstring = "w1_";
  Wheel1reset = true;
  SerialUSB.print(wheel1outstring + "001");
}
void resetwheel2() {

  Wheel2_Slot = wheel2.setToStart();
  wheel2outstring = "w2_";
  Wheel2reset = true;
  SerialUSB.print(wheel2outstring + "001");
}
void sendwheelposition() {
  if (retstep1 != 0) {
    if (retstep1 == -1) {
      wheel1outstring = "w1x";
    }
    Wheel1_Slot = retstep1;
    String slotnumPadded = "";
    if (Wheel1_Slot < 100 && Wheel1_Slot > 9) {
      slotnumPadded += "0";
      slotnumPadded += Wheel1_Slot;
    }
    else if (Wheel1_Slot < 10) {
      slotnumPadded += "00";
      slotnumPadded += Wheel1_Slot;
    }
    SerialUSB.print(wheel1outstring + slotnumPadded);
    retstep1 = 0;
  }

  if (retstep2 != 0) {
    if (retstep2 == -1) {
      wheel2outstring = "w2x";
    }
    Wheel2_Slot = retstep2;
    String slotnumPadded = "";
    if (Wheel2_Slot < 100 && Wheel2_Slot > 9) {
      slotnumPadded += "0";
      slotnumPadded += Wheel2_Slot;
    }
    else if (Wheel2_Slot < 10) {
      slotnumPadded += "00";
      slotnumPadded += Wheel2_Slot;
    }
    SerialUSB.print(wheel2outstring + slotnumPadded);
    retstep2 = 0;
  }

}



void setLedState() {
  if (wheel1outstring.startsWith("w1x")) {
    leds[0] = CRGB::Red;

  }
  else if (wheel1outstring.startsWith("w1_")) {
     if (retstep1 != 0) {
    if (retstep1 == 1) {
      leds[0] = CRGB::Blue;
    }else{
      leds[0] = CRGB::Green;
  }
  }
  }

  /***************/
  if (wheel2outstring.startsWith("w2x")) {
    leds[1] = CRGB::Red;
  }
  else if (wheel2outstring.startsWith("w2_")) {
     if (retstep2 != 0) {
    if (retstep2 == 1) {
      leds[1] = CRGB::Blue;
    }else{
      leds[1] = CRGB::Green;
  }
  }
  }

  FastLED.show();
}

void connecttoPC () {
  // reply only when you receive data:
  while (SerialUSB.available() > 0) {
    delay(2);
    //inByte = SerialUSB.read();
    inString = SerialUSB.readStringUntil('*');



    if (inString != "") {
      //SerialUSB.println("     " + inString + "       ");
      if (inString == "p") {  //connection byte
        inString = "";

        SerialUSB.print("ALPnPW"); //reply with your dial type

      } else if (inString == "rsw1") {
        resetwheel1();
        inString = "";
      } else if (inString == "rsw2") {
        resetwheel2();
        inString = "";
      } else if (inString.startsWith("w1=") && inString.endsWith("xx")) {
        //set wheel1 resolution
        inString.replace("w1=", "");
        inString.replace("xx", "");
        Wheel1_SlotCount = inString.toInt();
        wheel1.set_encoderResolution(Wheel1_SlotCount);
        if (Wheel1_SlotCount != 48) {
          //wheel1.set_bound(80);
          wheel1.invertCount(true);
        } else {
          //wheel1.set_bound(10);
          wheel1.invertCount(false);
        }
        wheel1outstring = "w1x";
        SerialUSB.print(wheel1outstring + "001");
        Wheel1reset = false;
        //SerialUSB.print(inString.toInt());
        inString = "";
        //SerialUSB.print("ack111");
      } else if (inString.startsWith("w2=") && inString.endsWith("xx")) {
        //set wheel2 resolution
        inString.replace("w2=", "");
        inString.replace("xx", "");
        Wheel2_SlotCount = inString.toInt();
        wheel2.set_encoderResolution(Wheel2_SlotCount);
        if (Wheel2_SlotCount != 24) {
          //wheel2.set_bound(38);
          wheel2.invertCount(true);
        } else {
          //wheel2.set_bound(10);
          wheel2.invertCount(false);
        }

        wheel2outstring = "w2x";
        SerialUSB.print(wheel2outstring + "001");
        Wheel2reset = false;
        //SerialUSB.print(inString.toInt());
        inString = "";
        //SerialUSB.print("ack111");
      }
      else {
        inString = "";
      }
    }
  }
}

void disableInterrupt(bool act, int wheelnum) {
  if (act) {
    if (wheelnum == 1) {
      detachInterrupt(digitalPinToInterrupt(0));
      detachInterrupt(digitalPinToInterrupt(1));
    } else if (wheelnum == 2) {
      detachInterrupt(digitalPinToInterrupt(5));
      detachInterrupt(digitalPinToInterrupt(10));
    }


  } else {
    if (wheelnum == 1) {
      pinMode(wheel1.get_sensorINTpin(1), INPUT);
      attachInterrupt(digitalPinToInterrupt(wheel1.get_sensorINTpin(1)), ISR1, CHANGE);

      pinMode(wheel1.get_sensorINTpin(2), INPUT);
      attachInterrupt(digitalPinToInterrupt(wheel1.get_sensorINTpin(2)), ISR2, CHANGE);


    } else if (wheelnum == 2) {
      pinMode(wheel2.get_sensorINTpin(1), INPUT);
      attachInterrupt(digitalPinToInterrupt(wheel2.get_sensorINTpin(1)), ISR3, CHANGE);

      pinMode(wheel2.get_sensorINTpin(2), INPUT);
      attachInterrupt(digitalPinToInterrupt(wheel2.get_sensorINTpin(2)), ISR4, CHANGE);
    }
  }
}
