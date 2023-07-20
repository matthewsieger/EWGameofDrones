using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

// convert user input in input field into float for further processing
public class FloatParser : MonoBehaviour
{
	// set in inspector
	public string Default;	// default value when field empty
	
	TMP_InputField InputField;	// the input field to read input from
	
	// called before first frame update
	void Start()
	{
		// retrieve the input field component
		InputField = gameObject.GetComponent<TMP_InputField>() as TMP_InputField;
		
		// ensure default value is set
		Debug.Assert(Default != "", "Default float field value is not set!", this);
	}
	
	// convert the text in an input field to a float
	// while keeping in mind potential errors
	public float ReadField()
	{
		string input = InputField.text;
		
		// if the input is an empty string, an error can happen.
		// set it to default value to avoid this
		// same if it is a single negative sign or a negative decimal point
		if (input == "" || input == "-" || input == "-.")
		{
			input = Default;
		}
		
		// if the input starts with a ".", an error can happen
		// change the input to avoid this
		if (input[0] == '.')
		{
			// insert a '0' at the beginning to avoid
			input = input.Insert(0, "0");
			
			// if the input ends with '.' an error can still happen
			// (this doesn't appear to happen with other numbers. ie: "5."
			// add a trailing 0 to prevent an error
			if (input == "0.")
			{
				input += "0";
			}
		}
		
		// attempt to convert the input from a string into a float
		try
		{
			return Convert.ToSingle(input);
		}
		catch (OverflowException)
		{
			// if an OverflowException was caught, simply return the maximum value a float can have
			return Single.MaxValue;
		}
	}
}
