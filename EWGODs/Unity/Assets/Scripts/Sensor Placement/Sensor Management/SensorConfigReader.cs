using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// allow user input to be combined into a single struct
public class SensorConfigReader : MonoBehaviour
{
    // set in inspector
	public TypeParser TypeDropdown;		// user input for sensor type
	public IntParser IDField;			// user input for sensor id
	public FloatParser XField;			// user input for sensor x
	public FloatParser YField;			// user input for sensor y
	public FloatParser HRotationField;	// user input for sensor horizontal rotation
	public FloatParser VRotationField;	// user input for sensor vertical rotation
	
	// read user input into a SensorConfiguration struct
	public SensorConfiguration ReadSensor()
	{
		// create the struct
		SensorConfiguration Sensor = new SensorConfiguration();
		
		// populate struct from user input
		Sensor.type = TypeDropdown.ReadDropdown();
		Sensor.id = IDField.ReadField();
		Sensor.x = XField.ReadField();
		Sensor.y = YField.ReadField();
		Sensor.hRotation = HRotationField.ReadField();
		Sensor.vRotation = VRotationField.ReadField();
		
		// return configuration
		return Sensor;
	}
}
