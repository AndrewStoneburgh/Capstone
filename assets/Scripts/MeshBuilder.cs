using UnityEngine;
using System.Collections;

public class MeshBuilder{

	//Always square
	int _size;
	public float tileSize = 1.0f;

	public Mesh BuildMesh(int size){
		_size = size;
		tileSize = tileSize / _size;
		int numTiles = _size*_size;
		int numTris = numTiles*2;
		int numVerts = (_size + 1) * (_size + 1);
		//Terrainbuilder returns 2D array of floats with height data and requires the size in verts
		TerrainBuilder builder = new TerrainBuilder();
		float[,] heights = builder.BuildTerrain(_size + 1);

		// Generate the mesh data
		Vector3[] vertices = new Vector3[numVerts];
		Vector3[] normals = new Vector3[numVerts];
		Vector2[] uv = new Vector2[numVerts];
		int[] triangles = new int[ numTris * 3 ];

		int z,x;
		//Pass 1 - Generate Vectors
		for(z = 0; z < _size + 1; z++){
			for(x = 0; x < _size + 1; x++){
				vertices[z*(_size+1) + x] = new Vector3(x*tileSize, heights[x,z], z*tileSize);
				normals[z*(_size+1) + x] = new Vector3(0,1,0);
				uv[z*(_size+1) + x] = new Vector3((float)x/((float)_size), (float)z/((float)_size));
			}
		}
		//Pass 2 - Generate Triangles
		for(z = 0; z < _size; z++){
			for(x = 0; x < _size; x++){
				//Same as dealing with a pointer to a 2D array in c++
				int index = z*size + x;
				//6 verts per tri
				int offset = index*6;
				//First half - Counter-clockwise
				triangles[offset + 0] = z*(_size + 1) + x;
				triangles[offset + 1] = z*(_size + 1) + x + (_size + 1);
				triangles[offset + 2] = z*(_size + 1) + x + (_size + 1) + 1;

				//Second half
				triangles[offset + 3] = z*(_size + 1) + x;
				triangles[offset + 4] = z*(_size + 1) + x + 1 + (size + 1);
				triangles[offset + 5] = z*(_size + 1) + x + 1;
			}
		}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;

		return mesh;
	}
}
