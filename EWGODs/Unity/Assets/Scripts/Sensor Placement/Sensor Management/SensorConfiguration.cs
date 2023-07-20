using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores information necessary to construct a sensor
public class SensorConfiguration
{
    public string type;	// the type of sensor used
	public uint id;			// the sensor's Zigbee ID
	public float x;			// the sensor's position in feet from the left of the red team zone
	public float y;			// the sensor's position in feet from the bottom of the cage
	public float hRotation;	// the sensor's rotation in degrees horizontally
	public float vRotation;	// the sensor's rotation in degrees vertically
}
