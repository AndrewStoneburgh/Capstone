using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ant : MonoBehaviour {
	//A list of all possible terrains this ant can create.
	//Adding multiple instances of the same terrain increases its chances to appear
	public List<int> terrainTypes;


	// Use this for initialization
	void Start () {
		terrainTypes.Add(0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
