using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour
{
	//Where the agent is on the map
	public Vector2 index = Vector2.zero;
	public int speed = 10;
	public int CT = 0;
	public int range = 4;
	public int health = 100;
	public int damage = 15;
	public int threat = 0;
	public int attackRange = 1;
	public int threatMultiplier = 1;
	public bool hasMoved = false;
	public bool hasActed = false;
	//Which team is this character a member of?
	//0 - neutral
	//1 - team 1 (default player team)
	//2 - team 2 (default enemy team)
	//3 - team 3, etc
	public int alignment = 0;
	public Map map;
	//An integer to represent the current action chosen from a list. 
	public int currentChoice = 0;
	//A* stuff
	public ArrayList openList;
	public ArrayList closedList;
	public ArrayList endList;
	public Tile start;
	public Tile end;
	public Tile currentTile;
	private int m_x, m_y;
	public bool abilityMenuAlive = false;
	public bool infoMenuAlive = false;
	Vector3 menuPos;
	private int buttonHeight = 25;

	// Use this for initialization
	void Start ()
	{
		menuPos = Vector3.zero;
	}

	//Change colours to red and green in subclasses
	public virtual void Update ()
	{
		currentTile = map.tileList[(int)index.x, (int)index.y];
		//Hard-coding 17,17 as center value for now since 33 is the smallest vertex number and thats likely what Ill be using.
		//If I make it variable, Ill need to set this to width/2, height/2
		gameObject.transform.position = currentTile.center + new Vector3(0, currentTile.GetComponent<MeshFilter>().mesh.vertices[(currentTile.size/2)*currentTile.size + (currentTile.size/2)].y, 0);
		if (health <= 0) {
				map.agentList.Remove (this);
				if (map.waitList.Contains (this)) {
						map.waitList.Remove (this);
				}
				Destroy (gameObject);
		}
	}

	public virtual bool Action (Tile t)
	{
		abilityMenuAlive = false;
		//Actions need default threat values
		switch (currentChoice) {
		case 0:
		//Player default. If this ever triggers I messed up.
			return false;
		case 1:
		//If target is within range
			if (t.dist(currentTile) <= range && t.dist(currentTile) != 0 && (t.guest == null || t.guest == this) && hasMoved == false) {
				map.tileList [(int)index.x, (int)index.y].guest = null;
				map.RemoveHighlight ();
				t.guest = this;
				index = t.index;
				gameObject.transform.position = new Vector3 (t.gameObject.transform.position.x, gameObject.transform.position.y, t.gameObject.transform.position.z);
				currentChoice = 0;
				hasMoved = true;
				return true;
				} else {
					return false;
				}
		case 2:
			//You may attack an empty square, but the actions dont trigger(no threat increase)
			if (t.dist(currentTile) <= attackRange && t.dist (map.tileList [(int)index.x, (int)index.y]) > 0 && hasActed == false
			    && t.guest != null) {
				map.tileList [(int)t.index.x, (int)t.index.y].guest.health -= damage;
				threat += damage * threatMultiplier;
				}
			hasActed = true;
			return true;
		case 3:
			hasActed = true;
			threatMultiplier = 3;
			return true;
		case 4:
		//This will trigger a turn end. Should eventually be changed so that ending a turn without doing both actions saves CT
			hasActed = true;
			hasMoved = true;
			map.state = Map.GameState.End;
			return true;
		case 5:
			return false;
		default:
			return true;
		}
	}

	public virtual void OnMouseDown ()
	{
		map.agentClicked (this);
		menuPos = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, Input.mousePosition.z);
		if(map.waitList.First() == this){
			abilityMenuAlive = !abilityMenuAlive;
		}else{
			infoMenuAlive = !infoMenuAlive;
		}
	}
	public virtual void OnGUI(){
		if(abilityMenuAlive){
			//5 is default number of actions. This number will be dynamic later
			//The extra number is the header
			int boxHeight = buttonHeight*6;
			int boxWidth = Screen.width / 7;
			//Add in the other three bounds
			if(menuPos.y + boxHeight > Screen.height){
				menuPos.y = boxHeight;
			}
			BuildMenu(menuPos.x, menuPos.y, boxWidth, boxHeight);
		}
	}
	void BuildMenu(float x, float y, int width, int height){
		GUI.Box(new Rect(menuPos.x, menuPos.y, width, height), name);
		if(GUI.Button(new Rect(menuPos.x + 5, menuPos.y + buttonHeight, width - 10, buttonHeight), "Move")
		   && hasMoved == false){
			currentChoice = 1;
			map.state = Map.GameState.SelectTarget;
			map.Highlight(this, Color.blue, range);
			abilityMenuAlive = false;
		}
		if(GUI.Button(new Rect(menuPos.x + 5, menuPos.y + 2*buttonHeight, width - 10, buttonHeight), "Attack")
		   && hasActed == false){
			currentChoice = 2;
			map.state = Map.GameState.SelectTarget;
			map.Highlight(this, Color.red, attackRange);
			abilityMenuAlive = false;
		}
		if(GUI.Button(new Rect(menuPos.x + 5, menuPos.y + 3*buttonHeight, width - 10, buttonHeight), "Special")
		   && hasActed == false){
			currentChoice = 3;
			map.state = Map.GameState.SelectTarget;
			abilityMenuAlive = false;
		}
		if(GUI.Button(new Rect(menuPos.x + 5, menuPos.y + 4*buttonHeight, width - 10, buttonHeight), "End Turn")){
			currentChoice = 4;
			//I tried to keep this code to just modify choice but for end turn we need to circumvent the target select option.
			//Once I have a 'confirm choice' enabled, we can jump directly there, probably
			Action(currentTile);
			map.state = Map.GameState.SelectTarget;
			abilityMenuAlive = false;
		}
		if(GUI.Button(new Rect(menuPos.x + 5, menuPos.y + 5*buttonHeight, width - 10, buttonHeight), "Cancel")){
			currentChoice = 5;
			map.state = Map.GameState.SelectTarget;
			abilityMenuAlive = false;
		}
	}
	//Add tiles around a given tile to open list
	void addSurrounding (int x, int y)
	{
				//Left
				if (x > 0) {
						if (map.tileList [x - 1, y].isPassable && map.tileList [x - 1, y].guest == null
								&& openList.Contains (map.tileList [x - 1, y]) == false
								&& closedList.Contains (map.tileList [x - 1, y]) == false) {
								openList.Add (map.tileList [x - 1, y]);
						}
				}
				//Right
				if (x < map.width - 1) {
						if (map.tileList [x + 1, y].isPassable && map.tileList [x + 1, y].guest == null
								&& openList.Contains (map.tileList [x + 1, y]) == false
								&& closedList.Contains (map.tileList [x + 1, y]) == false) {
								openList.Add (map.tileList [x + 1, y]);
						}
				}
				//Above
				if (y < map.length - 1) {
						if (map.tileList [x, y + 1].isPassable && map.tileList [x, y + 1].guest == null
					  	  		&& openList.Contains (map.tileList [x, y + 1]) == false
					   			&& closedList.Contains (map.tileList [x, y + 1]) == false) {
								openList.Add (map.tileList [x, y + 1]);
					}
				}
				//Below
				if (y > 0) {
						if (map.tileList [x, y - 1].isPassable && map.tileList [x, y - 1].guest == null
								&& openList.Contains (map.tileList [x, y - 1]) == false
								&& closedList.Contains (map.tileList [x, y - 1]) == false) {
								openList.Add (map.tileList [x, y - 1]);
						}
				}
	}

	public void RemoveMovable ()
	{
		for (int i = 0; i < this.map.width; i++) {
			for (int j = 0; j < this.map.length; j++) {
				this.map.tileList [i, j].inRange = false;
			}
		}
	}
	//Create endList later and call calcPath on each one to determine if it is inrange
	public void CalcPath (Tile t)
	{
		openList = new ArrayList ();
		closedList = new ArrayList ();
		//Iterate over list and add all tiles that may be in range and are passable
		for (int i = 0; i < this.map.width; i++) {
			for (int j = 0; j < this.map.length; j++) {
				this.map.tileList[i, j].g = 0;
				this.map.tileList[i, j].f = 0;
				this.map.tileList[i, j].h = 0;
				this.map.tileList[i, j].state = Tile.State.Default;
				this.map.tileList[i, j].parent = null;
			}
		}
		start = this.map.tileList[(int)this.index.x, (int)this.index.y];
		end = t;
		openList.Add(start);

		m_x = (int)start.index.x;
		m_y = (int)start.index.y;
		while(this.map.tileList[m_x, m_y] != end) {
			addSurrounding(m_x, m_y);
			foreach(Tile ti in openList){
				if(ti.state != Tile.State.Open && ti != start && ti.isPassable){
					ti.state = Tile.State.Open;
					ti.parent = this.map.tileList[m_x, m_y];
				}
			}
			openList.Remove(this.map.tileList[m_x, m_y]);
			closedList.Add(this.map.tileList[m_x, m_y]);
			//Calculate F, G and H
			foreach(Tile ti in openList){
				ti.g = ti.parent.g + 10;
				ti.h = 10*(ti.dist(end));
				ti.f = ti.g + ti.h;
			}
			if(openList.Count == 0){
				break;
			}
			//Select lowest F value
			Tile lowestF = openList[0] as Tile;
			foreach(Tile ti in openList){
				if(ti.f < lowestF.f){
					lowestF = ti;
				}
			}
			//Removed this chunk of code. I dont think this heuristic works without
			//diagonal travel
			/*//Prioritize nodes closer to the end
			foreach(Tile ti in openList){
				if(ti.f == lowestF.f){
					if(lowestF.h >= ti.h){
						lowestF = ti;
					}				
				}
			}*/
			//Set parent node
			//lowestF.parent = CreateMap.floor[m_x,m_y].GetComponent<Node>();
			m_x = (int)lowestF.index.x;
			m_y = (int)lowestF.index.y;
		}
		//Go back through parents and set them to open
		Tile temp = end;
		int dist = 0;

		while(temp != start){
			if(temp == start){
				openList.Clear ();
				closedList.Clear ();
				temp.inRange = true;
				break;
			}
			dist++;
			temp.state = Tile.State.Closed;
			temp = temp.parent;
		}
		if(dist <= this.range){
			t.inRange = true;
		}
		openList.Clear ();
		closedList.Clear ();
	}
}
