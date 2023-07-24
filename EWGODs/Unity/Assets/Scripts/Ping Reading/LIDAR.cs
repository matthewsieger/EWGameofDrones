using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// code for LIDAR sensors
public class LIDAR : Sensor
{
	// default constructor
	public LIDAR()
	{
		// LIDAR sensors are type 1 (the first byte of LIDAR packets over the XBee network is 1)
		Type = 1;
	}
	
	// calculate the position of a sensor ping
    protected override Vector3 CalculatePing(Ping ping)
	{
		// get the data from the ping
		// LIDAR packets only have the distance as the data
		Debug.Assert(ping.data.Count == 1, "Invalid packet size: " + ping.data.Count.ToString(), this);
		int distance = Convert.ToInt16(ping.data[0]);
	
		// calculate the position of the ping
		
		Vector3 coord = new Vector3();	// the coordinates (xy) of the ping
		
		// the ping should be <distance> away from the sensor
		coord.x = distance;
		coord.y = 0f;
		coord.z = 0f;
		
		// apply the sensor's angle and position to the coordinates
		coord = ApplySensorAngle(coord);
		
		coord.x = CmToScreen(coord.x);
		coord.y = CmToScreen(coord.y);
		coord.z *= 0.0328084f;
		
		coord = AddSensorPosition(coord);
		
		// return the ping coordinates
		return coord;
	}
	
	// generate a random fake ping for jamming
	protected override Ping CalculateJamPing()
	{
		// start with an empty string
		string pingPacket = "";
		
		// LIDAR packets are in the form type,id,distance
		// randomize distance to be between 0 and 1000
		pingPacket += Type.ToString();
		pingPacket += "," + SensorData.id.ToString();
		pingPacket += "," + ((int)Mathf.Ceil(UnityEngine.Random.value * 1000f)).ToString();
		
		// create the randomized fake ping
		Ping jamPing = new Ping(pingPacket);
		
		// return the fake ping
		return jamPing;
	}
}
