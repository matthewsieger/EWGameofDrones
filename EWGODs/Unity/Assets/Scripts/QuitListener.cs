using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitListener : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		// listen to quit on all scenes
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
		// quit if ESC pressed
        if (Input.GetKeyUp(KeyCode.Escape))
		{
			// quit
			Application.Quit();
		}
    }
}
