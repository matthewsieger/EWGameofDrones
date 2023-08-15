using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// listen for ESCAPE key, then return to SensorPlacement scene
public class BackToCageButton : MonoBehaviour
{
	// keep track of persistant objects that must be destroyed manually when changing scene
	List<GameObject> ObjectsToDestroy = new List<GameObject>();
	
	// prevent accidental activation for 1 second after scene starts
	float EscapeCooldown = 1f;
	
	void Start()
	{
		// get references to objects that must be destroyed manually
		ObjectsToDestroy.Add(GameObject.FindWithTag("Sensor Manager"));
		ObjectsToDestroy.Add(GameObject.FindWithTag("Ruler"));
	}
	
    // Update is called once per frame
    void Update()
    {
		// track time since scene start
		EscapeCooldown -= 0.5f * Time.deltaTime;
		
		// check if escape is pressed
		if (Input.GetKeyUp(KeyCode.Escape) && EscapeCooldown <= 0f)
		{
			// when the ESCAPE key is pressed, swap scenes
			
			foreach(GameObject obj in ObjectsToDestroy)
			{
				Destroy(obj);
			}
			
			// change scenes
			SceneManager.LoadScene("CageConfig");
		}
		
		// track time since scene start
		EscapeCooldown -= 0.5f * Time.deltaTime;
		
		// keep cooldown to a reasonable number
		if (EscapeCooldown < 0f)
		{
			EscapeCooldown = 0f;
		}
    }
}
