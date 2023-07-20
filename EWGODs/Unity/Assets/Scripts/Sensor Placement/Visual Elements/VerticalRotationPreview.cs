using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// manages the preview of the currently being edited sensor
public class VerticalRotationPreview : MonoBehaviour
{
	// set in inspector
	public TMP_InputField VRotationField;	// vertical rotation text field
	
	FloatParser RotationReader;
	Transform Arrow;
	
    // Start is called before the first frame update
    void Start()
    {
		VRotationField.onValueChanged.AddListener(RotationChanged);

		RotationReader = VRotationField.gameObject.GetComponent<FloatParser>() as FloatParser;
		
		Arrow = transform.Find("Arrow");
    }

	void RotationChanged(string text)
	{
		Vector3 rotation = new Vector3(0f, 0f, 0f);
		rotation.z = RotationReader.ReadField();
		
		Arrow.eulerAngles = rotation;
	}
}
