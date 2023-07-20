using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

// handles the cage configuration menu
public class CageConfigDropdown : MonoBehaviour
{
	// maximum sector counts
	// each sector count results in the instantiation of a LineRenderer
	// it is important to not let these go too high as to not freeze
	// the program
	const uint MAX_SECTOR_COUNT_X = 100;
	const uint MAX_SECTOR_COUNT_Y = 100;
	
	// set in inspector
	public TMP_InputField NameField;			// input field containing the name of a configuration
	public TMP_InputField CageLengthField;		// input field containing the length of the cage
	public TMP_InputField CageWidthField;		// input field containing the width of the cage
	public TMP_InputField SectorLengthField;	// input field containing the length of a sector
	public TMP_InputField SectorCountXField;	// input field containing the number of sectors along the X-axis
	public TMP_InputField SectorCountYField;	// input field containing the number of sectors along the Y-axis
	public Button EditModeButton;	// button for toggling the editability of input fields
	public Button SaveButton;		// button for saving changes to a configuration
	public Button DeleteButton;		// button for deleting a configuration
	public Button SelectButton;		// button for selecting a configuration
	
	// event called as values are edited
	public static UnityEvent ValueChanged = new UnityEvent();
	
	TMP_Dropdown DropdownObject;		// the object containing the dropdown menu
	List<CageConfiguration> Configs;	// list of configuration settings available in the dropdown menu
	
    // Start is called before the first frame update
    void Start()
    {
		// get a reference to the dropdown menu
		DropdownObject = gameObject.GetComponent<TMP_Dropdown>() as TMP_Dropdown;
		
		// initialize the list of options for the dropdown menu from the list of configs saved
        Configs = new List<CageConfiguration>(Saver.GetState().ConfigPrefabs);
		
		// create a new config used for the user creating a custom configuration
		CageConfiguration customConfig = new CageConfiguration();	// initialize
		customConfig.configName = "Custom";	// set name
		customConfig.cageWidth = 10f;		// set width
		customConfig.cageLength = 10f;		// set length
		customConfig.sectorLength = 4f;	// set sector length
		customConfig.sectorCountX = 2;		// set sector count x
		customConfig.sectorCountY = 2;		// set sector count y
		Configs.Add(customConfig);			// add configuration to dropdown list
		
		// update the dropdown menu to show the options to the user
		UpdateDropdown();
    }

	// when changes are made to the Configs list, the user does not yet have access to them
	// the UpdateDropdown function updates the dropdown menu so that the user can access
	// new configurations (or not access deleted configurations)
	//
	// optionIndex is an optional argument for selecting which option in the dropdown list
	// should be selected when the update finishes
    void UpdateDropdown(int optionIndex = 0)
    {
		// clear the dropdown list so we can re-add everything
		DropdownObject.ClearOptions();
		
		// list of options in the form of strings (names of options)
		// use the names of the configs as the names of the options
		List<string> dropOptions = new List<string>();
		foreach (CageConfiguration config in Configs)
		{
			dropOptions.Add(config.configName);
		}
		
		// add options to the dropdown menu
		DropdownObject.AddOptions(dropOptions);
		
		// set current option as selected by the optionIndex parameter
		DropdownObject.value = optionIndex;
		
		// called because changing current option can result in new values visible
		OnDropdownValueChange();
    }
	
	// whenever the currently selected option changes, update the
	// input fields to match the new option
	public void OnDropdownValueChange()
	{
		// set each text box to match the corresponding option
		NameField.text = Configs[DropdownObject.value].configName;
		CageLengthField.text = Configs[DropdownObject.value].cageLength.ToString();
		CageWidthField.text = Configs[DropdownObject.value].cageWidth.ToString();
		SectorLengthField.text = Configs[DropdownObject.value].sectorLength.ToString();
		SectorCountXField.text = Configs[DropdownObject.value].sectorCountX.ToString();
		SectorCountYField.text = Configs[DropdownObject.value].sectorCountY.ToString();
		
		// enable and disable elements in the UI depending on
		// which option is selected
		if (DropdownObject.value != Configs.Count - 1)
		{
			// most options will be edit locked by default.
			// the edit toggle button and the select button are still available
			NameField.interactable = false;
			CageLengthField.interactable = false;
			CageWidthField.interactable = false;
			SectorLengthField.interactable = false;
			SectorCountXField.interactable = false;
			SectorCountYField.interactable = false;
			EditModeButton.interactable = true;
			SaveButton.interactable = false;
			DeleteButton.interactable = false;
			SelectButton.interactable = true;
		}
		else
		{
			// the last option is the custom config option
			// everything should be available by default (with some exceptions)
			// the edit toggle, delete button, and select button should never be available
			NameField.interactable = true;
			CageLengthField.interactable = true;
			CageWidthField.interactable = true;
			SectorLengthField.interactable = true;
			SectorCountXField.interactable = true;
			SectorCountYField.interactable = true;
			EditModeButton.interactable = false;
			SaveButton.interactable = true;
			DeleteButton.interactable = false;
			SelectButton.interactable = false;
		}
		
		ConfigStorage.SetConfig(GetCurrentConfig());
	}
	
	// when the edit toggle button is clicked, switch the interactability of
	// most elements to allow (or disallow) editing of the input fields, saving,
	// deleting, and selecting
	public void OnEditToggle()
	{
		// invert the interactability of all input fields, the save button, the delete button,
		// and the select button
		NameField.interactable = !NameField.interactable;
		CageLengthField.interactable = !CageLengthField.interactable;
		CageWidthField.interactable = !CageWidthField.interactable;
		SectorLengthField.interactable = !SectorLengthField.interactable;
		SectorCountXField.interactable = !SectorCountXField.interactable;
		SectorCountYField.interactable = !SectorCountYField.interactable;
		SaveButton.interactable = !SaveButton.interactable;
		DeleteButton.interactable = !DeleteButton.interactable;
		SelectButton.interactable = !SelectButton.interactable;
	}
	
	// when the save button is clicked, update related variables with changes made to
	// the input fields
	// if the current option is the custom config, create a new config with the values
	// from the input fields instead
	public void OnSave()
	{
		// get the data from the currently selected configuration
		CageConfiguration newConfig = GetCurrentConfig();
		
		// add new information to Configs list
		if (DropdownObject.value != Configs.Count - 1)
		{
			// if saving changes to a config, update the corresponding entry in the Configs list
			Configs[DropdownObject.value] = newConfig;
		}
		else
		{
			// if saving a new custom config, create a new config right before the custom option
			Configs.Insert(Configs.Count - 1, newConfig);
		}
		
		// update the dropdown for the user
		UpdateDropdown(DropdownObject.value);
		
		// save the configs (except the custom config option)
		Saver.GetState().ConfigPrefabs = Configs.GetRange(0, Configs.Count - 1);
		Saver.ForceSave();
	}
	
	// when the delete button is clicked, delete the currently selected config
	// the delete button should never be available for the custom config option
	public void OnDelete()
	{
		// remove the config from the Configs list
		Configs.Remove(Configs[DropdownObject.value]);
		
		// update the dropdown list for the user
		UpdateDropdown();
		
		// svae the configs (except the custom config option)
		Saver.GetState().ConfigPrefabs = Configs.GetRange(0, Configs.Count - 1);
		Saver.ForceSave();
	}
	
	// when the select button is clicked, continue to the next menu
	public void OnSelect()
	{
		// load the next menu (scene)
		SceneManager.LoadScene("SensorPlacement", LoadSceneMode.Single);
	}
	
	// return a copy of the currently selected configuration
	public CageConfiguration GetCurrentConfig()
	{
		// initialize
		CageConfiguration newConfig = new CageConfiguration();
		
		// retrieve data from input fields
		
		// get the name
		newConfig.configName = NameField.text;
		
		// get the cage length
		if (CageLengthField.text != "" && CageLengthField.text[0] != '-' && !(CageLengthField.text[0] == '.' && CageLengthField.text.EndsWith('.')))
		{
			try
			{
				// get cage length
				newConfig.cageLength = Convert.ToSingle(CageLengthField.text);
			}
			catch (OverflowException)
			{
				// if input too large, set to maximum
				newConfig.cageWidth = Single.MaxValue;
			}
			if (newConfig.cageLength == 0f)
			{
				// change to prevent division by zero
				newConfig.cageLength = 0.001f;
			}
		}
		else
		{
			// if not set or negative, use 0 as default
			newConfig.cageLength = 0.001f;
		}
		
		// get the cage width
		if (CageWidthField.text != "" && CageWidthField.text[0] != '-' && !(CageWidthField.text[0] == '.' && CageWidthField.text.EndsWith('.')))
		{
			try
			{
				// get cage width
				newConfig.cageWidth = Convert.ToSingle(CageWidthField.text);
			}
			catch (OverflowException)
			{
				// if input too large, set to maximum
				newConfig.cageWidth = Single.MaxValue;
			}
			if (newConfig.cageWidth == 0f)
			{
				// change to prevent division by zero
				newConfig.cageWidth = 0.001f;
			}
		}
		else
		{
			// if not set or negative, use 0 as default
			newConfig.cageWidth = 0.001f;
		}
		
		// get the sector length
		if (SectorLengthField.text != "" && SectorLengthField.text[0] != '-' && !(SectorLengthField.text[0] == '.' && SectorLengthField.text.EndsWith('.')))
		{
			try
			{
				// get sector length
				newConfig.sectorLength = Convert.ToSingle(SectorLengthField.text);
			}
			catch (OverflowException)
			{
				// if input too large, set to maximum
				newConfig.sectorLength = Single.MaxValue;
			}
		}
		else
		{
			// if not set or negative, use 0 as default
			newConfig.sectorLength = 0f;
		}
		
		// get the sector count in the x dimension
		if (SectorCountXField.text != "" && SectorCountXField.text[0] != '-')
		{
			try
			{
				// get sector count in x dimension
				newConfig.sectorCountX = Convert.ToUInt32(SectorCountXField.text);
			}
			catch (OverflowException)
			{
				// if input too large, set to maximum
				newConfig.sectorCountX = UInt32.MaxValue;
			}
			
			// keep sector count below max
			if (newConfig.sectorCountX > MAX_SECTOR_COUNT_X)
			{
				newConfig.sectorCountX = MAX_SECTOR_COUNT_X;
			}
		}
		else
		{
			// if not set or negative, use 0 as default
			newConfig.sectorCountX = 0;
		}
		
		// get the sector count in the y dimension
		if (SectorCountYField.text != "" && SectorCountYField.text[0] != '-')
		{
			try
			{
				// get sector count in y dimension
				newConfig.sectorCountY = Convert.ToUInt32(SectorCountYField.text);
			}
			catch (OverflowException)
			{
				// if input too large, use 0 as default
				newConfig.sectorCountY = UInt32.MaxValue;
			}
			
			// keep sector count below max
			if (newConfig.sectorCountY > MAX_SECTOR_COUNT_Y)
			{
				newConfig.sectorCountY = MAX_SECTOR_COUNT_Y;
			}
		}
		else
		{
			// if not set or negative, use 0 as default
			newConfig.sectorCountY = 0;
		}
		
		// return constructed configuration
		ConfigStorage.SetConfig(newConfig);
		return newConfig;
	}
	
	// when a value in an input field changes, invoke an event
	public void OnValueChange()
	{
		// update current config
		ConfigStorage.SetConfig(GetCurrentConfig());
		
		// invoke an event to notify components
		ValueChanged.Invoke();
	}
}
