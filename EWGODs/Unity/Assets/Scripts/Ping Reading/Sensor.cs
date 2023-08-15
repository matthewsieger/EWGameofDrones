using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// blueprint for sensor classes
public abstract class Sensor : MonoBehaviour
{
	public CageRenderer Cage { protected get; set; }		// for getting cage dimensions. Expected to be set upon creation
	public GameObject PingTemplate { private get; set; }	// for creating pings. Expected to be set upon creation
	public TMP_Text PingTextTemplate {private get; set; }	// for showing pings' altitude. Expected to be set upon creation
	public SensorManager Manager { private get; set; }		// for getting sensor data. Expected to be set upon creation
	private Camera MainCamera;
	
	public uint Type { get; protected set; }	// the type associated with a sensor. Should be set in subclass constructor
	
	protected SensorConfiguration SensorData { get; private set; }	// stores data relating to sensors. Set by LoadSensorData()
	
    protected abstract Vector3 CalculatePing(Ping ping);	// subclasses must define this to calculate the coordinates of the ping
	
	protected abstract Ping CalculateJamPing();
	
	void Start()
	{
		MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>() as Camera;
	}
	
	// create fake packets for all sensors matching the type of this instance
	public void Jam(uint id)
	{
		// find all sensors
		List<SensorRenderer> renderers = Manager.GetSensors();
		
		// check each sensor
		foreach (SensorRenderer renderer in renderers)
		{
			// compare the sensor's type and id with the type of this Sensor instance
			if (renderer.Config.type == this.GetType().Name && (renderer.Config.id == id || id == 0))
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
		Vector3 coord = CalculatePing(ping);
		
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
	void PlotOnScreen(Vector3 coord)
	{
		// we will need to convert the given 2D coordinate into a 3D coordinate for Unity
		Vector3 coordDisplayed = new Vector3();
		
		// transfer the data from the 2D coordinate to the 3D coordinate
		coordDisplayed.x = coord.x;
		coordDisplayed.y = coord.y;
		coordDisplayed.z = 0;	// use z = 0. the z component is used for the number display
		
		// create and setup the ping object
		GameObject pingObject = Instantiate(PingTemplate, transform);	// create ping
		pingObject.transform.position = coordDisplayed;						// set ping's position
		pingObject.SetActive(true);										// activate ping
		
		// prepare the altitude text
		if (coord.z.ToString("n2") == "0.00")
		{
			// display nothing if no significant verticality
			PingTextTemplate.text = "";
		}
		else if (coord.z > 0f)
		{
			// set the text in the format: "+x.xx"
			PingTextTemplate.text = "+" + (coord.z).ToString("n2");
		}
		else
		{
			// set the text in the format: "-x.xx"
			PingTextTemplate.text = (coord.z).ToString("n2");
		}
		
		// create the text object
		GameObject pingText = Instantiate(PingTextTemplate.gameObject, PingTextTemplate.gameObject.transform.parent);
		
		// set the text's position
		Vector3 textPos = pingObject.transform.position;	// get position from ping
		textPos.z = 0f;	// keep in front of other elements
		pingText.transform.position = textPos;	// set position of text
		
		// make the text visible
		pingText.SetActive(true);
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
	protected Vector3 AddSensorPosition(Vector3 coord)
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
	protected Vector3 ApplySensorAngle(Vector3 inCoord)
	{
		// assume sensor is at (0, 0) for calculations
		return ApplySensorAngle(inCoord, new Vector2(0f, 0f));
	}
	
	// rotate the given coordinate to match the rotation given to the sensor in setup.
	// in this overload, the coordinate of the sensor is given as a parameter
	// WARNING: This function assumes that the given coordinate is level with the sensor (on the xy plane)
	protected Vector3 ApplySensorAngle(Vector3 inCoord, Vector2 sensorCoord)
	{
		return ApplySensorAngle(inCoord, new Vector3(sensorCoord.x, sensorCoord.y, 0f));
	}
	
	// rotate the given coordinate to match the rotation given to the sensor in setup.
	// in this overload, the coordinate of the sensor is given as a parameter
	// WARNING: This funciton ignores the z property of the sensor coordinate
	// WARNING: This function assumes that the given coordinate is level with the sensor (on the xy plane)
	protected Vector3 ApplySensorAngle(Vector3 inCoord, Vector3 sensorCoord)
	{
		// translate the coordinate to (0, 0, 0) for the rotation
		Vector3 translatedCoord = inCoord - sensorCoord;
		
		// calculate the spherical coordinates of the ping
		// (distance, horizontal rotation, vertical rotation)
		// formulas are:
		//	distance = sqrt(x^2 + y^2 + z^2)
		//	horizontal rotation = arctan(y/x)
		//	vertical rotation = arccos(z/distance)
		Vector3 sphericalCoord = new Vector3();
		sphericalCoord.x = translatedCoord.magnitude;
		sphericalCoord.y = Mathf.Atan2(translatedCoord.y, translatedCoord.x);
		if (sphericalCoord.x == 0f)
		{
			sphericalCoord.z = 0f;	// if no distance from sensor, vertical angle doesn't matter
		}
		else
		{
			sphericalCoord.z = Mathf.Acos(translatedCoord.z / sphericalCoord.x);
		}
		
		// apply the rotation of the sensor to the coordinate
		sphericalCoord.y += Mathf.Deg2Rad * SensorData.hRotation;
		sphericalCoord.z += Mathf.Deg2Rad * -SensorData.vRotation;
		
		// convert back to Cartesian coordinates
		// formulas are:
		//	x = distance * sin(vertical rotation) * cos(horizontal rotation)
		//	y = distance * sin(vertical rotation) * sin(horizontal rotation)
		//	z = distance * cos(vertical rotation)
		Vector3 cartesianCoord = new Vector3();
		cartesianCoord.x = sphericalCoord.x * Mathf.Sin(sphericalCoord.z) * Mathf.Cos(sphericalCoord.y);
		cartesianCoord.y = sphericalCoord.x * Mathf.Sin(sphericalCoord.z) * Mathf.Sin(sphericalCoord.y);
		cartesianCoord.z = sphericalCoord.x * Mathf.Cos(sphericalCoord.z);
		
		//Debug.Log(SensorData.vRotation);
		//Debug.Log(sphericalCoord.x.ToString() + ", " + (Mathf.Rad2Deg * sphericalCoord.y).ToString() + " " + (Mathf.Rad2Deg * sphericalCoord.z).ToString());
		
		// reapply the sensor's position
		cartesianCoord += sensorCoord;
		
		// return the calculated coordinate
		return cartesianCoord;
	}
}
