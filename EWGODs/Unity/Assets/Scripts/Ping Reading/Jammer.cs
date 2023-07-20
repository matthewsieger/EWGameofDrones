using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jammer : MonoBehaviour
{
	//public PingRenderer Pings;
	SensorManager Sensors;
	
	List<Ping> FakePings = new List<Ping>();
	
	const float MAX_JAM_TIME = 0.1f;
	float JamTime = 0f;
	
	public void Jam()
	{
		
		
		/*if (JamTime <= 0f)
		{
			List<SensorRenderer> jammedRenderers = Sensors.GetSensors();
			List<SensorConfiguration> jammedSensors = new List<SensorConfiguration>();
			foreach(SensorRenderer renderer in jammedRenderers)
			{
				jammedSensors.Add(renderer.Config);
			}
			
			FakePings = new List<Ping>();
			
			foreach(SensorConfiguration sensor in jammedSensors)
			{
				int maxI = 50;
				
				for (int i = 0; i < maxI; i++)
				{
					Ping jamPing = new Ping();
					jamPing.id = sensor.id;
					jamPing.r = Random.Range(0f, 500f);
					jamPing.theta = 90;//Random.Range(0f, 180f);
					jamPing.phi = 0;//Random.Range(0f, 180f);
				
					FakePings.Add(jamPing);
				}
			}
			
			
		}
		
		JamTime = MAX_JAM_TIME;
		
		
		*/
		
	}
	
	public void DisplayJammed()
	{
		/*if (JamTime > 0f)
		{
			foreach(Ping ping in FakePings)
			{
				Debug.Log(ping.r);
				Ping movedPing = ping;
				movedPing.r += 18 * (Random.value - 0.5f);
				//movedPing.theta += (Random.value - 0.5f);
				//movedPing.phi += (Random.value - 0.5f);
				Pings.PlotPing(movedPing);
			}
		}*/
	}
	
    // Start is called before the first frame update
    void Start()
    {
        Sensors = GameObject.Find("Sensors").GetComponent<SensorManager>() as SensorManager;
    }

    // Update is called once per frame
    void Update()
    {
		
        JamTime -= Time.deltaTime;
		if (JamTime < 0f)
		{
			JamTime = 0f;
		}
    }
}
