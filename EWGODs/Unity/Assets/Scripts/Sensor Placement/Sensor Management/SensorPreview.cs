using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// manages the preview of the currently being edited sensor
public class SensorPreview : MonoBehaviour
{
	// set in inspector
	public SensorConfigReader Reader;		// to read user input into a SensorConfiguration
	public GameObject SensorTemplate;		// template for sensor visuals
	public TMP_Dropdown TypeDropdown;		// type dropdown menu
	public TMP_InputField IDField;			// id text field
	public TMP_InputField XField;			// x text field
	public TMP_InputField YField;			// y text field
	public TMP_InputField HRotationField;	// horizontal rotation text field
	public TMP_InputField VRotationField;	// vertical rotation text field
	
	GameObject PreviewObject;		// reference to the visual sensor managed
	SensorRenderer PreviewRenderer;	// reference to the visual sensor's SensorRenderer script
	
    // Start is called before the first frame update
    void Start()
    {
		// set methods to execute automatically when changes are made
		// to the user inputs
		TypeDropdown.onValueChanged.AddListener(DropdownChanged);
		IDField.onValueChanged.AddListener(TextFieldChanged);
		XField.onValueChanged.AddListener(TextFieldChanged);
		YField.onValueChanged.AddListener(TextFieldChanged);
		HRotationField.onValueChanged.AddListener(TextFieldChanged);
		VRotationField.onValueChanged.AddListener(TextFieldChanged);
		
    }

	// listens for changes to text fields
	// necessary because the onValueChanged event for text fields calls
	// methods with a string argument
	void TextFieldChanged(string text)
	{
		// run the code to handle data changes
		OnDataChange();
	}
	
	// listens for changes to dropdown menus
	// necessary because the onValueChanged event for dropdown menus calls
	// methods with an int argument
	void DropdownChanged(int index)
	{
		// run the code to handle data changes
		OnDataChange();
	}
	
	// runs whenever specified user inputs change
	void OnDataChange()
	{	
		// if no visual preview exists, create one
		if (PreviewObject == null)
		{
			// create sensor
			PreviewObject = Instantiate(SensorTemplate, transform);
			// enable sensor (calls SensorRenderer OnEnable() code)
			PreviewObject.SetActive(true);
			// store reference to SensorRenderer script
			PreviewRenderer = PreviewObject.GetComponent<SensorRenderer>() as SensorRenderer;
		}
		
		// update the preview with the current configuration specified by the user
		PreviewRenderer.UpdateConfig(Reader.ReadSensor());
	}

	// remove the visual preview
	public void ResetPreview()
	{
		// destroy the preview object
		Destroy(PreviewObject);
		
		// prevent bad reference
		PreviewObject = null;
	}
}
