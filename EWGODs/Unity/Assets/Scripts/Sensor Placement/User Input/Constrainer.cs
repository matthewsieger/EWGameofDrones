using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// constrain values in input fields
public abstract class Constrainer : MonoBehaviour
{
	[HideInInspector]
	public FloatParser Input;			// script that gets input from input field
	[HideInInspector]
	public TMP_InputField InputField;	// input field
	
    // Start is called before the first frame update
    void Start()
    {
		// get the components needed
		Input = gameObject.GetComponent<FloatParser>() as FloatParser;
		Debug.Assert(Input != null, "Failed to get FloatParser!", this);
		InputField = gameObject.GetComponent<TMP_InputField>() as TMP_InputField;
		
		// listen for changes to the input fields's value
		InputField.onValueChanged.AddListener(ConstrainInput);
    }

	// runs when input field is modified
    public abstract void ConstrainInput(string change);
}
