using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// keep the value in the float input field able to be converted to a positive float
public class PosFloatConstrainer : FloatConstrainer
{
	// runs when input field is modified
    public override void ConstrainInput(string change)
	{
		// ensure the input can be a float
		base.ConstrainInput(change);
		
		// prevent situations where the string in the input field isn't positive
		if (change.Length > 0 && change[0] == '-')
		{
			// prevent the float from being negative ('-' as first character)
			if (change.Length > 1)
			{
				// if there are other characters, clear it to "0"
				change = "0";
			}
			else
			{
				// if the whole string is "-", clear the string
				change = "";
			}
			
			// update the input field
			InputField.text = change;
			
			// move the cursor to position 0
			InputField.stringPosition = 0;
		}
	}
}
