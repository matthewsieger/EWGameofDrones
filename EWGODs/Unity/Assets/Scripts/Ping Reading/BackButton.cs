using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// listen for ESCAPE key, then return to SensorPlacement scene
public class BackButton : MonoBehaviour
{

	bool SwappedScenes = false;	// allows waiting for 1 full frame to allow
								// objects to initialize after changing scene

	// stores sensor configurations to transfer them between scenes
	List<SensorConfiguration> ExistingSensors = new List<SensorConfiguration>();

    // Update is called once per frame
    void Update()
    {
	
        if (SwappedScenes)
		{
			// if the scene has been swapped (a frame has passed since then)
			// 	then add the sensor data to the manager and dropdown
			
			// get references to the sensor manager and the sensor dropdown
			SensorManager manager = GameObject.FindWithTag("Sensor Manager").GetComponent<SensorManager>() as SensorManager;
			SensorDropdown dropdown = GameObject.FindWithTag("Sensor Dropdown").GetComponent<SensorDropdown>() as SensorDropdown;
			
			// add each sensor
			foreach(SensorConfiguration config in ExistingSensors)
			{
				// add the sensor's data to the SensorManager
				manager.SaveSensor(config.id, config);
				
				// add the sensor as an option to the dropdown
				dropdown.AddNew(config.id);
				
				dropdown.UpdateInputs();
			}
			
			// destroy this object to prevent it from activating again
			Destroy(gameObject);
		}
		else if (Input.GetKeyUp(KeyCode.Escape))
		{
			// when the ESCAPE key is pressed, save the sensor data and
			//	swap scenes
			
			// prevent this object from being destroyed when the scene changes
			DontDestroyOnLoad(gameObject);
			
			// get a reference to the sensor manager
			GameObject manager = GameObject.FindWithTag("Sensor Manager");
			
			// save the configuration data for each existing sensor
			foreach(SensorRenderer renderer in manager.GetComponent<SensorManager>().GetSensors())
			{
				// save the configuration data for the sensor
				ExistingSensors.Add(renderer.Config);
			}
			
			// destroy the existing sensor manager to prevent duplicate sensor managers
			// when the scene changes
			Destroy(GameObject.FindWithTag("Sensor Manager"));
			
			// change scenes
			SceneManager.LoadScene("SensorPlacement");
			
			// set flag so sensor data is transferred on next frame
			//	giving components time to initialize
			SwappedScenes = true;
		}
    }
}
