using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {
	public bool debugLogs = false;
	public Tile[,] tileMap;
	public int xSize = 10; // 10 is a default
	public int ySize = 10; // 10 is a default

	public bool ______________;

	// TODO - Sterilize inputs here
	public void SetMapSize(int x, int y) {
		xSize = x;
		ySize = y;
	}

	// Returns true if the location is in the map.   False otherwise.
	// NOTE: Does NOT check if it is a walkable square within generated map
	public bool IsWithinMapBounds(int x, int y) {
		return x >= 0 && x < xSize && y >= 0 && y < ySize;
	}

	public List<Tile> GetNeighbors(Tile tile) {
		List<Tile> neighbors = new List<Tile>();

		for (int i = -1; i <= 1; ++i) {
			for (int j = -1; j <= 1; ++j) {
				if (i == 0 && j == 0) { continue; } // No need to check the original tile

				int checkX = tile.location.x + i;
				int checkY = tile.location.y + j;

				if (IsWithinMapBounds(checkX, checkY)) {
					neighbors.Add(tileMap[checkX, checkY]);
				}
			}
		}
		return neighbors;
	}
}