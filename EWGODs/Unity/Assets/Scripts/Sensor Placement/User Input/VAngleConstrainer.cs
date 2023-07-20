using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// keep the value in the input field within a -90 to +90 degree range
public class VAngleConstrainer : FloatConstrainer
{
	// runs when input field is modified
    public override void ConstrainInput(string change)
	{
		// constrain the input to be a float
		base.ConstrainInput(change);
		
		// read the field as a float
		float fieldValue = Input.ReadField();
		
		// keep the value within a -90 to +90 degree range
		if (fieldValue > 90f)
		{
			// if the value is greater than 90,
			// set it to 90
			InputField.text = (90f).ToString();
			
			// move the cursor to the end
			InputField.MoveTextEnd(false);
		}
		else if (fieldValue < -90f)
		{
			// if the value is less than -90,
			// set it to -90
			InputField.text = (-90).ToString();
			
			// move the cursor to the end
			InputField.MoveTextEnd(false);
		}
	}
}
