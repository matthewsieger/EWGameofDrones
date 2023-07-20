using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ensure user input in the ID field remains parsable and within bounds
public class IDConstrainer : MonoBehaviour
{
	TMP_InputField InputField;	// input field object
	
    // Start is called before the first frame update
    void Start()
    {
		// get the components needed
		InputField = gameObject.GetComponent<TMP_InputField>() as TMP_InputField;
		
		// listen for changes to the input fields's value
		InputField.onValueChanged.AddListener(ConstrainInput);
    }
	
	// keep the input within acceptable range
    void ConstrainInput(string change)
	{
		// prevent negative ids (there are no negative ids)
		if (change.Length > 0 && change[0] == '-')
		{
			// if first character is '-', remove it
			if (change.Length > 1)
			{
				// remove only the '-' if there are more characters in the string
				InputField.text = change.Substring(1);
			}
			else
			{
				// clear the string if it is only "-"
				InputField.text = "";
			}
		}
		
		// prevent the use of id 0 (coodinator id)
		if (change.Length > 0 && change[0] == '0')
		{
			// if first character is '0' remove it
			if (change.Length > 1)
			{
				// remove only the '0' if there are more characters in the string
				InputField.text = change.Substring(1);
			}
			else
			{
				// clear the string if it is only "0"
				InputField.text = "";
			}
		}
	}
}
