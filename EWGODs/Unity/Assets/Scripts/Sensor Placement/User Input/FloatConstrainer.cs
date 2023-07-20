using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// keep the value in the float input field able to be converted to a float
public class FloatConstrainer : Constrainer
{
	// runs when input field is modified
    public override void ConstrainInput(string change)
	{
		// prevent situations where the string in the input field cannot be converted into a float
		if (change.Length > 0 && change[0] == '.')
		{
			// if the first character is '.', insert a '0' beforehand
			change = change.Insert(0, "0");
			
			// update the input field
			InputField.text = change;
			
			// move the cursor 1 forward because a character was added
			InputField.stringPosition++;
		}
		else if (change.Length > 1 && change[0] == '0' && change[1] != '.')
		{
			// if there are leading zeroes (not "0" itself), remove the first one
			// this causes recursion as this event will be called again as we 
			// remove this zero which will again call this function.
			// this allows removal of all leading zeroes
			change = change.Substring(1);
			
			// update the input field
			InputField.text = change;
		}
	}
}
