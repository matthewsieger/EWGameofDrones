using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// keep the value in the X input field within constraints
public class XConstrainer : PosFloatConstrainer
{
	// runs when input field is modified
    public override void ConstrainInput(string change)
	{
		// constrain the input to be a positive float (parent class)
		base.ConstrainInput(change);
		
		// read the input field
		float input = Input.ReadField();
		
		if (input > ConfigStorage.GetConfig().sectorLength * ConfigStorage.GetConfig().sectorCountX)
		{
			// if the input is larger than the amount of feet in the X dimension of the cage
			// available to red team (sectors in x dimension * length of sectors) than lower it
			// to that value
			InputField.text = (ConfigStorage.GetConfig().sectorLength * ConfigStorage.GetConfig().sectorCountX).ToString();
			
			// move the cursor to the end
			InputField.MoveTextEnd(false);
		}
	}
}
