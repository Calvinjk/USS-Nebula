using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	public enum TileType {
		Door,
		Floor,
		Ungenerated,
		Wall
	};

	public bool ________________;
	public Vector2Int location;
	public TileType curTileType = TileType.Floor;

    //////////////////////// Constructors
    public Tile() {
        curTileType = TileType.Ungenerated;
    }
	public Tile(Vector2Int loc, TileType state = TileType.Floor) {
		location = loc;
		curTileType = state;
	}

    ////////////////////////

    public void PrintTile() {
		Debug.Log("TileType: " + curTileType
			+ ", Location: (" + location.x + ", " + location.y + ")");
	}
}