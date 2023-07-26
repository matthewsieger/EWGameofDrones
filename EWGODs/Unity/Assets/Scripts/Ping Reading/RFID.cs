using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// code for RFID sensors
public class RFID : Sensor
{
	// default constructor
	public RFID()
	{
		// RFID sensors are type 5 (the first byte of LIDAR packets over the XBee network is 5)
		Type = 5;
	}
	
	// calculate the position of a sensor ping
    protected override Vector3 CalculatePing(Ping ping)
	{
		// calculate the position of the ping
		
		Vector3 coord = new Vector3();	// the coordinates (xy) of the ping
		
		// RFID sensors don't have exact location data, so will simply display a ping on the sensor to
		// 	show that it has bee triggered
		coord = AddSensorPosition(coord);
		
		// return the ping coordinates
		return coord;
	}
	
	// generate a random fake ping for jamming
	protected override Ping CalculateJamPing()
	{
		// start with an empty string
		string pingPacket = "";
		
		// RFID packets are in the form type,id
		pingPacket += Type.ToString();
		pingPacket += "," + SensorData.id.ToString();
		
		// create the randomized fake ping
		Ping jamPing = new Ping(pingPacket);
		
		// return the fake ping
		return jamPing;
	}
}
