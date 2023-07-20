using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AddingButton : MonoBehaviour
{
	// set in inspector
	public float AddingValue;		// value to increment by when clicked
	public GameObject InputObject;	// object with input field that is being affected
	
	FloatParser Input;			// parser to parse user input
	TMP_InputField InputField;	// input field to affect
	
	
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnButtonClick);		// listen for button presses
		Input = InputObject.GetComponent<FloatParser>() as FloatParser;				// get the input reader
		InputField = InputObject.GetComponent<TMP_InputField>() as TMP_InputField;	// get the input field
    }
	
	// runs when button clicked
	void OnButtonClick()
	{
		// add to the value in the input field
		InputField.text = (Input.ReadField() + AddingValue).ToString();
	}
}
