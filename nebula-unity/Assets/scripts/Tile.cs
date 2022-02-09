using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	public enum TileState {
		Door,
		Obstructed,
		Open,
		Ungenerated,
		Wall
	};

	public bool ________________;
	public Vector2Int location;
	public TileState curTileState = TileState.Open;

	// Constructors
	public Tile(Vector2Int loc, TileState state = TileState.Open) {
		location = loc;
		curTileState = state;
	}

	public void PrintTile() {
		Debug.Log("TileState: " + curTileState
			+ ", Location: (" + location.x + ", " + location.y + ")");
	}
}