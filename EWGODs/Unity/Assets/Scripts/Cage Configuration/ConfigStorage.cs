using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// store a cage configuration to allow
// it to be used between scenes
public static class ConfigStorage
{
	// the selected cage configuration
	static CageConfiguration SelectedConfig = new CageConfiguration();
	
	// set the current cage configuration
	static public void SetConfig(CageConfiguration config)
	{
		SelectedConfig = config;
	}
	
	// get the current configuration
	static public CageConfiguration GetConfig()
	{
		return SelectedConfig;
	}
}
