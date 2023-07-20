using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

// convert user input from a dropdown menu into a sensor type for further processing
public class TypeParser : MonoBehaviour
{
	TMP_Dropdown Dropdown;	// the dropdown menu to read input from
	
	// called before first frame update
	void Start()
	{
		// retrieve the dropdown menu component
		Dropdown = gameObject.GetComponent<TMP_Dropdown>() as TMP_Dropdown;
		
		// get the names of every sensor type
		List<string> types = new List<string>();
		
		// find each sensor type by searching for subclasses of "Sensor"
		System.Type[] sensorTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Sensor)))
            .ToArray();

		// save the name of each sensor type found
        foreach (System.Type type in sensorTypes)
        {
            // save type name
            types.Add(type.Name);
        }
		
		// set dropdown values to be the sensor type names
		Dropdown.ClearOptions();
		Dropdown.AddOptions(types);
	}
	
	// get the text in the dropdown menu
	public string ReadDropdown()
	{	
		// return the text of the currently selected option
		return Dropdown.options[Dropdown.value].text;
	}
	
	// set the value of the dropdown to match the given string (if option exists)
	public void SetValue(string name)
	{
		// search through the dropdown menu options
		for (int i = 0; i < Dropdown.options.Count; i++)
		{
			// check if the option matches the given option name
			if (Dropdown.options[i].text == name)
			{
				// if the option matching the given string is found, set the dropdown menu to be at
				// that option
				Dropdown.value = i;
				return;
			}
		}
	}
	
	// set the value of the dropdown to match the given index
	public void SetValue(int i)
	{
		// set to match the index
		Dropdown.value = i;
	}
}
