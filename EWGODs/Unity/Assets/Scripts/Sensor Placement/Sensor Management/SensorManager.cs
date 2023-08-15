using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// deals with sensors placed by the user.
// allows them to be modified and saved between scenes
public class SensorManager : MonoBehaviour
{
	// set in inspector
	public GameObject SensorTemplate;
	
	List<SensorRenderer> Sensors = new List<SensorRenderer>();
	
	// Start is called before the first frame update
    void Start()
    {
		// prevent the SensorManager and its child objects
		// from being destroyed between the sensor placement
		// scene and the ping viewing scene
        DontDestroyOnLoad(gameObject);
    }
	
	// save a SensorConfiguration
	// this displays the sensor visually
	// id is the id of the sensor to change
	// newConfig is the configuration to update the sensor with
	public void SaveSensor(uint id, SensorConfiguration newConfig)
	{
		// attempt to get the sensor matching the new configuration'
		// id in case one already exists
		SensorRenderer renderer = FindSensor(id);
		
		// if the id given and the id in the configuration don't match,
		// some extra work must be done to make sure problems don't arrise
		if (id != newConfig.id)
		{
			// attempt to delete the sensor with the id we are swapping to
			// if it exists as there can only be one sensor for any given id
			DeleteSensor(newConfig.id);
		}
		
		// make sure that we have access to a renderer object to display
		if (renderer == null)
		{
			// if a sensor for the specified id doesn't already exist, create it
			GameObject newSensor = Instantiate(SensorTemplate, transform);			// create the visual sensor
			newSensor.SetActive(true);												// activate the visual
			renderer = newSensor.GetComponent<SensorRenderer>() as SensorRenderer;	// get the objects renderer component
			Sensors.Add(renderer);													// store a reference to the renderer
		}
		
		// update the visual sensor object with the new configuration
		renderer.UpdateConfig(newConfig);
	}
	
	// delete a sensor matching the given id if it exists
	public void DeleteSensor(uint id)
	{
		// find the matching sensor
		SensorRenderer renderer = FindSensor(id);
		
		// check if it exists
		if (renderer != null)
		{
			// if it exists, delete it
			Sensors.Remove(renderer);		// remove it from the local list
			Destroy(renderer.gameObject);	// destroy the Unity GameObject
		}
	}
	
	// return a SensorRenderer matching the id
	// from the sensors this class is managing
	public SensorRenderer FindSensor(uint id)
	{
		// find the sensor with the matching id
		foreach(SensorRenderer renderer in Sensors)
		{
			// check every SensorRenderer
			if (renderer.Config.id == id)
			{
				// if the id matches, return it
				return renderer;
			}
		}
		
		// if none found, return null
		return null;
	}
	
	// make all sensors do calculations again to refresh their position and status
	public void UpdateSensors()
	{
		// refresh each sensor
		foreach(SensorRenderer renderer in Sensors)
		{
			// refresh the sensor by updating it with its own configuration
			renderer.UpdateConfig(renderer.Config);
		}
	}
	
	// get the sensors
	public List<SensorRenderer> GetSensors()
	{
		return Sensors;
	}
	
	// hide a specific sensor from view
	public void HideSensor(uint id)
	{
		// find the sensor and deactivate it
		SensorRenderer renderer = FindSensor(id);
		
		if (renderer != null)
		{
			renderer.gameObject.SetActive(false);
		}
	}
	
	// ensure all sensors are visible
	public void ShowAllSensors()
	{
		// loop through each sensor
		foreach(SensorRenderer renderer in Sensors)
		{
			// activate the sensor's sprite
			renderer.gameObject.SetActive(true);
		}
	}
}
