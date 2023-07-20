using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// blueprint for sensor classes
public abstract class Sensor : MonoBehaviour
{
	public CageRenderer Cage { protected get; set; }		// for getting cage dimensions. Expected to be set upon creation
	public GameObject PingTemplate { private get; set; }	// for creating pings. Expected to be set upon creation
	public SensorManager Manager { private get; set; }		// for getting sensor data. Expected to be set upon creation
	
	public uint Type { get; protected set; }	// the type associated with a sensor. Should be set in subclass constructor
	
	protected SensorConfiguration SensorData { get; private set; }	// stores data relating to sensors. Set by LoadSensorData()
	
    protected abstract Vector2 CalculatePing(Ping ping);	// subclasses must define this to calculate the coordinates of the ping
	
	protected abstract Ping CalculateJamPing();
	
	// create fake packets for all sensors matching the type of this instance
	public void Jam()
	{
		// find all sensors
		List<SensorRenderer> renderers = Manager.GetSensors();
		
		// check each sensor
		foreach (SensorRenderer renderer in renderers)
		{
			// compare the sensor's type with the type of this Sensor instance
			if (renderer.Config.type == this.GetType().Name)
			{
				// if they match, generate fake pings.
				// start by loading in the data for the current sensor
				LoadSensorData(renderer.Config.id);
				
				// skip if failed to load sensor data
				if (SensorData == null)
				{
					continue;
				}
				
				// calculate a random fake ping
				Ping jamPing = CalculateJamPing();
				
				// plot the fake ping
				PlotPing(jamPing);
			}
		}
	}
	
	// from a ping packet, plot the ping on the screen
	public void PlotPing(Ping ping)
	{
		// get the location and rotation of the sensor the ping came from
		LoadSensorData(ping.id);
		
		if (SensorData == null)
		{
			return;
		}
		
		// calculate the coordinates on the screen of the ping
		Vector2 coord = CalculatePing(ping);
		
		// display the ping on the screen
		PlotOnScreen(coord);
	}
	
	// find the information correlating with a specific sensor
	void LoadSensorData(uint id)
	{
		// get the sensor renderer associated with the sensor's id
		SensorRenderer renderer = Manager.FindSensor(id);
		
		// ensure that a sensor with the id exists
		if (renderer == null)
		{
			SensorData = null;
		}
		else
		{
			SensorData = renderer.Config;
		}
	}
	
	// display a ping at specified coordinates
	void PlotOnScreen(Vector2 coord2D)
	{
		// we will need to convert the given 2D coordinate into a 3D coordinate for Unity
		Vector3 coord3D = new Vector3();
		
		// transfer the data from the 2D coordinate to the 3D coordinate
		coord3D.x = coord2D.x;
		coord3D.y = coord2D.y;
		coord3D.z = 0f;	// use z = 0
		
		// create and setup the ping object
		GameObject pingObject = Instantiate(PingTemplate, transform);	// create ping
		pingObject.transform.position = coord3D;						// set ping's position
		pingObject.SetActive(true);										// activate ping
	}
	
	// convert a given value in centimeters to Unity units (screen units)
	protected float CmToScreen(float cm)
	{
		float feet = cm * 0.0328084f;	// convert from centimeters to feet
		return Cage.FeetToPixels(feet);	// convert from feet to Unity units (screen units)
	}
	
	// calculates the x position of the sensor in Unity units (screen units)
	protected float GetSensorX(SensorConfiguration sensor)
	{
		// calculate the position of the left of the red team area, which is x=0 for sensors
		float redLeft = Cage.right - 
			(Cage.FeetToPixels(ConfigStorage.GetConfig().sectorLength) * ConfigStorage.GetConfig().sectorCountX);
		
		// add the sensor's x to the left of the red team area to get the x coordinate of the sensor
		return redLeft + Cage.FeetToPixels(sensor.x);
	}
	
	// calculates the y position of the sensor in Unity units (screen units)
	protected float GetSensorY(SensorConfiguration sensor)
	{
		// add the bottom of the cage (y=0) to the y of the sensor
		return Cage.down + Cage.FeetToPixels(sensor.y);
	}
	
	// centers the given coordinate around the sensor instead of (0, 0)
	protected Vector2 AddSensorPosition(Vector2 coord)
	{
		// add the coordinates of the sensor to the given coordinate
		coord.x += GetSensorX(SensorData);
		coord.y += GetSensorY(SensorData);
		
		return coord;
	}
	
	// rotate the given coordinate to match the rotation given to the sensor in setup.
	// WARNING: this function assumes the sensor's location is at (0, 0), so this function
	// 	should be called before translating the coordinate to the sensor's position (i.e. AddSensorPosition())
	// 	or ApplySensorAngle() should be called with 2 arguments
	// WARNING: This function assumes that the given coordinate is level with the sensor (on the xy plane)
	protected Vector2 ApplySensorAngle(Vector2 inCoord)
	{
		// assume sensor is at (0, 0) for calculations
		return ApplySensorAngle(inCoord, new Vector2(0f, 0f));
	}
	
	// rotate the given coordinate to match the rotation given to the sensor in setup.
	// in this overload, the coordinate of the sensor is given as a parameter
	// WARNING: This function assumes that the given coordinate is level with the sensor (on the xy plane)
	protected Vector2 ApplySensorAngle(Vector2 inCoord, Vector2 sensorCoord)
	{
		// translate the coordinate to (0, 0) for the rotation
		Vector2 translatedCoord = inCoord - sensorCoord;
		
		// stores calculated coordinates
		Vector2 outCoord = new Vector2();
		
		// rotate coordinate by horizontal rotation (degrees) sensor is positioned at
		// formula:
		//	x' = x*Cos(theta) - y*Sin(theta)
		//	y' = y*Cos(theta) + x*Sin(theta)
		outCoord.x = translatedCoord.x * Mathf.Cos(Mathf.Deg2Rad * SensorData.hRotation) - 
			translatedCoord.y * Mathf.Sin(Mathf.Deg2Rad * SensorData.hRotation);
		outCoord.y = translatedCoord.y * Mathf.Cos(Mathf.Deg2Rad * SensorData.hRotation) +
			translatedCoord.x * Mathf.Sin(Mathf.Deg2Rad * SensorData.hRotation);

		// rotate the coordinate about the sensor's vertical rotation.
		// the rotation is 0 when the sensor faces straight up and goes to 90 degrees when sensor
		// is aligned with the xy plane. To calculate the projection of the ping position on the xy
		// plane we can multiply the coordinate by the sin of the vertical angle
		outCoord *= Mathf.Sin(Mathf.Deg2Rad * SensorData.vRotation);
		
		// translate the coordinate back to the sensor's position
		outCoord += sensorCoord;
		
		// return calculated coordinate
		return outCoord;
	}
}
