using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingFader : MonoBehaviour
{
	SpriteRenderer Renderer;
	Color SpriteColor;
	
    // Start is called before the first frame update
    void Start()
    {
        Renderer = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
		SpriteColor = Renderer.color;
    }

    // Update is called once per frame
    void Update()
    {
		
        SpriteColor.a -= 2f * Time.deltaTime;
		
		Renderer.color = SpriteColor;
		
		if (SpriteColor.a <= 0f)
		{
			Destroy(gameObject);
		}
    }
}
