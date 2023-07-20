using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// struct for representing the values needed to digitally reconstruct a drone cage
public struct CageConfiguration
{
	public string configName;	// an identifying name for the drone cage
	public float cageWidth;		// the width of the drone cage
	public float cageLength;	// the length of the drone cage
	public float sectorLength;	// the length of a sector
	public uint sectorCountX;	// the number of sectors in the X dimension
	public uint sectorCountY;	// the number of sectors in the y dimension
};

// the attributes to be saved persistantly
[JsonObject]
public class SaveState
{
	// the prefabricated configurations for the drone cage
	public List<CageConfiguration> ConfigPrefabs = new List<CageConfiguration>();
}
