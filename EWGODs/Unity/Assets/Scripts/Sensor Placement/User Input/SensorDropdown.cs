using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

// manages the sensor selection dropdown menu
public class SensorDropdown : MonoBehaviour
{
	// set in inspector
	public Button DeleteButton;				// the delete button
	public TypeParser TypeDropdown;			// the sensor type dropdown
	public TMP_InputField IDField;			// the id text field
	public TMP_InputField XField;			// the x coordinate text field
	public TMP_InputField YField;			// the y coordinate text field
	public TMP_InputField HRotationField;	// the horizontal rotation text field
	public TMP_InputField VRotationField;	// the vertical rotation text field
	public SensorManager Manager;			// manages sensors
	
	TMP_Dropdown Dropdown;	// the dropdown for selecting sensors
	
    // Start is called before the first frame update
    void Start()
    {
		// get the dropdown component and listen to changes to its selection
		Dropdown = gameObject.GetComponent<TMP_Dropdown>() as TMP_Dropdown;
		Dropdown.onValueChanged.AddListener(OnIndexChange);
    }
	
	// return the id of the currently selected sensor
	public uint GetID()
	{
		// get the id of the sensor at the currently selected position
		return GetID(Dropdown.value);
	}
	
	// return the id of the specified option
	uint GetID(int index)
	{
		// get the id
		if (index == Dropdown.options.Count - 1)
		{
			// if the index is on the last option ("New Sensor"), then its id is 0,
			// which is unused
			return 0;
		}
		else
		{
			// if the index is not on the last option, its text is in the form "Sensor X" where X is an
			// arbitrary substring that can be converted into the id
			
			// get the index of the first character of the id (1 past the space)
			int idIndex = Dropdown.options[index].text.IndexOf(' ') + 1;
			
			// get the substring with the id starting from the index found
			string idString = Dropdown.options[index].text.Substring(idIndex);
			
			// convert it from a string into a 16 bit unsigned integer
			return Convert.ToUInt16(idString);
		}
	}
	
	// add a new sensor entry to the dropdown
	public void AddNew(uint id)
	{
		// try to find an existing dropdown option matching the id
		int optionIndex = FindOption(id);
		
		if (optionIndex == -1)
		{
			// if no matching existing entry was found, create a new one
			TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();	// create object
			option.text = "Sensor " + id.ToString();						// form text
			Dropdown.options.Insert(Dropdown.options.Count - 1, option);	// insert option just before "New Sensor"
			Dropdown.value = Dropdown.options.Count - 2;					// set dropdown to be at new entry
		}
		else
		{
			// if a matching entry was found, swap the dropdown to be at it
			Dropdown.value = optionIndex;
		}
		
		// refresh the visual component of the dropdown menu
		Dropdown.RefreshShownValue();
		
		// fill in the user inputs with data
		UpdateInputs();
	}
	
	// delete the entry matching the given id if it exists
	public void DeleteID(uint id)
	{
		// try to find an existing dropdown option matching the id
		int optionIndex = FindOption(id);
		
		if (optionIndex != Dropdown.options.Count - 1 && optionIndex != -1)
		{
			// if an entry was found and it isn't the "New Sensor" entry, delete it
			Dropdown.options.Remove(Dropdown.options[optionIndex]);	// remove it from the dropdown list
			Dropdown.value = Dropdown.options.Count - 1;			// move the dropdown to be at the "New Sensor" option
			// call OnIndexChange (it normally is already called when Dropdown.value changes, but is is possible that
			// Dropdown.value remained the same after the previous operation and it is more important to make sure this
			// method is called
			OnIndexChange(Dropdown.value);						
			Dropdown.RefreshShownValue();							// refresh the visual component of the dropdown
		}
	}
	
	// update the inputs to match the option selected
	// usually this is done automatically
	public void UpdateInputs()
	{
		// update the inputs
		OnIndexChange(Dropdown.value);
	}
	
	// runs when the dropdown changes
	// (more specifically, when Dropdown.value changes. It is possible
	// for the dropdown to change without Dropdown.value changes i.e. when
	// an earlier option in the dropdown list is deleted)
	void OnIndexChange(int index)
	{
		// set the user inputs to have the correct values in them
		uint targetID = GetID(index);	// get the id of the current selection
		if (targetID == 0)
		{
			// if on the "New Sensor" option, fill the user inputs with default values
			TypeDropdown.SetValue(0);
			IDField.text = "";
			XField.text = "";
			YField.text = "";
			HRotationField.text = "";
			VRotationField.text = "";
			
			// deactivate the delete button as "New Sensor" cannot be deleted
			DeleteButton.interactable = false;
			
			// all existing sensors should be visible
			Manager.ShowAllSensors();
		}
		else
		{
			// fill the user inputs with values loaded from the corresponding sensor configuration
			SensorRenderer renderer = Manager.FindSensor(targetID);	// get the renderer corresponding to the found sensor id
			Debug.Assert(renderer != null, "Dropdown unable to load sensor data!", this);	// this should never trigger in theory
			SensorConfiguration config = renderer.Config;	// get the configuration
			
			// populate the inputs with values
			TypeDropdown.SetValue(config.type);
			IDField.text = config.id.ToString();
			XField.text = config.x.ToString();
			YField.text = config.y.ToString();
			HRotationField.text = config.hRotation.ToString();
			VRotationField.text = config.vRotation.ToString();
			
			// enable the delete button
			DeleteButton.interactable = true;
			
			// show all sensors except the saved version of the sensor being edited
			Manager.ShowAllSensors();
			Manager.HideSensor(targetID);
		}
	}
	
	// find the index of an option in the dropdown menu with the specified id
	// returns -1 if not found
	int FindOption(uint id)
	{
		// find the index
		for (int i = 0; i < Dropdown.options.Count; i++)
		{
			// check the index against the given id
			if (GetID(i) == id)
			{
				// if the index and id match, return the index
				return i;
			}
		}
		
		// if an index wasn't found, return -1
		return -1;
	}
}
