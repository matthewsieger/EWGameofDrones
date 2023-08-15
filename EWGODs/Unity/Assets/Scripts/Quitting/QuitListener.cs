using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitListener : MonoBehaviour
{

	// set in inspector
	public GameObject ConfirmationBox;
	
    // Update is called once per frame
    void Update()
    {
		// quit if ESC pressed
        if (Input.GetKeyUp(KeyCode.Escape))
		{
			// open confirmation box
			ConfirmationBox.SetActive(true);
		}
    }
}
