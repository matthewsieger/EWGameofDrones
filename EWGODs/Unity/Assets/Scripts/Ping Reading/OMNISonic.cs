using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OMNISonic : Sensor
{
	public OMNISonic()
	{
		Type = 4;
	}
	
	protected override Vector3 CalculatePing(Ping ping)
	{
		// get the data from the ping
		// LIDAR packets only have the distance as the data
		Debug.Assert(ping.data.Count == 3, "Invalid packet size: " + ping.data.Count.ToString(), this);
		int distance = Convert.ToInt16(ping.data[0]);
		int hRotation = Convert.ToInt16(ping.data[1]);
		int vRotation = Convert.ToInt16(ping.data[2]);
		
		Vector3 coord = new Vector3();
		
		// rotate coordinate by horizontal rotation (degrees) sensor is positioned at
		// formula:
		//	x' = x*Cos(theta) - y*Sin(phi)
		//	y' = y*Cos(theta) + x*Sin(phi)
		coord.x = Cage.FeetToPixels(distance / 12f) * 
			Mathf.Cos(Mathf.Deg2Rad * (SensorData.hRotation + hRotation)) * 
			Mathf.Sin(Mathf.Deg2Rad * ((SensorData.vRotation) + vRotation));
		coord.y = Cage.FeetToPixels(distance / 12f) * 
			Mathf.Sin(Mathf.Deg2Rad * (SensorData.hRotation + hRotation)) * 
			Mathf.Sin(Mathf.Deg2Rad * ((SensorData.vRotation) + vRotation));
		coord.z = (distance / 12f) * 
			Mathf.Cos(Mathf.Deg2Rad * ((SensorData.vRotation) + vRotation));
		
		coord = AddSensorPosition(coord);
		
		return coord;
	}
	
	int jamCount = 0;
	
	// calculate a random fake ping for this sensor type
	// the omnisonic sensor has the capability to detect all around in (in theory) so we must
	// display more fake pings to mask the real ones more effectively
	protected override Ping CalculateJamPing()
	{
		// we are recursively running this function until the 11th time
		if (jamCount > 10)
		{
			// reset the recursive counter
			jamCount = 0;
			
			// calculate the final randomized fake ping and return it
			return RandomizeJamPing();
		}
		
		// display a random fake ping
		PlotPing(RandomizeJamPing());
		
		// increase the count of how many times in this recursive sequence this method
		// has been called
		jamCount++;
		
		// recursively call this method
		return CalculateJamPing();
	}
	
	// randomly generate a ping
	Ping RandomizeJamPing()
	{
		// start with an empty string
		string jamPacket = "";
		
		// OMNISonic pings are in the form type,id,distance,hrotation,vrotation
		// generate distance between 0 and 180 inches
		// generate hrotation between -180 and 180
		// generate vrotation between -90 and 90
		jamPacket += Type.ToString() + ",";
		jamPacket += SensorData.id.ToString() + ",";
		jamPacket += ((int)Mathf.Ceil(UnityEngine.Random.value * 180f)).ToString() + ",";
		jamPacket += ((int)Mathf.Ceil((UnityEngine.Random.value * 360f) - 180f)).ToString() + ",";
		jamPacket += ((int)Mathf.Ceil((UnityEngine.Random.value * 180) - 90f)).ToString();
		
		// create and return the randomized ping
		return new Ping(jamPacket);
	}
}
