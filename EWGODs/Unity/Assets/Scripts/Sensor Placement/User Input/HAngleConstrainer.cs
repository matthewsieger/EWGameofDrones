using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// keep the value in the input field within a 0-360 degree range
public class HAngleConstrainer : FloatConstrainer
{
	// runs when input field is modified
    public override void ConstrainInput(string change)
	{
		// constrain the input field text to be in float form
		base.ConstrainInput(change);
		
		// read the text into a float
		float fieldValue = Input.ReadField();
		
		// keep the value within a 0-360 degree range
		if (fieldValue >= 360f)
		{
			// if the value is greater than or equal to 360,
			// subtract 360 from it
			// changing InputField.text causes this function to
			// run again
			InputField.text = (fieldValue - 360f).ToString();
			
			// move the cursor to the end
			InputField.MoveTextEnd(false);
		}
		else if (fieldValue < 0f)
		{
			// if the value is less than 0,
			// add 360 to it
			// changing InputField.text causes this function to 
			// run again
			InputField.text = (fieldValue + 360f).ToString();
			
			// move the cursor to the end
			InputField.MoveTextEnd(false);
		}
	}
}
