using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// graphical interface class that implements a visible ruler
// on the sides of the drone cage graphic
public class Ruler : MonoBehaviour
{
	// set in inspector
	// TODO: fix RightSprite's name
	public GameObject MainSprite;	// the ruler rectangle for the bottom of the cage graphic
	public GameObject RightSprite;	// the ruler rectangle for the left of the cage graphic (i know)
	public GameObject LinePrefab;	// a rect used as a template for how ruler lines should appear
	
	CageRenderer Cage;		// the renderer of the drone cage needed for measurement data
	float OldLeft = 0f;		// the left x of the sectors in the cage from last frame (does not include drone area)
	float OldRight = 0f;	// the right x of the cage from last frame
	float OldDown = 0f;		// the bottom y of the cage from last frame
	float OldUp = 0f;		// the top y of the cage from last frame
	float OldUnit = 0f;		// the Unity units measured to be equivalent to 1 foot measured last frame
	List<GameObject> Lines = new List<GameObject>();	// keeps track of created ruler line objects
	
    // Start is called before the first frame update
    void Start()
    {
		// allow the GameObject this script is attached to and
		// all child GameObjects to persist between different scenes 
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
		// when a scene is changed, the cage renderer variable will become
		// null as the CageRenderer will not exist as the same object in the
		// new scene
		// when this happens, find the cage renderer in the new scene and store it
		// TODO: prevent the drone cage renderer from being destroyed on scene loading
        if (Cage == null)
		{
			// find the new cage renderer
			// to facilitate this, all drone cages the 
			// Ruler can interact with must be named "Drone Cage"
			Cage = GameObject.Find("Drone Cage").GetComponent<CageRenderer>() as CageRenderer;
		}
		
		// move the bottom ruler to the correct position and scale it appropriately
		// x should be directly between the leftmost and rightmost edges of the cage 
		//	(linear interpolation between min x and max x)
		// y should be 0.5 unity units below the cage
		// z shouldn't change
		// the ruler should be scaled in the x dimension to cover the sectors in the cage 
		//	(between right and left of cage)
		// 	TODO: allow bottom ruler to cover drone starting area as well
		// the ruler should be scaled in the y dimension to be 0.5 unity units
		// the ruler's scale in the z dimension should not change
		MainSprite.transform.position = new Vector3(Mathf.Lerp(Cage.MIN_X, Cage.MAX_X, 0.5f), 
			Cage.down - 0.5f, 
			MainSprite.transform.position.z);
		MainSprite.transform.localScale = new Vector3(Cage.right - Cage.left, 0.5f, 1f);
		
		// move the left ruler to the correct position and scale it appropriately
		// x should be 0.5 unity units left of the cage
		// y should be directly between the topmost and bottommost edges of the cage 
		//	(linear interpolation between min y and max y)
		// z shouldn't change
		// the ruler should be scaled in the x dimension to be 0.5 unity units
		// the ruler should be scaled in the y dimension to be the height of the cage (up - down)
		// the ruler's scale in the z dimension should not change
		RightSprite.transform.position = new Vector3(Cage.left - 0.5f, 
			Mathf.Lerp(Cage.MIN_Y, 
			Cage.MAX_Y, 0.5f), 
			RightSprite.transform.position.z);
		RightSprite.transform.localScale = new Vector3(0.5f, Cage.up - Cage.down, 1f);
		
		// if any of the cage's borders or the feet-to-unity-units ratio have changed since last
		// frame, update the ruler marks
		// TODO: have the position and scale setting happen under this condition as well
		if (Cage.up != OldUp || 
			Cage.down != OldDown || 
			Cage.left != OldLeft || 
			Cage.right != OldRight || 
			Cage.FeetToPixels(1f) != OldUnit)
		{
			// update the ruler markings
			UpdateMarks();
		}
		
		// track the locations of the cage's edges and the feet-to-unity-units ratio so changes can
		// be detected
		OldLeft = Cage.left;
		OldRight = Cage.right;
		OldUp = Cage.up;
		OldDown = Cage.down;
		OldUnit = Cage.FeetToPixels(1f);
			
    }
	
	//
	void UpdateMarks()
	{
		float pixelsInFoot = Cage.FeetToPixels(1f);
		int totalFeet = (int)Mathf.Floor(MainSprite.transform.localScale.x / pixelsInFoot);
		int totalFeetVertical = (int)Mathf.Floor(RightSprite.transform.localScale.y / pixelsInFoot);
		
		ClearLines();
		
		for (int i = 1; i < totalFeet && i < 500; i++)
		{
			
			
			GameObject newLine = Instantiate(LinePrefab, transform);
			
			
			
			if (i % 5 == 0)
			{
				newLine.transform.localScale = new Vector3(
					newLine.transform.localScale.x,
					0.5f * MainSprite.transform.localScale.y,
					newLine.transform.localScale.z);
				
				newLine.transform.position = new Vector3(
					Cage.left + (pixelsInFoot * i),
					MainSprite.transform.position.y - (0.25f * MainSprite.transform.localScale.y),
					10f);
			}
			else
			{
				newLine.transform.localScale = new Vector3(
					newLine.transform.localScale.x,
					0.25f * MainSprite.transform.localScale.y,
					newLine.transform.localScale.z);
				
				newLine.transform.position = new Vector3(
					Cage.left + (pixelsInFoot * i),
					MainSprite.transform.position.y - (0.375f * MainSprite.transform.localScale.y),
					10f);
			}
			
			Lines.Add(newLine);
			newLine.SetActive(true);
		}
		
		for (int i = 1; i < totalFeetVertical && i < 500; i++)
		{
			
			
			GameObject newLine = Instantiate(LinePrefab, transform);
			
			
			
			if (i % 5 == 0)
			{
				newLine.transform.localScale = new Vector3(
					0.5f * RightSprite.transform.localScale.x,
					0.03f,
					newLine.transform.localScale.z);
				
				newLine.transform.position = new Vector3(
					RightSprite.transform.position.x - (0.25f * RightSprite.transform.localScale.x),
					Cage.down + (pixelsInFoot * i),
					10f);
			}
			else
			{
				newLine.transform.localScale = new Vector3(
					0.25f * RightSprite.transform.localScale.x,
					0.03f,
					newLine.transform.localScale.z);
				
				newLine.transform.position = new Vector3(
					RightSprite.transform.position.x - (0.375f * RightSprite.transform.localScale.x),
					Cage.down + (pixelsInFoot * i),
					10f);
			}
			
			Lines.Add(newLine);
			newLine.SetActive(true);
		}
	}
	
	void ClearLines()
	{
		foreach (GameObject line in Lines)
		{
			Destroy(line);
		}
		
		Lines.Clear();
	}
}
