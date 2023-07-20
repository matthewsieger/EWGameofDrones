using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

// convert user input in input field into an unsigned int for further processing
public class IntParser : MonoBehaviour
{
	// set in inspector
	public string Default;
	
	TMP_InputField InputField;	// the input field to read input from
	
	// called before first frame update
	void Start()
	{
		// retrieve the input field component
		InputField = gameObject.GetComponent<TMP_InputField>() as TMP_InputField;
		
		// ensure default value is set
		Debug.Assert(Default != "", "Default int field value is not set!", this);
	}
	
	// convert the text in an input field to an unsigned int
	// while keeping in mind potential errors
	public uint ReadField()
	{
		string input = InputField.text;
		
		// if the input is an empty string, an error can happen.
		// set it to the default value to avoid this
		if (input == "")
		{
			// set to default
			input = Default;
		}
		
		// attempt to convert the input from a string into an unsigned int
		try
		{
			return Convert.ToUInt32(input);
		}
		catch (OverflowException)
		{
			// if an OverflowException was caught, simply return the maximum value an unsigned int can have
			return uint.MaxValue;
		}
	}
}
