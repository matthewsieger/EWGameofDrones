using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Linq;

// class that handles receiving data from the coordinator Arduino
public class ArduinoListener : MonoBehaviour
{
	// object representing the figurative connection
	// between an instance of this class and an
	// arduino.
	// Handles communication via serial port
	SerialPort Port;
	
	// set in inspector
	public string COM;				// the serial port the arduino can be accessed from
	public Jammer SensorJammer;		// interface for the jamming functionality of this application
	public CageRenderer Cage;		// reference to CageRenderer for calculations
	public GameObject PingTemplate;	// reference to template ping object
	public SensorManager Manager;	// for accessing sensor data
	
	// list of types of sensors that can handle ping packets
	List<Sensor> Sensors = new List<Sensor>();
	
	// input from the serial port is stored here
	string Buffer;
	
	const float MAX_JAM_TIME_BUFFER = 0.2f;
	float JamTimeBuffer = 0f;
	
    // Start is called before the first frame update
    void Start()
    {
		// update the positions of visual sensors so that they are aligned with the cage in the ArduinoListener's scene
		Manager = GameObject.Find("Sensors").GetComponent<SensorManager>() as SensorManager;
		Manager.UpdateSensors();
		
		// open serial port communication with the Arduino
		Port = new SerialPort();
		Port.PortName = COM;	// set in inspector in format "COMX" where X is the COM number
		Port.BaudRate = 115200;	// matches the baud rate in the Arduino's code
		Port.DtrEnable = true;	// seems to help with preventing split packets
		Port.Open();			// open the port
		
		// find all sensor types by finding subclasses of "Sensor"
        System.Type[] subclasses = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Sensor)))
            .ToArray();

        foreach (System.Type subclass in subclasses)
        {
            // save each sensor type found and provide it with references to the components
			// it needs
            Sensor sensor = gameObject.AddComponent(subclass) as Sensor;
			Sensors.Add(sensor);
			sensor.Cage = Cage;
			sensor.PingTemplate = PingTemplate;
			sensor.Manager = Manager;
        }
    }
	
	// Update is called once every frame
	void Update()
	{
		if (JamTimeBuffer > 0f)
		{
			// jam all sensors
			foreach(Sensor sensorType in Sensors)
			{
				// jam all sensors of this sensor type
				sensorType.Jam();
			}
			
			JamTimeBuffer -= Time.deltaTime;
			if (JamTimeBuffer < 0f)
			{
				JamTimeBuffer = 0f;
			}
		}
		
			
		
		// read the data being transmitted
		Buffer += Port.ReadExisting();
		
		// if the buffer contains "\r\n", then the end of a packet has been read
		while (Buffer.Contains("\r\n"))
		{
			// split the buffer up into individual packets
			// packets in theory always end with "\r\n"
			// further confirmation of this fact is recommended
			string[] splitBuffer = Buffer.Split("\r\n");
			
			
			// "Starting..." is not a sensor ping and should be ignored
			// "0" is a null packet that was needed in intervals in previous iterations
			// of this GUI application, supposedly to prevent MATLAB from
			// losing connection. It should be ignored here
			if (splitBuffer[0] != "Starting..." && splitBuffer[0] != "0" && splitBuffer[0] != "")
			{
				// read through the packet
				ParsePing(splitBuffer[0]);
			}
			
			// reform the split up packets back into a single string
			// for the next pass through the loop
			Buffer = splitBuffer[1];	// skip index 0 as we just dealt with that packet
			for (int i = 2; i < splitBuffer.Length; i++)
			{
				// append each remaining packet back onto the buffer
				// WARNING: This process could potentially lead to a bug in which 
				// 	at the start of this loop 2+ whole packets are split from
				// 	the buffer. When the last packet is added back onto the buffer,
				// 	its "\r\n" isn't readded with it
				Buffer += "\r\n" + splitBuffer[i];
			}
		}
	}
	
	// parse data in a complete packet and handle it appropriately
	void ParsePing(string input)
	{
		
		
		// parse the data as a ping packet
		Ping ping = new Ping(input);
		
		if (ping.type == 3)	// jam packet
		{
			JamTimeBuffer = MAX_JAM_TIME_BUFFER;
			
			return;
		}
		else
		{
		
			// have the appropriate sensor type handle the ping packet
			foreach(Sensor sensorType in Sensors)
			{
				// check the type of the ping packet and the type of the sensor
				if (ping.type == sensorType.Type)
				{
					// if the ping is for this type of sensor, handle the packet
					sensorType.PlotPing(ping);
					
					return;
				}
			}
		}
	}
}
