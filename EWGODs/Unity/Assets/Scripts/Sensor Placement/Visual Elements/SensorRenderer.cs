using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// Convert a sensor configuration into a visual sensor icon on the screen
public class SensorRenderer : MonoBehaviour
{
	
	public SensorConfiguration Config { get; set; }	// the sensor configuration
	CageRenderer Cage;								// includes display information about the cage
	static Dictionary<string, Sprite> Sprites;		// the sprites for different sensor types
	SpriteRenderer SensorSprite;					// interface for visually changing the sensor sprite

	// runs when object is enabled
	void OnEnable()
	{
		// create a dictionary matching names of sensor types to corresponding sprites
		if (Sprites == null)
		{
			// create dictionary
			Sprites = new Dictionary<string, Sprite>();
			
			// get all subclasses of "Sensor"
			System.Type[] sensorTypes = System.AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.IsSubclassOf(typeof(Sensor)))
				.ToArray();

			// populate dictionary
			foreach (System.Type type in sensorTypes)
			{
				// add class type name matched with the sprite matching the name to the dictionary
				Sprites.Add(type.Name, Resources.Load<Sprite>("Images/" + type.Name));
			}
		}
		
		// get the components
		SensorSprite = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
		Cage = GameObject.Find("Drone Cage").GetComponent<CageRenderer>() as CageRenderer;
	}

    public void UpdateConfig(SensorConfiguration sensor)
    {
		// it is possible for the stored CageRenderer to become null as the scene changes (confirm button clicked)
		if (Cage == null)
		{
			// to fix the Cage attribute, find the object named "Drone Cage" and get its CageRenderer component
			Cage = GameObject.Find("Drone Cage").GetComponent<CageRenderer>() as CageRenderer;
		}
		
		// save the updated configuration settings
		Config = sensor;
		
		// calculate the sensor's position on the screen
        //	x will be the right edge of the cage - the length of all sectors combined to get the
		//		left edge of the red team area (x = 0). Then add the x of the sensor
		//	y will be the bottom of the cage (y = 0) + the y of the sensor
		Vector3 newPos = new Vector3();
		newPos.x = Cage.right - 
			Cage.FeetToPixels((ConfigStorage.GetConfig().sectorLength * ConfigStorage.GetConfig().sectorCountX)) + 
			Cage.FeetToPixels(Config.x);
		newPos.y = Cage.down + Cage.FeetToPixels(Config.y);
		newPos.z = 0;	// z doesn't really matter as long as it shows in front of other objects
		// update position
		transform.position = newPos;
		
		// calculate the sensor's horizontal rotation on the screen
		Vector3 newRot = new Vector3();
		newRot.x = 0f;
		newRot.y = 0f;
		// z is the only one that will affect the visual rotation
		// it should be in degrees the rotation from the configuration
		newRot.z = Config.hRotation;	
		// update the rotaiton
		transform.eulerAngles = newRot;
		
		// update the sensor image (sprite) with the sprite that matches the sensor's type
		SensorSprite.sprite = Sprites[Config.type];
    }
}
