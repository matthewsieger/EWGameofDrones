using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// stores a ping input from the Arduino
public class Ping
{
	public uint type { get; private set; }			// marks the sensor type the ping is meant for
    public uint id { get; private set; }			// marks the sensor id for the sensor the ping came from
	public List<string> data { get; private set; }	// stores additional data from the ping
	
	public Ping(string input)
	{
		//Debug.Log(input);
		
		// split up the packet by commas
		string[] dataPoints = input.Split(',');
		
		// packets must contain a type and id at minimum
		Debug.AssertFormat(dataPoints.Length >= 2, "Packet too small!", this);
		
		// get the type and id from the first two data entries
		type = Convert.ToUInt16(dataPoints[0]);
		id = Convert.ToUInt16(dataPoints[1]);
		
		// store the rest of the packet
		data = new List<string>();	// create list
		
		// add each additional data point to be stored
		for (int i = 2; i < dataPoints.Length; i++)
		{
			data.Add(dataPoints[i]);
		}
	}
}
