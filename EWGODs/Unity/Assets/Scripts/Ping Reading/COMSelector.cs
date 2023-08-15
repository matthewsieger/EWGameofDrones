using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles user selection of COM port of Coordinator Arduino
public class COMSelector : MonoBehaviour
{
	// the connection between the GUI and the Coordinator
	ArduinoListener Connection;
	
	// the default COM port of the coordinator
	public int DefaultCom;
	
	void Start()
	{
		// get a reference to the connection
		Connection = gameObject.GetComponent<ArduinoListener>() as ArduinoListener;
		Connection.Connect(DefaultCom);	// attempt to connect on the default com port
	}

    // Update is called once per frame
    void Update()
    {
		// check if any number key (0-9) was pressed (including numpad)
        for (int i = 0; i <= 9; i++)
        {
			// check if the key was pressed
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
				// if a number was pressed then try and connect to the corresponding com port
                Connection.Connect(i);
            }
        }
    }
}
