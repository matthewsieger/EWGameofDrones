using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// keep the value in the Y input field within constraints
public class YConstrainer : PosFloatConstrainer
{
	// runs when input field is modified
    public override void ConstrainInput(string change)
	{
		// constrain the string to a positive float (parent class)
		base.ConstrainInput(change);
		
		// read the input field
		float input = Input.ReadField();
		
		if (input > ConfigStorage.GetConfig().cageWidth)
		{
			// if the input is larger than the amount of feet in the Y dimension in
			// the cage, lower it to that value
			InputField.text = (ConfigStorage.GetConfig().cageWidth).ToString();
		}
	}
}
