using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {
	//The (x,y)[actually (x,z)] position of the tile. i.e. where it is in the 2D array
	public Vector2 index = Vector2.zero;
	//Boolean for tile passibility
	public bool isPassable = true;
	public bool inRange = false;
	//Agent currently on this tile.
	public Agent guest = null;
	public Vector3 center = Vector3.zero;
	public Map map;
	public Tile parent;
	public State state = State.Default;
	public int g, h, f;
	public int size;
	//Terrain type determines the properties of the terrain. 0 is default white
	public int terrainType = 0;

	public enum State{
		Open,
		Closed,
		Default,
		Impassible
	}


	// Use this for initialization
	void Start () {
		MeshBuilder mb = new MeshBuilder();
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
		MeshCollider mesh_collider = GetComponent<MeshCollider>();
		
		mesh_filter.mesh = mb.BuildMesh(size);
		mesh_collider.sharedMesh = mesh_filter.mesh;
	}
	
	// Update is called once per frame
	void Update () {

	}
	public void setColor(Color c){
		//gameObject.renderer.material.color = c;
		//terrain.renderer.material.color = c;
		GetComponent<MeshRenderer>().material.color = c;
	}
	void AddAgent(Agent a){
		guest = a;
	}
	void RemoveAgent(){
		guest = null;
	}
	void OnMouseEnter(){
		//gameObject.renderer.material.color = Color.green;
		//Debug.Log("( " + index.x + " , " + index.y + " )");
	}
	void OnMouseExit(){
		//gameObject.renderer.material.color = Color.white;
	}
	void OnMouseDown(){
		map.tileClicked(this);
	}
	public int dist(Tile t){
		int distance;
		distance = (int)(Mathf.Abs(t.index.x - index.x) + Mathf.Abs(t.index.y - index.y));
		return distance;
	}
}
