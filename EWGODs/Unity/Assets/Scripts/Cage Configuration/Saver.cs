using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.IO;

// a class handling autosaves 
public class Saver : MonoBehaviour
{
	// if true, will load prefabs from persistent files
	// set in inspector between program executions for debugging
	public bool LoadPrefabs = true;
	
	// where save data is stored
	static SaveState State = new SaveState();
	
	static float AutosaveTimer;			// timer for autosaves
	const float AutosaveInterval = -1f;	// the time between autosaves (-1 for no autosaves)
	
    // Start is called before the first frame update
    void Start()
    {
		// start the timer
        AutosaveTimer = AutosaveInterval;
		
		// if a save file exists, load it into the save state
		if (LoadPrefabs && File.Exists(Path.Combine(Application.persistentDataPath, "data.json")))
		{
			// get the data from the file
			string jsonPath = Path.Combine(Application.persistentDataPath, "data.json");
			string jsonData = File.ReadAllText(jsonPath);
			
			// load the data into the save state
			State = JsonConvert.DeserializeObject<SaveState>(jsonData);
		}
    }
	
	// every frame, check for autosave
	void Update()
	{
		// advance the autosave timer
		AutosaveTimer -= Time.deltaTime;
		
		// autosave if timer runs out
		if (AutosaveTimer <= 0f && AutosaveInterval != -1f)
		{
			// save data and reset autosave timer
			Save();
			AutosaveTimer = AutosaveInterval;
		}
	}
	
	// force the game to save data immediately
	public static void ForceSave()
	{
		// save data
		Save();
	}
	
	
	// save project data
	static void Save()
	{
		// convert the save state to a JSON string
		string jsonData = JsonConvert.SerializeObject(State);
		// print the string for testing
		File.WriteAllText(Path.Combine(Application.persistentDataPath, "data.json"), jsonData);
	}

	// return a reference to the save state for reading and modification
    public static SaveState GetState()
	{
		return State;
	}
}
