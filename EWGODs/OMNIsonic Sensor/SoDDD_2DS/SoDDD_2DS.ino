//How to use 3D SoDrD (3-Dimensional Sonar Drone Detection)
//     During the long delay, the Arduino should point straight at where it will be in the GUI
//     The servos are sent to zero and to the middle of rotation before scanning and detection activates
//     You can change how long it stays this way with the variable "startDelay"

//How the code works:
//     The Arduino uses one servo to change horizontal rotation (theta) and one to change vertical rotation (phi). 
//     Since the pulse width is 15 degrees and the servos "should" pan 180 degrees, (~135 in reality,) we divide these
//     two numbers and get 12 measurements. Adding a measurement taken at 0, we obtain 13 columns for each row in 
//     the scan. To make a semi-sphere, we make 13 rows. Each increase in column represents a vertical rotation 
//     change and each row represents a change in horizontal rotation (theta). We treat each element in the 
//     first three scans as "baselines" that are run in the setup. Afterwards, the Arduino continues to scan 
//     the environment and a 20% deviation from all 3 scans corresponds to a detection of an enemy drone. 
//     This 20% number can be changed, but I suggest you run the MATLAB code to see the usual range of error (~1-4%).

//Weird Things:

//     XBee and the regular servo libraries clash. As such, we are limited to the inferior ServoTimer2 library
//     ServoTimer2 pulses the servo in microseconds instead of position, so it is less intutive to work with.
//     I was unable to find a Zigbee library that does work with the normal Servo library we will convert 
//     this time into degrees via some calculations obtained from the normal servo library. 

//Limitations:
//  i.    Arduino ServoTimer2 seems to limit the angle of a regular servo. This was not the
//        case in MATLAB, but two XBee modules got bricked attempting to use XCTU with XBee. 
//        XCTU would allow MATLAB use.
//
//  ii.   The 156-160ms delay for the servos is necessary in Arduino, but MATLAB needs less time. Arduino needs a pulse with value between 544 and 2400 us; however, MATLAB only needs 1500 to 1900 us. A difference of 15 "code degrees" in Arduino is roughly 160 (156 to be exact). In MATLAB, this is far smaller. Also, if you shorten the delay to less than this, the sweep of the servo decreases from 130-140 degrees down to as low as 50 degrees. Use a protractor to recalibrate if you go below 150 ms.
//
//  iii.  I attempted to have the servos take measurements on their way to the max rotation and also on the way 
//        back to minimum rotation (544us and 2400us). I did this using a modulus; however, servos only recalibrate 
//        themselves every time they return to "zero" (544us pulse width). Due to this, I often got 3-5 completely
//        different values for the same exact spot in the real world . Trying stepper motors may alleviate this,
//        but may cause other precision issues.

//  
#include <ServoTimer2.h> 
#include <XBee.h>
#include <Printers.h>
#include <AltSoftSerial.h>
#include "binary.h"
#include <NewPing.h>

#define TRIGGER_PIN  11
#define ECHO_PIN     12
#define MAX_DISTANCE 400 // Maximum distance we want to measure (in centimeters).


NewPing sonar(TRIGGER_PIN, ECHO_PIN, MAX_DISTANCE); // NewPing setup of pins and maximum distance.
const int delaySonar=29;//lowest it can be is 29
// minPulseWidth is 544 and max is 2400. Taken from the regular servo library. Try dividing that range into 180 degrees
const int min=544;
const int max=2400;
const int degree=(max-min)/180;

float degreesPerStep=11.25;
//type for omnidirectional sensors in the coord code is 4. Don't change unless you mess with the Unity GUI code!!
const int type=4; 
//delay for the setup before the scans begin
const int startDelay=500; 

//the delay for the servos can be set as low as 50 ms, but one runs the risk of getting errors and decreasing the 
//sweep angle the further they are from 200ms any change in delay requires a measurement to be taken with a protracto
//r and adjust the "servoMaxDegrees" variable accordingly
//15*(2400-544)/180~155
int delay1=160; 
int delay2=160;
int t=0;
int g=0;
//Arduino only allows for 135 degree servos in the packetScan this will be taken into consideration
int servoMaxDegrees=135; 
float oneRealDegree=(max-min)/servoMaxDegrees;

//these will help in locating the detection in spherical coordinates
int horizontalRotation;
int verticalRotation;

#define DebugSerial Serial // using the Arduino serial monitor for debugging
#define XBeeSerial SoftSerial // using software serial for Xbee comms

//#define servo1pin  13 //verticalRotation servo 
#define horizontalRotationServopin  13 //horizontalRotation servo

//ServoTimer2 servo1;    // declare variables for up to eight servos
ServoTimer2 horizontalRotationServo; 

XBeeWithCallbacks xbee;  
AltSoftSerial SoftSerial;

// the pins for the ultraSonic sensor
//Note: ultrasonic sensors have two kinds of calibration error: linear and constant. 
//Some of the SR04 have a 0.9in mismatch with reality and some increase in error depending on distance. 
//Test all Ultrasonic sensors before to make sure you have the 0.9 inch error
const int trigPin = 11;  
const int echoPin = 12;

long duration;
int distance;
unsigned long measurement;

const int rows = 13;
const int columns = 13;

//arrays 1 through 3 are baselines (13x13 matrix) and scan is 
//the one scanning the already mapped-out areas for a drone
uint8_t baseline1[ rows ][ columns ];
uint8_t baseline2[ rows ][ columns ];
uint8_t baseline3[ rows ][ columns ];
uint8_t baselineDummy[ rows ][ columns ];
bool detection[ rows ][ columns ];
uint8_t scan[ rows ][ columns ];

//data sent to the Zigbee in the correct orderYou can use the arrayPrint function to print this in debugging
int packetSanic[5]; 

//It uses seconds instead of position for its commands. The values in the calculation are 
//taken from the regular Servo library
int oneDegree = (2400-544)/180; //max pulse width in microseconds - min pulse width / 180 degrees

unsigned long last_tx_time = 0; //must use to avoid potential latency issues

//each sensor should have a different sensorID. Change this based off of the label
//if you have no label, make up any number above 9 and label it!
int sensorID=3;

const int yellowLED = A1;//solid LED light means it has not started baseline scans or it is in detection mode. Blinking lights means scanning baselines
const int blueLED = A3; //blink means something was detected
int ledON = 255;
int ledOFF = 0;

void setup () {// the setup attaches the servos, ultrasonic sensor, and serial

  pinMode(yellowLED, OUTPUT);
  pinMode(blueLED, OUTPUT);
  digitalWrite(yellowLED, ledON);
  digitalWrite(blueLED, ledON);
 //servo setup
 // servo1.attach(servo1pin);
  horizontalRotationServo.attach(horizontalRotationServopin);

  //ultrasonic setup
  pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
  pinMode(echoPin, INPUT); // Sets the echoPin as an Input
  horizontalRotationServo.write(0*oneRealDegree+min);
 // servo1.write(0*oneRealDegree+min);
  delay(2500);
  horizontalRotationServo.write(oneRealDegree*servoMaxDegrees/2+min); //pauses it at the midpoint of max sweep and start
 // servo1.write(oneRealDegree*90+min);
  delay(startDelay);
  digitalWrite(yellowLED, ledOFF);
  digitalWrite(blueLED, ledOFF);
  //XBee and regular Serial setup
  Serial.begin(115200); //sets the baud rate of the serial
  Serial.println("start");
  XBeeSerial.begin(9600);
  xbee.begin(XBeeSerial);
  xbee.onPacketError(printErrorCb, (uintptr_t)(Print*)&DebugSerial); //idk. Took this from last year's interns
  xbee.onResponse(printErrorCb, (uintptr_t)(Print*)&DebugSerial);
  delay(startDelay);
  
  digitalWrite(yellowLED, ledOFF);
  //why will there be 3 baselines you may ask? Since the beam for sonar is so wide, 
  //it occasionally will reflect when it hits a corner and returns 2 different values. 
  //On average, by corners, this happened 1/3 of the time. 
  //You can add a 4th scan if you like. Make sure to edit the "detection" if statement (use ctr+f with the word detection)
  ultrasonicScan(trigPin, echoPin, horizontalRotationServo, baseline1);
  printArray(baseline1, "baseline1"); //prints array to serial for debugging
  ultrasonicScan(trigPin, echoPin, horizontalRotationServo, baseline2);
  printArray(baseline2, "baseline2"); 
 // ultrasonicScan(trigPin, echoPin, horizontalRotationServo, baseline3);
 // printArray(baseline3, "baseline3");
  // ultrasonicScan(trigPin, echoPin, horizontalRotationServo, baseline4);
  // printArray(baseline4, "baseline4");
}




void loop () { //the below if the exact same as the ultrasonicScan() sub-routine; 
//however, a zigbee transmission when detections happen has been added
digitalWrite(yellowLED, ledON);
  for (int a = 0; a < 13; a = a + 1) { //this will change the theta value 
   //after ultrasonic measurements have been taken from 0-180 degrees phi
   if (a*oneRealDegree*degreesPerStep+min<max) {
      //servo1.write(a*oneRealDegree*degreesPerStep+min);// why 15?? ecause that is the angle  width of the ultrasonic sensor
      delay(delay1);
      for (int pos = 0; pos < 13; pos = pos + 1) { //scans the 180 degrees of phi at a corresponding theta. 
      //why 13? because 180/15=12 and +1 for the value at phi=0
       if (pos*oneRealDegree*degreesPerStep+min<max) {
         horizontalRotationServo.write(pos*oneRealDegree*degreesPerStep+min);
         delay(delay1);
         scan[a][pos]= takeUltrasonicMeasurement();
         //sendPacket(type, sensorID, scan[a][pos], pos, 0,servoMaxDegrees); //only for debug
         digitalWrite(blueLED, ledOFF);
         if (
         (deviatesEnough(a, pos, scan, baseline1) == true) //makes sure there is 20% deviation from all baselines
            && (deviatesEnough(a, pos, scan, baseline2) == true)
          //  && (deviatesEnough(a, pos, scan, baseline3) == true)
          //  && (deviatesEnough(a, pos, scan, baseline4) == true)
           && (notTooFar(a, pos, scan, baseline1)==true)  //removes values larges than the baselines
            && (notTooFar(a, pos, scan, baseline2)==true)
          //  && (notTooFar(a, pos, scan, baseline3)==true)
          //  && (notTooFar(a, pos, scan, baseline4)==true)
           && (scan[ a ][ pos ] > 1 ) //if a zero is given it cannot be a detection. The drone would be breaking the ultrasonic!
           && (scan[ a ][ pos ] < 150 ) ) //range is approximately 150in. Anything over this cannot be a detection
          {
             //if there is a 20% deviation from all three baselines
             detection[a][pos] = true;
             digitalWrite(blueLED, ledON);
             sendPacket(type, sensorID, scan[a][pos], pos, 0,servoMaxDegrees); //change 0 to the letter pos when doing 3d testing
             //comment the prints out if not debugging
             //Serial.print("detection --- horizontalRotation:"); Serial.print(packetSanic[1][5]); Serial.print(", verticalRotation: "); Serial.print(packetSanic[1][4]); Serial.print(", distance (in.)"); Serial.println(scan[a][pos]);
             //Serial.print(a);Serial.print(",");Serial.print(pos);Serial.print(",");Serial.println(distance);
          }
         
        last_tx_time = millis();
       xbee.loop();
         }
    }
   }
  }
  printArray(scan, "scan");
  printArray(detection, "detection");
}


int ultrasonicScan(int trigPin, int echoPin, ServoTimer2, uint8_t baseline[13][13] ) { //this scans the area with an ultrasonic sensor and makes a 13x13 array "mapping" of the surrounding area for baseline detection
  for (int a = 0; a < 13; a = a + 1) { //this will change the theta value after ultrasonic measurements have been taken from 0-180 degrees phi
    if (a*oneRealDegree*degreesPerStep+min<max) {
     //servo1.write(a*oneRealDegree*degreesPerStep+min);// why 15?? because that is the angle of the ultrasonic sensore
     delay(delay1);
     for (int pos = 0; pos < 13; pos = pos + 1) { //scans the 180 degrees of phi at a corresponding theta
       if (pos*oneRealDegree*degreesPerStep+min<max) {
         horizontalRotationServo.write(pos*degreesPerStep*oneRealDegree+min);
        //  digitalWrite(yellowLED, ledON);
         delay(delay2);
        //  digitalWrite(yellowLED, ledOFF);
          digitalWrite(yellowLED, ledON);
          baseline[a][pos]= takeUltrasonicMeasurement();
          digitalWrite(yellowLED, ledOFF);
          if (baseline[a][pos]<6) {
            baseline[a][pos]=169;
          }
          sendPacket(type, sensorID, baseline[a][pos], pos, 0,servoMaxDegrees);
          //Serial.println(measurement); Serial.println(pos); Serial.println(a); Serial.println(baseline1[a][pos/15]);
          // Serial.print("baseline1, ");Serial.print(a+1);Serial.print(", ");Serial.print(pos/15+1);Serial.print(", ");Serial.println(measurement);
         }
       }
    }
  }
}

int ultrasonicScanWithDummy(int trigPin, int echoPin, ServoTimer2, uint8_t baseline[13][13], uint8_t baselineDummy[13][13] ) { //this scans the area with an ultrasonic sensor and makes a 13x13 array "mapping" of the surrounding area for baseline detection
  for (int a = 0; a < 13; a = a + 1) { //this will change the theta value after ultrasonic measurements have been taken from 0-180 degrees phi
    if (a*oneRealDegree*degreesPerStep+min<max) {
     //servo1.write(a*oneRealDegree*degreesPerStep+min);// why 15?? because that is the angle of the ultrasonic sensore
     delay(delay1);
     for (int pos = 0; pos < 13; pos = pos + 1) { //scans the 180 degrees of phi at a corresponding theta
       if (pos*oneRealDegree*degreesPerStep+min<max) {
         horizontalRotationServo.write(pos*degreesPerStep*oneRealDegree+min);
        //  digitalWrite(yellowLED, ledON);
         delay(delay2);
        //  digitalWrite(yellowLED, ledOFF);
          digitalWrite(yellowLED, ledON);
          baseline[a][pos]= takeUltrasonicMeasurement();
          digitalWrite(yellowLED, ledOFF);
          if (baseline[a][pos]<6) {
            baseline[a][pos]=169;
          }
          sendPacket(type, sensorID, baseline[a][pos], pos, 0,servoMaxDegrees);
          //Serial.println(measurement); Serial.println(pos); Serial.println(a); Serial.println(baseline1[a][pos/15]);
          // Serial.print("baseline1, ");Serial.print(a+1);Serial.print(", ");Serial.print(pos/15+1);Serial.print(", ");Serial.println(measurement);
         }
       }
    }
  }
  for (int timeToSharpen=0; timeToSharpen<2;timeToSharpen=timeToSharpen+1) { //numbver of times baseline is cycled to get the nearest measurement
    for (int a = 0; a < 13; a = a + 1) { //this will change the theta value after ultrasonic measurements have been taken from 0-180 degrees phi
      if (a*oneRealDegree*degreesPerStep+min<max) {
      //servo1.write(a*oneRealDegree*degreesPerStep+min);// why 15?? because that is the angle of the ultrasonic sensore
      delay(delay1);
      for (int pos = 0; pos < 13; pos = pos + 1) { //scans the 180 degrees of phi at a corresponding theta
        if (pos*oneRealDegree*degreesPerStep+min<max) {
          horizontalRotationServo.write(pos*degreesPerStep*oneRealDegree+min);
          //  digitalWrite(yellowLED, ledON);
          delay(delay2);
          //  digitalWrite(yellowLED, ledOFF);
            digitalWrite(yellowLED, ledON);
            baselineDummy[a][pos]= takeUltrasonicMeasurement();
            digitalWrite(yellowLED, ledOFF);
            if (baseline[a][pos]<6) {
              baseline[a][pos]=169;
            }
            baseline[a][pos]=min(baseline[a][pos],baselineDummy[a][pos]);
            sendPacket(type, sensorID, baseline[a][pos], pos, 0,servoMaxDegrees);
            //Serial.println(measurement); Serial.println(pos); Serial.println(a); Serial.println(baseline1[a][pos/15]);
            // Serial.print("baseline1, ");Serial.print(a+1);Serial.print(", ");Serial.print(pos/15+1);Serial.print(", ");Serial.println(measurement);
          }
        }
      }
    }
  }
}

int takeUltrasonicMeasurement() {
  int g=0;
  delay(29);                    // Wait 50ms between pings (about 20 pings/sec). 29ms should be the shortest delay between pings.

  int distance = sonar.ping_cm()/2.54; // Send ping, get distance in cm and print result (0 = outside set distance range)
 while (distance==0 && g<10){
   distance = sonar.ping_cm()/2.54;
   g=g+1;
   }
  //Serial.print("Distance: ");
  //Serial.print(distance);
  return distance;
  //Serial.println("in");
}

void printArray(uint8_t baseline[13][13], char name[]) //MATLAB-like matrix printing
{
  Serial.println(name);
  Serial.println("_                                        _"); 
  Serial.println("|                                        |");
  for (int a = 0; a < rows; a++) { //prints each row
  Serial.print("|");
   for (int b = 0; b < columns; b++) { //prints each column
     if (b==columns-1) { 
        Serial.print(baseline[a][b]); //hits enter after a row is finished
        Serial.println("|");
       } 
     else {
        Serial.print(baseline[a][b]);
        Serial.print(",");
       } 
   }
  }
  Serial.println();
}

void printArray(bool baseline[13][13], char name[]) //MATLAB-like matrix printing
{
  Serial.println(name);
  Serial.println("_                                        _"); 
  Serial.println("|                                        |");
  for (int a = 0; a < rows; a++) { //prints each row
  Serial.print("|");
   for (int b = 0; b < columns; b++) { //prints each column
     if (b==columns-1) { 
        Serial.print(baseline[a][b]); //hits enter after a row is finished
        Serial.println("|");
       } 
     else {
        Serial.print(baseline[a][b]);
        Serial.print(",");
       } 
   }
  }
  Serial.println();
}


int sendPacket(int type, int sensorID, int distance, int theta,int phi, int servoMaxDegrees) { //, int servoMaxDegrees

    // Prepare the Zigbee Transmit Request API packet
    horizontalRotation=theta;
   // Serial.print("1 "); Serial.println(horizontalRotation); debug
    verticalRotation=phi;
    horizontalRotation=horizontalRotation*degreesPerStep;
    //Serial.print("2 "); Serial.println(horizontalRotation); debug
    verticalRotation=verticalRotation*degreesPerStep;
    horizontalRotation=horizontalRotation-servoMaxDegrees/2; //converting it to the maximum the servo will turn in arduino
    horizontalRotation=-1*horizontalRotation;
    //the second term, 180-servoMaxDegrees/2 shifts the degrees of detection to the proper starting and end point
    //Serial.print("3 "); Serial.println(horizontalRotation); //debug
    verticalRotation=verticalRotation;//-(180-servoMaxDegrees/2); 
    // horizontalRotation=90-horizontalRotation; //centering the sensor in the GUI as 0 degrees in the Arduino output
    verticalRotation=90-verticalRotation;
    if(verticalRotation<0) { //verticalRotation is normally defined from 0 to 180 degrees
      verticalRotation=-1*verticalRotation;
      horizontalRotation=180+horizontalRotation;
      //Serial.print("4 "); Serial.println(horizontalRotation);
      if (horizontalRotation>360) {
        horizontalRotation=horizontalRotation-360;
       //Serial.print("5 "); Serial.println(horizontalRotation);
      }
    }
    packetSanic[0]=type;
    packetSanic[1]=sensorID;
    packetSanic[2]=distance;
    packetSanic[2]=verticalRotation;
    packetSanic[4]=horizontalRotation;
    Serial.print("detection:");
    Serial.print(packetSanic[0]);Serial.print(",");Serial.print(packetSanic[1]);Serial.print(",");
    Serial.print(packetSanic[2]);Serial.print(",");Serial.print(packetSanic[3]);Serial.print(",");Serial.println(packetSanic[4]);
    ZBTxRequest txRequest;
    txRequest.setAddress64(0x0013A20041C165F8);

    // Allocate <X> payload bytes: 1 type byte plus however many other data bytes needed
    AllocBuffer<9> packet; 
    
    // Packet type & payload data items
    packet.append<uint8_t>(type); //type
    packet.append<int>(sensorID); 
    //Serial.println("hi");
    //Serial.println(distance);
    packet.append<int>(distance);
    packet.append<int>(horizontalRotation);
    packet.append<int>(verticalRotation);
    // Can add on another append<> for each additional data item, but payload bytes must be adjusted. 
    txRequest.setPayload(packet.head, packet.len());

    // Send the packet
    xbee.send(txRequest);
}

void printPacket(int sanicPacket[1][5], char name[]) //MATLAB-like matrix printing
{
  Serial.println(name);
  Serial.println("_                                        _"); 
  Serial.println("|                                        |");
  for (int a = 0; a < 1; a++) { //prints each row
  Serial.print("|");
   for (int b = 0; b < 5; b++) { //prints each column
     if (b==4) { 
        Serial.print(sanicPacket[a][b]); //hits enter after a row is finished
        Serial.println("|");
       } 
     else {
        Serial.print(sanicPacket[a][b]);
        Serial.print(",");
       } 
   }
  }
  Serial.println();
  Serial.println("clear");
}

bool deviatesEnough(int a, int pos, uint8_t scan[13][13], uint8_t baseline[13][13]) {
 for (int dummy = 0; dummy <12; dummy=dummy+1) {
   if ( abs( scan[a][pos] - baseline[dummy][pos] ) > .1 * baseline[dummy][pos] ) {}
    else {
      bool enemySpotted=false;
      return enemySpotted;
      }
   }
   bool enemySpotted=true;
   return enemySpotted;
   }

bool notTooFar(int a, int pos, uint8_t scan[13][13], uint8_t baseline[13][13]) {
 for (int dummy = 0; dummy <12; dummy=dummy+1) {
   if ( ( scan[ a ][ pos ] - baseline[ dummy ][ pos ] )< (.1 * baseline[ dummy ][ pos ] ) )  {}
   else {
      bool enemyIsCloseEnough=false;
      return enemyIsCloseEnough;
      }
   }
   bool enemyIsCloseEnough=true;
   return enemyIsCloseEnough;
   }
// Change-log:
//1.7:  Changed everything to real degrees instead of what the servo thinks is a degree. 
//      This way, the Arduino will only take a measurement once every 15 real degrees. I
//      also added if statements that ignore the servo positions altogether if the position
//      would be above the max pulse width. This means that the matrix returned will have 
//      zeros in areas that cannot be scanned by the Arduino. Every element in the matrix
//      now directly corresponds with a 15 degree change in vertical or horizontal rotation
//      in reality. Additionally, there should not be any overlapping parts of a previous scan. This should address