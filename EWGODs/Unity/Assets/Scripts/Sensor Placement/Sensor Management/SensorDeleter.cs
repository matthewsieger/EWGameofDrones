using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// delete a sensor when the delete button is clicked
public class SensorDeleter : MonoBehaviour
{
    // set in inspector
	public SensorManager Manager;			// handles sensors in play
	public SensorConfigReader Reader;		// reads user input
	public SensorDropdown SensorSelection;	// the dropdown selecting different sensors
	
    // Start is called before the first frame update
    void Start()
    {
		// get the button on the same GameObject as this script and set the
		// OnDeleteButtonClicked() method to execute when the button is clicked
        Button deleteButton = gameObject.GetComponent<Button>() as Button;
		deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

	// runs when the delete button is clicked
    void OnDeleteButtonClicked()
	{
		// find the id to delete from the dropdown
		uint targetID = SensorSelection.GetID();
		
		// delete the sensor matching the id
		Manager.DeleteSensor(targetID);
		
		// remove the corresponding entry from the dropdown
		SensorSelection.DeleteID(targetID);
	}
}
