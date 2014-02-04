using UnityEngine;
using System.Collections;
using System;

public class TerrainBuilder{
	//Roughness constant
	public float h = 0.34f;
	//Seed values for corners
	//Unity appears to read the data in different.
	//TopRight is actually the bottom right. Ill sort this out later.
	public float bottomLeft = 0;
	public float bottomRight = 0;
	public float topLeft = 0;
	public float topRight = 0;
	System.Random rand;

	//Width in vertices. Must be (2^n)+1
	public float[,] BuildTerrain(int size){
		float[,] terrainData = new float[size,size];
		rand = new System.Random();
		float adjustment;//Calculated random based off of H(roughness constant)
		terrainData[0,0] = bottomLeft;
		terrainData[0,size-1] = topRight;
		terrainData[(size-1),(size-1)] = topLeft;
		terrainData[(size-1),0] = bottomRight;
		
		for(int squareSide = size - 1; squareSide >= 2; squareSide /= 2, h/= 2.0f){
			int halfSide = squareSide / 2;
			//Square Step
			for(int x = 0; x < size-1; x += squareSide){
				for(int  y = 0; y < size-1; y+= squareSide){
					float average;//average corners to calculate center
					average = terrainData[x,y] + terrainData[x,(y+squareSide)]
					+ terrainData[(x + squareSide),y] + terrainData[(x+squareSide),(y+squareSide)];
					average = average / 4;
					adjustment = (-h) + (h - (-h))*rand.Next(9999) / (10000);
					terrainData[(x+halfSide),(y+halfSide)] = average + adjustment;
				}
			}
			
			//Diamond Step
			for(int x = 0; x <= size-1; x+=halfSide){
				for(int y = (x+halfSide)%squareSide; y <= size-1; y+=squareSide){
					float average;//average corners to calculate center
					if(x == 0){
						average = 
							//no left value. Average 3 points instead
							terrainData[((x+halfSide)%size),y]	+	//right of center
								terrainData[x,((y+halfSide)%size)]	+	//below center
								terrainData[x,((y-halfSide+size)%size)];	//above center
						average = average / 3;
					}else if(y == 0){
						average = 
							//no right value
							terrainData[((x-halfSide+size)%size),y] +//left of center
								terrainData[((x+halfSide)%size),y]	+	//right of center
								terrainData[x,((y+halfSide)%size)];		//Below
						average = average / 3;
					}else if(x == size-1){
						average = 
							//no below
							terrainData[((x-halfSide+size)%size),y] +//left of center
								terrainData[x,((y+halfSide)%size)]	+	//below center
								terrainData[x,((y-halfSide+size)%size)];	//above center
						average = average / 3;
					}else if(y == size-1){
						average = 
							//no above
							terrainData[((x-halfSide+size)%size),y] +//left of center
								terrainData[((x+halfSide)%size),y]	+	//right of center
								terrainData[x,((y-halfSide+size)%size)];	//above center
						average = average / 3;
					}else{
						average = 
							terrainData[((x-halfSide+size)%size),y] +//left of center
								terrainData[((x+halfSide)%size),y]	+	//right of center
								terrainData[x,((y+halfSide)%size)]	+	//below center
								terrainData[x,((y-halfSide+size)%size)];	//above center
						average = average / 4;
					}
					
					adjustment = (-h) + (h - (-h))*rand.Next(9999) / (10000);
					terrainData[x,y] = average + adjustment;

					//For now, set all edges to 0 so they meet other tiles nicely
					if (x == (size - 1)) terrainData[(size - 1),y] = 0;
					if (y == (size - 1)) terrainData[x , (size - 1)] = 0;
					if (x == 0) terrainData[0 , y] = 0;
					if (y == 0) terrainData[x, 0] = 0;
				}
			}
		}
		return terrainData;
	}
}
