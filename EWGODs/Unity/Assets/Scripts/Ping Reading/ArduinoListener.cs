using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Linq;
using TMPro;

// class that handles receiving data from the coordinator Arduino
public class ArduinoListener : MonoBehaviour
{
	
	List<JamOrder> JamOrders = new List<JamOrder>();
	
	// object representing the figurative connection
	// between an instance of this class and an
	// arduino.
	// Handles communication via serial port
	SerialPort Port;
	
	// set in inspector
	public CageRenderer Cage;			// reference to CageRenderer for calculations
	public GameObject PingTemplate;		// reference to template ping object
	public TMP_Text PingTextTemplate;	// reference to template ping text object
	public SensorManager Manager;		// for accessing sensor data
	public TMP_Text ConnectionFail;		// message signifying a failed connection
	public TMP_Text ConnectionSucceed;	// message signifying successful connection
	public TMP_Text NoConnection;		// instructions for connecting to coordinator
	
	GameObject ConnectionMessage;
	
	// list of types of sensors that can handle ping packets
	List<Sensor> Sensors = new List<Sensor>();
	
	// input from the serial port is stored here
	string Buffer;
	
	const float MAX_JAM_TIME_BUFFER = 0.2f;
	
    // Start is called before the first frame update
    void Start()
    {
		// update the positions of visual sensors so that they are aligned with the cage in the ArduinoListener's scene
		Manager = GameObject.Find("Sensors").GetComponent<SensorManager>() as SensorManager;
		Manager.UpdateSensors();
		
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
			sensor.PingTextTemplate = PingTextTemplate;
			sensor.Manager = Manager;
        }
    }
	
	public void Connect(int comNum)
	{
		if (Port != null)
		{
			Port.Close();
		}
		
		// open serial port communication with the Arduino
		Port = new SerialPort();
		Port.PortName = "COM" + comNum.ToString();	// set in inspector in format "COMX" where X is the COM number
		Port.BaudRate = 115200;	// matches the baud rate in the Arduino's code
		Port.DtrEnable = true;	// seems to help with preventing split packets
		
		if (ConnectionMessage != null)
			{
				Destroy(ConnectionMessage);
				ConnectionMessage = null;
			}
		
		// try to open the COM port
		try
		{
			// open the port
			Port.Open();
			
			NoConnection.gameObject.SetActive(false);
			ConnectionSucceed.text = "Connected to Coordinator on COM" + comNum.ToString();
			ConnectionMessage = Instantiate(ConnectionSucceed.gameObject, ConnectionSucceed.transform.parent);
			ConnectionMessage.SetActive(true);
		}
		catch
		{
			// if the port wasn't able to be opened,
			// keep Port as null
			Port = null;
			
			NoConnection.gameObject.SetActive(true);
			ConnectionFail.text = "Connection Failed on COM" + comNum.ToString();
			ConnectionMessage = Instantiate(ConnectionFail.gameObject, ConnectionFail.transform.parent);
			ConnectionMessage.SetActive(true);
		}
	}
	
	// Update is called once every frame
	void Update()
	{
		
		// do nothing if not connected to the Arduino Coordinator
		if (Port == null)
		{
				return;
		}
		
		// create jam pings for each order to jam
		for (int i = 0; i < JamOrders.Count; i++)
		{
			// check eligibility of jamming each type of sensor
			foreach (Sensor sensorType in Sensors)
			{
				// if the sensor type matches the jam order (matching type or type is 0) then jam it
				if (sensorType.Type == JamOrders[i].type || JamOrders[i].type == 0)
				{
					// jam all sensors of the type given
					sensorType.Jam(JamOrders[i].id);
				}
			}
			
			// reduce the amount of time the jam order has remaining
			JamOrders[i].time -= Time.deltaTime;
		}
		
		// remove all jam orders that have run out of time
		JamOrders.RemoveAll(j => j.time <= 0f);
		
		// read the data being transmitted
		try
		{
			Buffer += Port.ReadExisting();
		}
		catch
		{
			Port = null;
			return;
		}
		
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
			// create an order to jam one or more sensors
			JamOrder newJamOrder = new JamOrder();
			
			// if there is additional data in the ping, use it to specify sensor type
			if (ping.data.Count > 0)
			{
				// use the number given as the type of sensor to affect. 0 means affect all sensor types
				newJamOrder.type = Convert.ToUInt16(ping.data[0]);	// sensor type to jam. 0 means jam all sensor types
			}
			else
			{
				// assume 0 (all sensor types) if not given in ping packet
				newJamOrder.type = 0;
			}
			
			// if there is additional data in the ping, use it to specify sensor id
			if (ping.data.Count > 1)
			{
				// use the number given as the sensor id to affect. 0 means affect all sensor ids
				newJamOrder.id = Convert.ToUInt16(ping.data[1]);	// sensor id to jam. 0 means jam all sensor ids
			}
			else
			{
				// assume 0 (all sensor ids) if not given in ping packet
				newJamOrder.id = 0;
			}
			
			newJamOrder.time = MAX_JAM_TIME_BUFFER;	// time for jam to last after last associated jam packet
			
			// if the jam order already exists, refresh it instead of creating a new one
			foreach(JamOrder jam in JamOrders)
			{
				// check type and id to see if they match
				if (jam.type == newJamOrder.type && jam.id == newJamOrder.id)
				{
					// if the jam order is the same, refresh the duration
					jam.time = MAX_JAM_TIME_BUFFER;
					return;
				}
			}
			
			// create a new jam order
			JamOrders.Add(newJamOrder);
			
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
					Debug.Log(ping.type);
					// if the ping is for this type of sensor, handle the packet
					sensorType.PlotPing(ping);
					
					return;
				}
			}
		}
	}
}
