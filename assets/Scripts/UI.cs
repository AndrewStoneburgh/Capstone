using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.guiText.guiText.text = ("CT(" + Map.focus.CT + ")"
		                             + " AttackRange(" + Map.focus.attackRange + ")"
		                             + " Damage(" + Map.focus.damage + ")"
		                             + " Health(" + Map.focus.health + ")");
	}
}
