using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// handles rendering of the drone cage visual
public class CageRenderer : MonoBehaviour
{
	// most extreme dimensions allowed of the drone cage visual
	public float MAX_X;
	public float MIN_X;
	public  float MAX_Y;
	public float MIN_Y;
	
	// set in inspector
	public LineRenderer CageEdge;		// line renderer displaying edges of the drone cage
	public GameObject LineTemplate;		// prefab to make it easy to create new lines
	public GameObject DroneStartBox;	// box for the drones to start in
	public GameObject LabelPrefab;		// prefab object for sector labels
	
	List<LineRenderer> VerticalSectorEdges = new List<LineRenderer>();
	List<LineRenderer> HorizontalSectorEdges = new List<LineRenderer>();
	List<TMP_Text> SectorLabels = new List<TMP_Text>();
	
	[HideInInspector]
	public float left, right, up, down;
	
    // Start is called before the first frame update
    void Start()
    {
		// listen to event for when input fields are edited
        CageConfigDropdown.ValueChanged.AddListener(OnDataChange);
		OnDataChange();	// call event at startup
    }
	
	public float FeetToPixels(float feet)
	{
		CageConfiguration config = ConfigStorage.GetConfig();
		
		if (config.cageLength / config.cageWidth > (MAX_X - MIN_X) / (MAX_Y - MIN_Y))
		{
			// | if input length of cage * scale factor = visual length of cage,
			// | then scale factor = visual length of cage / input length of cage
			// | visual length of cage is end to end (so MAX - MIN)
			return feet * ((MAX_X - MIN_X) / config.cageLength);
		}
		else
		{
			// | if input width of cage * scale factor = visual width of cage,
			// | then scale factor = visual width of cage / input width of cage
			// | visual width of cage is end to end (so MAX - MIN)
			return feet * ((MAX_Y - MIN_Y) / config.cageWidth);
		}
	}
	
	// when values in input fields are changed, update the drone cage visual
	void OnDataChange()
	{
		// get the new config data
		CageConfiguration config = ConfigStorage.GetConfig(); //Data.GetCurrentConfig();
		
		// hardcode values if they would case errors (zero division)
		if (config.cageLength == 0f && config.cageWidth == 0)
		{
			config.cageLength = 1f;
			config.cageWidth = 1f;
		}
		if (config.sectorLength == 0)
		{
			config.sectorLength = 1;
		}
		
		
		
		// in order to fit the drone cage visual on the screen while maintaining the ratio
		// between its length and its width, we must figure out which edges will hit the 
		// maximum and minimum values first when scaled to fit the screen
		if (config.cageLength / config.cageWidth > (MAX_X - MIN_X) / (MAX_Y - MIN_Y))
		{
			// if the length should go from edge to edge:
			// x should be min or max
			// y should be calculated
			// | if input length of cage * scale factor = visual length of cage,
			// | then scale factor = visual length of cage / input length of cage
			// | visual length of cage is end to end (so MAX - MIN)
			// | we can scale width by the same factor:
			// | visual width of cage = input width of cage * scale factor
			// |
			// | we divide result by 2 because half of length is left of center of cage
			// | and half is right of center of cage
			// |
			// | we then need to get center of cage to be at center of viewport on screen
			// | center of line can be calculated by adding both ends (min and max) and dividing by 2
			// | add this value to result to translate cage visual to center of viewport on screen
			left = MIN_X;
			right = MAX_X;
			up = (config.cageWidth * ((MAX_X - MIN_X) / config.cageLength) / 2) + (MIN_Y + MAX_Y)/2;
			down = (-config.cageWidth * ((MAX_X - MIN_X) / config.cageLength) / 2) + (MAX_Y + MIN_Y)/2;
		}
		else
		{
			// if the width should go from edge to edge:
			// x should be calculated
			// | if input width of cage * scale factor = visual width of cage,
			// | then scale factor = visual width of cage / input width of cage
			// | visual width of cage is end to end (so MAX - MIN)
			// | we can scale length by the same factor:
			// | visual length of cage = input length of cage * scale factor
			// |
			// | we divide result by 2 because half of width is  above center of cage
			// | and half is below center of cage
			// |
			// | we then need to get center of cage to be at center of viewport on screen
			// | center of line can be calculated by adding both ends (min and max) and dividing by 2
			// | add this value to result to translate cage visual to center of viewport on screen
			//
			// y should be min or max
			left = (-config.cageLength * ((MAX_Y - MIN_Y) / config.cageWidth) / 2f) + (MAX_X + MIN_X)/2f;
			right = (config.cageLength * ((MAX_Y - MIN_Y) / config.cageWidth) / 2f) + (MAX_X + MIN_X)/2f;
			up = MAX_Y;
			down = MIN_Y;
		}
		
		// update display with corner coordinates
		// CAGE EDGE CORNERS
		// 0 - Top Left
		// 1 - Bottom Left
		// 2 - Bottom Right
		// 3 - Top Right
		CageEdge.SetPosition(0, new Vector3(left, up, 1f));
		CageEdge.SetPosition(1, new Vector3(left, down, 1f));
		CageEdge.SetPosition(2, new Vector3(right, down, 1f));
		CageEdge.SetPosition(3, new Vector3(right, up, 1f));
		
		// adjust the amount of vertical lines to match the number of sectors
		// in the x dimension
		
		// decrease the amount of vertical lines if necessary
		while (VerticalSectorEdges.Count > config.sectorCountX)
		{
			Destroy(VerticalSectorEdges[0].gameObject);	// destroy the first line
			VerticalSectorEdges.Remove(VerticalSectorEdges[0]);	// remove it from the list
		}
		
		// increase the amount of vertical lines if necessary
		while (VerticalSectorEdges.Count < config.sectorCountX)
		{
			GameObject lineObject = Instantiate(LineTemplate, transform);	// create a new line
			VerticalSectorEdges.Add(lineObject.GetComponent<LineRenderer>() as LineRenderer);	// add it to the list
		}
		
		// keep track of the leftmost point of the sectors
		float leftSectorBorder = right;
		
		// position vertical lines
		for (int i = 0; i < VerticalSectorEdges.Count; i++)
		{
			// calculate the horizontal position of the current line (each line is more left than the previous)
			// we can figure out how long a sector is in our scale by dividing the sector length by the length of the cage
			// | to get the percentage of the cage a sector can cover (via length) (config.sectorLength / config.cageLength)
			// we then multiply this percent by the size of the cage in the x dimension in our units (right - left)
			// we then multiply this by the number of the line we are placing (+ 1 so that we are placing the left
			// | border of a sector instead of the right (i + 1)
			// we subtract our right border by this value to translate the line into the cage
			float newLeftPosition = right - ((config.sectorLength / config.cageLength) * (right - left) * (i + 1));
			
			if (config.sectorCountY == 0)
			{
				// if there are no sectors in the y dimension, disable all vertical lines
				VerticalSectorEdges[i].gameObject.SetActive(false);
			}
			else if (newLeftPosition < left)
			{
				// if the line would be left of the cage, disable it and set the left sector border to the left border
				VerticalSectorEdges[i].gameObject.SetActive(false);
				leftSectorBorder = left;
			}
			else
			{
				// activate the line
				VerticalSectorEdges[i].gameObject.SetActive(true);
				
				// translate the line to the calculated position
				VerticalSectorEdges[i].SetPosition(0, new Vector3(newLeftPosition, up, 1.1f));
				VerticalSectorEdges[i].SetPosition(1, new Vector3(newLeftPosition, down, 1.1f));
			
				// save the current leftmost sector coordinate
				leftSectorBorder = newLeftPosition;
			}
		}
		
		// decrease the amount of horizontal lines if there are more than sectors in the y dimension
		while (HorizontalSectorEdges.Count > config.sectorCountY)
		{
			Destroy(HorizontalSectorEdges[0].gameObject);	// destroy the first line
			HorizontalSectorEdges.Remove(HorizontalSectorEdges[0]);	// remove it from the list
		}
		
		// increase the amount of horizontal lines if there are less than sectors in the y dimension
		while (HorizontalSectorEdges.Count < config.sectorCountY)
		{
			GameObject lineObject = Instantiate(LineTemplate, transform);	// create a new line
			HorizontalSectorEdges.Add(lineObject.GetComponent<LineRenderer>() as LineRenderer);	// add it to the list
		}
		
		// place each horizontal line
		for (int i = 0; i < HorizontalSectorEdges.Count; i++)
		{
			// the vertical dimension of the line is calculated by starting with the width of the
			// cage (up - down) and dividing it by the amount of sectors in the y dimension.
			// this gives the size of a sector in the y dimension such that the sectors fit
			// perfectly between the bottom and top of the cage.
			// then we multiply this value by the number of the line we are placing ( +1 so that we are placing
			// a bottom line instead of a top line)
			// finally, we translate this coordinate to be in the cage by subtracting the top of the cage (up) by it
			float verticalDimension = up - (((up - down) / config.sectorCountY) * (i + 1));
			
			// place the line
			HorizontalSectorEdges[i].SetPosition(0, new Vector3(leftSectorBorder, verticalDimension, 1.1f));
			HorizontalSectorEdges[i].SetPosition(1, new Vector3(right, verticalDimension, 1.1f));
		}
		
		// draw a colored rectangle to represent the drone starting zone
		if (leftSectorBorder != left)
		{
			// activate the drone starting zone visual
			DroneStartBox.SetActive(true);
			
			// scale the zone to fit the left area and place it there
			DroneStartBox.transform.localScale = new Vector3(leftSectorBorder - left, up - down, 1);
			DroneStartBox.transform.localPosition = new Vector3((leftSectorBorder + left) / 2, (up + down) / 2, 1.25f);
		}
		else
		{
			// if the sectors reach the left border of the cage, deactivate the drone starting zone visual
			DroneStartBox.SetActive(false);
		}
		
		
		// display sector labels
		
		// if there are not enough labels for the number of sectors, make more
		while (SectorLabels.Count < config.sectorCountX * config.sectorCountY)
		{
			// make a new label and store its text component
			GameObject newLabel = Instantiate(LabelPrefab, LabelPrefab.transform.parent);
			SectorLabels.Add(newLabel.GetComponent<TMP_Text>());
		}
		
		// if there are too many labels for the number of sectors, delete some
		if (SectorLabels.Count > config.sectorCountX * config.sectorCountY)
		{
			// destroy the GameObjects of extra labels
			for (int i = (int)config.sectorCountX * (int)config.sectorCountY - 1; i < SectorLabels.Count - 1; i++)
			{
				Destroy(SectorLabels[i].gameObject);
			}
			
			// remove excess labels from the list
			SectorLabels.RemoveRange((int)config.sectorCountX * (int)config.sectorCountY - 1, SectorLabels.Count - (int)config.sectorCountX * (int)config.sectorCountY);
		}
		
		// calculate the label (A1, A2, ..., B1, B2, ..., AA1, AA2, ... AB1, AB2, ...)
		string text;
		for (int i = 0; i < SectorLabels.Count; i++)
		{
			// start with an empty string
			text = "";
			
			// calculate the column being evaluated
			int j = (i / (int)config.sectorCountX) + 1;
			
			// form string of A-Z characters
			while (j > 0)
			{
				// get the remainder of the division by number of letters in the alphabet to get the row as a char
				text = (char)('A' + ((j - 1) % 26)) + text;
				
				// divide by number of letters in alphabet to allow repeat
				j = (j - 1) / 26;
			}
			
			// add the column number to the string
			text += ((i % config.sectorCountX) + 1).ToString();
			
			// display the string
			SectorLabels[i].text = text;
		}
		
		// position the labels
		for (int j = 0; j < VerticalSectorEdges.Count; j++)
		{
			for (int k = 0; k < HorizontalSectorEdges.Count; k++)
			{
				// position a label in the center of col j and row k
				SectorLabels[j + k*VerticalSectorEdges.Count].transform.position = new Vector3(
					leftSectorBorder + ((right - leftSectorBorder) / config.sectorCountX) * (j + 0.5f), 
					up - ((up - down) / config.sectorCountY) * (k + 0.5f),
					0.5f);
			}
		}
	}
}
