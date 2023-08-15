using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// save sensor data when save button is clicked
public class SensorSaver : MonoBehaviour
{
	// set in inspector
	public SensorManager Manager;			// handles sensors in play
	public SensorConfigReader Reader;		// reads user input
	public SensorDropdown SensorSelection;	// the dropdown selecting different sensors
	public SensorPreview Preview;			// handles the sensor placement preview
	
    // Start is called before the first frame update
    void Start()
    {
		// get the button on the same GameObject as this script and set the
		// OnSaveButtonClicked() method to execute when the button is clicked
        Button saveButton = gameObject.GetComponent<Button>() as Button;
		saveButton.onClick.AddListener(OnSaveButtonClicked);
    }

	// runs when the save button is clicked
    void OnSaveButtonClicked()
	{
		// read the sensor configuration from user input
		SensorConfiguration currentConfig = Reader.ReadSensor();
		
		uint id = SensorSelection.GetID();
		
		
		
		// delete the sensor visually
		if (id != currentConfig.id && id != 0)
		{
			// if the id of the currently edited sensor is different than the id from the user
			// input, then delete the sensor visual as we will be creating a new one
			SensorSelection.DeleteID(id);
		}
		
		// save the sensor configuration and display it
		Manager.SaveSensor(id, currentConfig);
		SensorSelection.AddNew(currentConfig.id);	// add option to dropdown
		
		// hide the new sensor (not the preview)
		Manager.ShowAllSensors();
		Manager.HideSensor(currentConfig.id);
	}
}
