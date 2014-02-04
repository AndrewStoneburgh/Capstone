using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public Texture2D icon;
	public int menuWidth = Screen.width/4;
	public int menuHeight = Screen.height/8;
	
	void OnGUI () {
		if(GUI.Button (new Rect (Screen.width/2 - menuWidth/2, Screen.height/2,menuWidth,menuHeight), new GUIContent ("Play", icon))){
			Application.LoadLevel(1);
		}
		if(GUI.Button (new Rect (Screen.width/2 - menuWidth/2, Screen.height/2 + menuHeight,menuWidth,menuHeight), new GUIContent ("Tutorial", icon))){
			print("I AINT GOT NO TUTORIAL YET");
		}
		if(GUI.Button (new Rect (Screen.width/2 - menuWidth/2, Screen.height/2 + 2*menuHeight,menuWidth,menuHeight), new GUIContent ("Quit", icon))){
			//quit
		}
	}
}
