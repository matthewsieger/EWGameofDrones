using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*

// sample code for how to add custom sensor types to the GUI

// the name used for the class is the name that will be used in the GUI
// in Assets/Resources/Images you must place a .png with the same name as this class
//		this png will be the image representing this type of sensor
public class SensorTemplate : Sensor
{
	// The default constructor must be defined in order to set the type the sensor is
	public LIDAR()
	{
		// The first byte of sensor packets received over the XBee network is a number corresponding
		// with the type of sensor the packet is for. Set that number here so the GUI will know to
		// send packets matching the type to this class.
		// do not have more than one sensor type using the same type unless they use the same code 
		//	(see LIDAR.cs and Ultrasonic.cs)
		Type = 1;	// 1 is for LIDAR and Ultrasonic sensors. Set this to match your sensor type
	}
	
	// you must define this method
	// it takes in a ping object (see Ping.cs) and calculates where on the
	// screen a ping should display
	// returns a Vector3 (a 3D coordinate. See Unity API documentation) with units in Unity Units (see CmToScreen and FeetToPixels)
    protected override Vector3 CalculatePing(Ping ping)
	{
		// for examples of calculating ping coordinates, see LIDAR.cs and OMNISonic.cs
		
		// There are methods and properties provided to you to help with you calculations:
		//
		// 	SensorData is a property containing the SensorConfiguration (see SensorConfiguration.cs) for
		//		the sensor matching the ping's id
		//
		//	CmToScreen(float cm): float		converts a given value in centimeters to a value in Unity units 
		//		representing the same distance
		//
		//	Cage.FeetToPixels(float feet): float	converts a given value in feet to a value in Unity units
		//		representing the same distance
		//
		//	GetSensorX(SensorConfiguration config): float	calculates the x dimension of the sensor matching the
		// 		ping in Unity units
		//
		//	GetSensorY(SensorConfiguration config): float	calculates the y dimension of the sensor matching the
		//		ping in Unity units
		//
		//	AddSensorPosition(Vector3 coord): Vector3	adds the position in Unity units of the sensor corresponding
		//		to the ping to the provided Vector
		//
		//	ApplySensorAngle(Vector3 coord): Vector3	rotates the coordinate about (0, 0) to match the physical
		//		rotation of the sensor. Do not use if the sensor can pick up pings above or below its plane
		//
		//	ApplySensorAngle(Vector3 pingCoord, Vector3 sensorCoord): Vector2	rotates the coordinate
		//		about the sensor's coordinates to match the physical rotation of the sensor. Do not
		//		use if the sensor can pick up pings above or below its plane
		//
		// See Unity.Mathf API documentation online to find trigonometric functions and other mathematical tools
		//
		// At the end of the function, return the Vector3 you have calculated
	}
	
	// generate a ping packet (preferably random) that will be displayed while jamming
	protected override Ping CalculateJamPing()
	{
		string jamPacket = ""
		
		jamPacket += Type.ToString();
		jamPacket += ",";
		jamPacket += SensorData.id;
		
		// fill in the rest of the packet here
		// fill the packet with data in the correct format for pings matching this sensor's type
		
		// UnityEngine.Random.value generates a random float between 0.0 and 1.0. It is helpful here
		// 	see Unity.Random to find more useful randomness features of Unity
		
		return new Ping(jamPacket);
	}
}

*/
