using UnityEngine;
using System.Collections;

public class ComputerAgent : Agent
{
	public Agent target;
	//Basically need to throw A* in here
	public override bool Action (Tile t)
	{
		//map.Highlight(this);
		Tile tempTarget = map.tileList [(int)index.x, (int)index.y];
		tempTarget.inRange = true;
		foreach (Tile ti in map.tileList) {
				if (ti.inRange) {
						if (ti.dist (t) < tempTarget.dist (t) && ti.dist (t) > 0 && ti.guest == null) {
								tempTarget = ti;
						}
				}
		}

		map.tileList [(int)index.x, (int)index.y].guest = null;
		map.RemoveHighlight ();
		tempTarget.guest = this;
		index = tempTarget.index;
		gameObject.transform.position = new Vector3 (tempTarget.gameObject.transform.position.x, gameObject.transform.position.y, tempTarget.gameObject.transform.position.z);
		if (dist (t) == 1) {
			map.tileList[(int)t.index.x, (int)t.index.y].guest.health -= damage;
		}
		return true;
	}

	public override void Update()
	{	
		base.Update();
		//Hard-coding 17,17 as center value for now since 33 is the smallest vertex number and thats likely what Ill be using.
		//If I make it variable, Ill need to set this to width/2, height/2
		gameObject.transform.position = currentTile.center + new Vector3(0, currentTile.GetComponent<MeshFilter>().mesh.vertices[(currentTile.size/2)*currentTile.size + (currentTile.size/2)].y + 0.5f, 0);
		if (Map.focus == this) {
			gameObject.renderer.material.color = Color.blue;
		} else {
			gameObject.renderer.material.color = Color.red;
		}
		Debug.DrawLine (gameObject.transform.position, target.gameObject.transform.position, Color.red);
		//Wont work, dont know why
		//base.Update();
	}

	public int dist (Tile t)
	{
			int distance;
			distance = (int)(Mathf.Abs (t.index.x - index.x) + Mathf.Abs (t.index.y - index.y));
			return distance;
	}
}
