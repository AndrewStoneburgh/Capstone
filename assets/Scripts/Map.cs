using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour {
	public enum GameState{
		SelectAgent,
		ConfirmAction,
		SelectTarget,
		ActionInProgress,
		Menu,
		End
	}

	public GameObject tilePrefab;
	public GameObject tankPrefab;
	public GameObject healerPrefab;
	public GameObject damagePrefab;
	public GameObject cursorPrefab;


	public GameObject computerAgentPrefab;
	//The map is a 2D array of tiles. The 'bottom left' is at (0,0)
	//and will extend into the positive X and Z axes.
	public Tile[,] tileList;
	public List<Agent> agentList;
	//Agents with 100+ CT waiting to move
	public List<Agent> waitList;
	List<Agent> threatList;
	//Width and length of the map in tiles
	public int width;
	public int length;
	//TileSize is the scale of the tiles in unity units
	public float tileSize;
	private Vector3 prefabTileSize; 
	public GameState state = GameState.Menu;
	public static Agent focus;
	TerrainData terrainData;
	GameObject cursor;
	float[,] baseData;
	float[,] tweenData;
	float currentHeight = 1.0f;
	float direction = 1;
	bool rising = false;
	TerrainBuilder tb;
	public Camera leftCam;
	public Camera rightCam;
	float targetHeight = 1.05f;
	Tile pendingTarget;
	Vector3 confirmDialogue;

	//33 is the smallest size allowed
	int verts = 17;
	float angle = 0.4f;

	void Start () {
		rightCam.gameObject.SetActive(false);
		cursor = (GameObject)Instantiate(cursorPrefab);
		TerrainBuilder tb;
		tb = new TerrainBuilder();
		baseData = new float[verts,verts];
		tweenData = new float[verts, verts];
		baseData = tb.BuildTerrain(verts);
		terrainData = new TerrainData();
		//Resolution in pixels. Must be (2^n) +1
		//int resolution = verts;
		//Camera.main.orthographic = true;
		GenerateMap();
		agentList = new List<Agent>();
		GenerateAgents();
		focus = agentList.First();
		End();

	}

	void Update () {
		cursor.transform.Rotate(new Vector3(0, 0, angle));
		cursor.transform.position = new Vector3(focus.transform.position.x, cursor.transform.position.y, focus.transform.position.z);
		switch(focus.alignment){
		case 0: cursor.renderer.material.color = Color.gray;
			break;
		case 1: cursor.renderer.material.color = Color.green;
			break;
		case 2: cursor.renderer.material.color = Color.red;
			break;
		case 3: cursor.renderer.material.color = Color.blue;
			break;
		default:
			break;
		}
		//leftCam.transform.position = new Vector3(focus.transform.localPosition.x + 1, focus.transform.localPosition.y, focus.transform.localPosition.z); 
		leftCam.transform.rotation = focus.transform.rotation;
		leftCam.transform.position = focus.transform.position;
		leftCam.transform.Translate(new Vector3(0,targetHeight,0.5f), Space.Self);
		leftCam.transform.LookAt(focus.transform.position + new Vector3(0, targetHeight, 0));
		//Temp fix. Set tiles white. If state == selectaction, set red.
		if(Input.GetKey(KeyCode.UpArrow)){ GameObject.Find("Main Camera").transform.position = GameObject.Find("Main Camera").transform.position + new Vector3(0,0,.1f); };
		if(Input.GetKey(KeyCode.DownArrow)){ GameObject.Find("Main Camera").transform.position = GameObject.Find("Main Camera").transform.position + new Vector3(0,0,-.1f); };
		if(Input.GetKey(KeyCode.LeftArrow)){ GameObject.Find("Main Camera").transform.position = GameObject.Find("Main Camera").transform.position + new Vector3(-.1f,0,0); };
		if(Input.GetKey(KeyCode.RightArrow)){ GameObject.Find("Main Camera").transform.position = GameObject.Find("Main Camera").transform.position + new Vector3(.1f,0,0); };
		if(Input.GetKeyDown(KeyCode.R)){ rising = !rising;}
		switch(state)
		{
		case GameState.End:
			//These two commands need to run every time End() does, but are outside of it
			//so that End() can be called once at the start of the game without having anything
			//in the waitlist.
			waitList.First().CT -= 100;
			waitList.RemoveAt(0);
			End();
			break;
		case GameState.ConfirmAction:
			break;
		case GameState.ActionInProgress:
			break;
		case GameState.SelectTarget:
			if(waitList.First().hasActed && waitList.First().hasMoved){
				state = GameState.End;
			}
			break;
		case GameState.SelectAgent:
			if(waitList.First().hasActed && waitList.First().hasMoved){
				state = GameState.End;
			}
			break;
		default:
			break;
		}
	}
	void OnGUI(){
		if(state == GameState.ConfirmAction){
			if(GUI.Button(new Rect(confirmDialogue.x, confirmDialogue.y , 75, 25), "Confirm")){
				//True and false set to the same for now. Cancelling an action, confirming an action or failing should all return the player to agent selection
				if(focus.Action(pendingTarget)){
					state = GameState.SelectAgent;
				}else{
					state = GameState.SelectAgent;
				}
				foreach(Tile ti in tileList){
					ti.setColor(Color.white);
					ti.inRange = false;
				}
			}
			if(GUI.Button(new Rect(confirmDialogue.x, confirmDialogue.y +25 , 75, 25), "Cancel")){
				state = GameState.SelectAgent;
				foreach(Tile ti in tileList){
					ti.setColor(Color.white);
					ti.inRange = false;
				}
			}
		}
	}
	void TweenHeight(){
		if(rising){
			if(currentHeight >= 1 || currentHeight <= 0){
				direction *= -1;
			}
			currentHeight += 0.01f*direction;
			for(int i = 0; i < verts; i++){
				for(int j = 0; j < verts; j++){
					tweenData[i,j] = baseData[i,j];
					tweenData[i,j]*=currentHeight;
				}
			}
			terrainData.SetHeights(0,0,tweenData);
		}
	}
	void End(){
		if(waitList.Count < 1){
			while(waitList.Count() < 1){
				foreach(Agent a in agentList){
					a.CT += a.speed;
					if(a.CT >= 100){
						waitList.Add(a);
						a.hasActed = false;
						a.hasMoved = false;
					}
				}
			}
		}

		agentList.OrderBy(Agent => Agent.CT);
		agentList.Reverse();
		setFocus(waitList.First());
		if(waitList.First().name == "ComputerAgent(Clone)"){
			AIStep();
			foreach(Tile ti in tileList){
				ti.setColor(Color.white);
				ti.inRange = false;
			}
		}else{
			state = GameState.SelectAgent;
		}
	}
	void setFocus(Agent a){
		//RemoveHighlight();
		focus.gameObject.layer = 0;
		foreach(Transform t in focus.transform){
			t.gameObject.layer = 0;
		}
		focus = a;
		focus.gameObject.layer = 8;
		foreach(Transform t in focus.transform){
			t.gameObject.layer = 8;
		}
		//Highlight(focus);
		//Camera.main.transform.LookAt(focus.transform.position);
	}
	public void Highlight(Agent a, Color c, int range){
		ArrayList endList = new ArrayList();
		for(int i = 0; i < width; i++){
			for(int j = 0; j < length; j++){
				if(tileList[i,j].dist(tileList[(int)a.index.x, (int)a.index.y]) <= range
				   && tileList[i,j].guest == null
				   && tileList[i,j].isPassable){
					endList.Add(tileList[i,j]);
				}
			}
		}
		foreach(Tile t in endList){
			a.CalcPath(t);
		}
		foreach(Tile t in tileList){
			if(t.inRange){
				t.setColor(c);
			}
		}
	}
	public void RemoveHighlight(){
		focus.RemoveMovable();
		focus.abilityMenuAlive = false;
	}
	public void agentClicked(Agent a){
		tileClicked(tileList[(int)a.index.x, (int)a.index.y]);
	}
	public void tileClicked(Tile t){
		switch(state)
		{
		case GameState.End:
			End();
			break;
		case GameState.Menu:
			break;
		case GameState.ActionInProgress:
			break;
		case GameState.SelectAgent:
			if(t.guest != null){
				setFocus(t.guest);
				if(waitList.First() == t.guest){
					t.guest.abilityMenuAlive = !t.guest.abilityMenuAlive;
				}else{
					t.guest.infoMenuAlive = !t.guest.infoMenuAlive;
				}
			}
			break;
		case GameState.ConfirmAction:
			//Do nothing. Awaiting action selectiong.
			break;
		case GameState.SelectTarget:
			pendingTarget = t;
			confirmDialogue = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 0);
			state = GameState.ConfirmAction;
			break;
		default:
			break;
		}
	}
	void GenerateMap(){
		//If the width and height haven't been set, default them
		if(width < 5) width = 5;
		if(length < 5) length = 5;
		if(tileSize == 0) tileSize = 1;
		//Get the size of the prefab to scale to tileSize
		prefabTileSize = tilePrefab.transform.localScale;
		//Allocate memory for the array we need
		tileList =  new Tile[width,length];
		for(int ww = 0; ww < width; ww++){
			for(int hh = 0; hh < length; hh++){
				Tile t = ((GameObject)Instantiate(tilePrefab)).GetComponent<Tile>();
				t.index = new Vector2(ww,hh);
				t.transform.position = new Vector3(ww*tileSize + tileSize/2, 0, hh*tileSize + tileSize/2);
				t.transform.localScale = new Vector3(tileSize*prefabTileSize.x, tileSize*prefabTileSize.y, tileSize*prefabTileSize.x);
				t.center = t.transform.position + new Vector3((tileSize*prefabTileSize.x)/2, 0, (tileSize*prefabTileSize.z)/2);
				t.map = this;
				tileList[ww,hh] = t;
			}
		}
	}
	void AIStep(){
		//The below method grabs the first non-allied agent and sets it as the highest threat target.
		//It then does a sort to see if any other non-allied agents have higher threat.
		Agent highestThreat = null;
		while(highestThreat == null){
			foreach(Agent a in agentList){
				if(a.alignment != focus.alignment){
					highestThreat = a;
				}
			}
		}
		foreach(Agent a in agentList){
			if(a.threat > highestThreat.threat && highestThreat.alignment != focus.alignment){
				highestThreat = a;
			}
		}
		Highlight(focus, Color.red, focus.range);
		((ComputerAgent)focus).target = highestThreat;
		if(focus.Action(tileList[(int)highestThreat.index.x, (int)highestThreat.index.y])){
			state = GameState.End;
		}
	}
	public void GenerateAgents(){
		for(int ii = 0; ii < 4; ii++){
			Tank a1 = ((GameObject)Instantiate(tankPrefab)).GetComponent<Tank>();
			a1.index = new Vector2(ii, 0);
			//a1.transform.position = tileList[ii, 0].transform.position + new Vector3(0,tankPrefab.transform.position.y,0);
			a1.transform.position = tileList[ii, 0].center;
			tileList[ii, 0].guest = a1;
			agentList.Add(a1);
			a1.alignment = 1;
			a1.map = this;
		}
		for(int ii = 0; ii < 4; ii++){
			ComputerAgent a2 = ((GameObject)Instantiate(computerAgentPrefab)).GetComponent<ComputerAgent>();
			a2.index = new Vector2(ii,7);
			tileList[ii,7].guest = a2;
			//a2.transform.position = tileList[ii,7].transform.position + new Vector3(0,computerAgentPrefab.transform.localScale.y,0);
			//tileList[ii*2,14].guest = a2;
			//a2.transform.position = tileList[ii, 7].center.x - new Vector3(0, 0.0f, 0);
			a2.transform.position = new Vector3(tileList[ii, 7].center.x, a2.transform.position.y, tileList[ii, 7].center.z);
			agentList.Add(a2);
			a2.transform.Rotate(new Vector3(0,180,0));
			a2.alignment = 2;
			a2.target = agentList.First();
			a2.map = this;
		}

		//Special Parameters: In place of having distinct units, setting parameters here will alter the defaults
		//Mock ranger
		agentList.ElementAt(0).speed = 11;
		agentList.ElementAt(0).attackRange = 5;
		agentList.ElementAt(0).damage = 25;
		agentList.ElementAt(0).health = 90;
		//Mock Rogue
		agentList.ElementAt(1).speed = 10;
		agentList.ElementAt(1).range = 7;
		agentList.ElementAt(1).damage = 20;
		agentList.ElementAt(1).health = 100;
		//Mock Tank
		agentList.ElementAt(2).speed = 9;
		agentList.ElementAt(2).range = 4;
		agentList.ElementAt(2).damage = 40;
		agentList.ElementAt(2).health = 150;
		//Mock healer
		agentList.ElementAt(3).speed = 11;
		agentList.ElementAt(3).range = 5;
		agentList.ElementAt(3).attackRange = 5;
		agentList.ElementAt(3).damage = -100;
		agentList.ElementAt(3).health = 70;
		//Enemy speeds
		agentList.ElementAt(4).speed = 7;
		agentList.ElementAt(5).speed = 10;
		agentList.ElementAt(6).speed = 10;
		agentList.ElementAt(7).speed = 8;
	}	
}
