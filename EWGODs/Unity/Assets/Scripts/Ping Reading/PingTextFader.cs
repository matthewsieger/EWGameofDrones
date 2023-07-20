using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PingTextFader : MonoBehaviour
{
	TMP_Text Text;
	float Alpha = 1f;
	
    // Start is called before the first frame update
    void Start()
    {
        Text = gameObject.GetComponent<TMP_Text>() as TMP_Text;
    }

    // Update is called once per frame
    void Update()
    {
		
        Alpha -= 2f * Time.deltaTime;
		
		Text.alpha = Alpha;
		
		if (Alpha <= 0f)
		{
			Destroy(gameObject);
		}
    }
}
