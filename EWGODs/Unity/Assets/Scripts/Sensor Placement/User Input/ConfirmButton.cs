using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// go to the next scene when the confirm button is clicked
public class ConfirmButton : MonoBehaviour
{
	SensorManager Manager;
	
    // Start is called before the first frame update
    void Start()
    {
		// listen for clicks on the confirm button
        Button confirm = gameObject.GetComponent<Button>() as Button;
		confirm.onClick.AddListener(OnConfirmButtonClicked);
		
		Manager = GameObject.FindWithTag("Sensor Manager").GetComponent<SensorManager>() as SensorManager;
    }

	// runs when confirm button is clicked
    void OnConfirmButtonClicked()
	{
		// ensure all sensors are visible
		Manager.ShowAllSensors();
		
		// load the next scene
		SceneManager.LoadScene("SensorReadings");
	}
}
