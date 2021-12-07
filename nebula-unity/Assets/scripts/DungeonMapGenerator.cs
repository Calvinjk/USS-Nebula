using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All generated rooms will be an odd number of tiles in at least one dimension to allow for a central door
public class DungeonMapGenerator : MonoBehaviour {
	public GameObject floorTile;	// Prefab of a floor tile model to generate.  

	// Generation variables to mess with
	[Tooltip("Toggles debug logs for map generation")]
	public bool generationDebugLogs = true;

	[Header("Generation Variables")]
	[Tooltip("Smallest dimension a room can have.  Includes walls")]
	public int minRoomDiameter = 3;
	[Tooltip("Largest dimension a room can have.  Includes walls")]
	public int maxRoomDiameter = 20;

	[Tooltip("Number of failed attempts to place a major feature until generator believes it is done")]
	public int maxAttempts = 100;

	public bool _____________________;

	public const int XDIMDEFAULT = 100;  // Default size of the dungeon floor in the X direction
	public const int YDIMDEFAULT = 100;  // Default size of the dungeon floor in the Z direction

	public int mapXSize;
	public int mapYSize;

	public int curRoomFailures = 0;  // How many times the algorithm has currently failed to place a room
	List<Tile> potentialDoorTiles;

	Tile[,] tiles;

	//.N.E.
	//..O..
	//.W.S.
	enum Direction {
		North,	// +z(y)
		South,  // -z(y)
		East, 	// +x
		West	// -x
	}

	void Awake() {
		potentialDoorTiles = new List<Tile> ();
	}

	void ResetGenerationVariables() {
		curRoomFailures = 0;
		if (potentialDoorTiles != null) {
			potentialDoorTiles.Clear();	
		}
	}

	// This function will generate a map given some bounding dimensions
	public Map GenerateMap(int xSize = XDIMDEFAULT, int ySize = YDIMDEFAULT){
		ResetGenerationVariables ();

		mapXSize = xSize;
		mapYSize = ySize;

		// Create an empty parent object for the map
		GameObject map = new GameObject("Map");	
		map.transform.position = new Vector3(0f, 0f, 0f);
		Map mapScript = map.AddComponent<Map> ();
		mapScript.SetMapSize (xSize, ySize);

		tiles = new Tile[xSize, ySize];

		for (int i = 0; i < xSize; ++i) {
			for (int j = 0; j < ySize; ++j) {

				// Create a tile and give it a name based on its location
				GameObject curTileObject = Instantiate (floorTile, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
				curTileObject.name = "(" + i + ", " + j + ")";

				// When instantiating a Tile, attach a Tile script to it and set variables
				Tile tileScript = curTileObject.AddComponent<Tile>();
				tileScript.location = new Vector2Int (i, j);
				SetUngenerated (tileScript);

				// Add the generated tile to the tiles array and set the object's parent to the map GameObject
				tiles[i, j] = tileScript;
				curTileObject.transform.SetParent(map.transform);
			}
		} 

		// Now that all the tiles are created, assign them
		mapScript.tileMap = tiles;

		// Create the initial room in the center (ish)
		int xLength = Random.Range(minRoomDiameter, maxRoomDiameter + 1) / 2;
		int yLength = Random.Range (minRoomDiameter, maxRoomDiameter + 1);

		int xLocation = (xSize / 2) - (xLength / 2);
		int yLocation = (ySize / 2) - (yLength / 2);

		if (generationDebugLogs){ 
			Debug.Log ("Placing starting room at (" + xLocation + "," + yLocation + ")...\n" +
				"Dimensions of " + (xLength * 2 + 1) + "x" + yLength + "...\n" +
				"Direction: " + Direction.North);
		}

		GenerateRoom (xLocation, yLocation, xLength, yLength, Direction.North);

		// Keep going!
		while (curRoomFailures < maxAttempts){
			// Re-randomize the size variables
			xLength = Random.Range(minRoomDiameter, maxRoomDiameter + 1) / 2;
			yLength = Random.Range(minRoomDiameter, maxRoomDiameter + 1);

			// Try to make a new room!
			if (potentialDoorTiles.Count == 0) {
				Debug.LogError ("Somehow ran out of valid wall tiles to create a door on");
			} else {
				int index = Random.Range (0, potentialDoorTiles.Count);

				Tile curTile = potentialDoorTiles [index]; 
				potentialDoorTiles.RemoveAt (index);

				// Determine which direction we should go in (There should only be one)
				Direction direction = GetRoomDirection(curTile);

				GenerateRoom (curTile.location.x, curTile.location.y, xLength, yLength, direction);
			}
		}
			
		return mapScript;
	}

	// xLocation, yLocation: 	Coordinates to a door of this room.
	// xLength: 				How far perpendicular to the inner door face the room extends from the door tile
	// yLength:					How far from the door the room extends in depth towards Direction
	// Direction:				In what direction the room extends
	void GenerateRoom(int xLocation, int yLocation, int xLength, int yLength, Direction direction){
		if (generationDebugLogs){ 
			Debug.Log ("Attempting to create room at (" + xLocation + "," + yLocation + ")...\n" +
				"Dimensions of " + (xLength * 2 + 1) + "x" + yLength + "...\n" +
				"Direction: " + direction);
		}

		// Check if we can fit a room here
		if (CheckTiles (xLocation, yLocation, xLength, yLength, direction)) {
			// Good, lets generate it!
			for (int i = -xLength; i <= xLength; ++i) {
				for (int j = 0; j < yLength; ++j) {
					Tile curTile = null;

					// A door is a door regardless of the direction
					if (i == 0 && j == 0) {
						curTile = tiles [xLocation, yLocation];

						// The previous state of the tile should be a wall, but now its a door!
						if (curTile.curTileState == Tile.TileState.Wall) {
							SetDoor (curTile);
						} else {
							// This should be the first room.
							if (generationDebugLogs){
								Debug.Log ("Creating first room starting point");
							}
							SetWall (curTile);
						}
					} else {
						switch (direction) {
						case Direction.North:
							curTile = tiles [xLocation + i, yLocation + j];
							break;
						case Direction.South:
							curTile = tiles [xLocation + i, yLocation - j];
							break;
						case Direction.East:
							curTile = tiles [xLocation + j, yLocation + i];
							break;
						case Direction.West:
							curTile = tiles [xLocation - j, yLocation + i];
							break;
						}

						if (Mathf.Abs (i) == xLength || j == 0 || j == (yLength - 1)) {
							SetWall (curTile);

							// If this wall tile is NOT a corner, add it to potential list of new doors
							if (!IsCorner(i, j, xLength, yLength)){
								potentialDoorTiles.Add (curTile);
							}
						} else {
							SetFloor (curTile);
						}
					}
				}
			}
		} else {
			++curRoomFailures;
		}
	}

	bool CheckTiles(int xLocation, int yLocation, int xLength, int yLength, Direction direction){
		if (generationDebugLogs) { Debug.Log ("Checking attempted room placement..."); }

		/*
		This code will split for each of the 4 directions.  Therefore, in order to not duplicate
		each of my comments 4 times, I will write the general logic up here and only comment the north direction:

		1: 		Iterate through each tile that would be involved in the room we are making, walls included.
		2:		If our current tile is off the map, return false.
		3: 		If even one tile that we check is NOT a ungenerated tile, return false as it overlaps with an existing room
		3.1: 	If we find an existing wall tile in a place we WOULD put a wall tile, allow it as we are ok with "shared" walls
		3.2:	If we find an existing wall tile in a place we WOULD NOT put a wall, return false
		4: 		If the loop makes it all the way through without returning false it has checked every tile!
		*/
		for (int i = -xLength; i <= xLength; ++i) {
			for (int j = 0; j < yLength; ++j) {
				Tile curTile = null;

				switch (direction) {
				case Direction.North:
					// Check if this tile is on the map
					if (IsInMapBoundaries(xLocation + i, yLocation + j)) {
						curTile = tiles [xLocation + i, yLocation + j];
					} else {
						// This room goes past the edge of the map!
						if (generationDebugLogs) {
							Debug.Log ("Result: False (Room would extend past map boundaries)");
						}
						return false;
					}
					break;
				case Direction.South:
					if (IsInMapBoundaries(xLocation + i, yLocation - j)) {
						curTile = tiles [xLocation + i, yLocation - j];
					} else {
						if (generationDebugLogs) {
							Debug.Log ("Result: False (Room would extend past map boundaries)");
						}
						return false;
					}
					break;
				case Direction.East:
					if (IsInMapBoundaries(xLocation + j, yLocation + i)) {
						curTile = tiles [xLocation + j, yLocation + i];
					} else {
						if (generationDebugLogs) {
							Debug.Log ("Result: False (Room would extend past map boundaries)");
						}
						return false;
					}
					break;

				case Direction.West:
					if (IsInMapBoundaries(xLocation - j, yLocation + i)) {
						curTile = tiles [xLocation - j, yLocation + i];
					} else {
						if (generationDebugLogs) {
							Debug.Log ("Result: False (Room would extend past map boundaries)");
						}
						return false;
					}
					break;
				}

				// Check if this tile is ungenerated
				if (curTile.curTileState != Tile.TileState.Ungenerated) {
					// If this tile as already been generated, check if it is a wall
					if (curTile.curTileState != Tile.TileState.Wall) {

						// If the tile is not a wall, we have overlap and should return false
						if (generationDebugLogs) {
							Debug.Log ("Result: False...Overlap at (" + (curTile.location.x) + "," + (curTile.location.y) + ")");
						}
						return false;
					} else {
						// If we have reached this point, we found a wall.  Now check if our current room would also put a wall there
						if (Mathf.Abs (i) == xLength || j == 0 || j == (yLength - 1)) {
							// Cool, this would also be a wall!  Allow it!
						} else {
							// Bad.  We wouldnt put a wall here.  That means something will be overlapping!
							if (generationDebugLogs) {
								Debug.Log ("Result: False...Overlap at (" + (curTile.location.x) + "," + (curTile.location.y) + ")");
							}
							return false;
						}
					}
				}
			}
		}

		// If we made it here without returning false, we didn't find any conflicts!
		if (generationDebugLogs) { Debug.Log ("Result: True"); }
		return true;
	}

	// No need to worry about North bias for this function as there should only be one valid room direction.
	// Therefore, the first one we find is the only valid one
	Direction GetRoomDirection(Tile door){
		if (IsInMapBoundaries (door.location.x, door.location.y + 1) &&
		    tiles [door.location.x, door.location.y + 1].curTileState == Tile.TileState.Ungenerated) {
			return Direction.North;
		} else if (IsInMapBoundaries (door.location.x, door.location.y - 1) &&
		           tiles [door.location.x, door.location.y - 1].curTileState == Tile.TileState.Ungenerated) { 
			return Direction.South;
		} else if (IsInMapBoundaries (door.location.x + 1, door.location.y) &&
		           tiles [door.location.x + 1, door.location.y].curTileState == Tile.TileState.Ungenerated) {
			return Direction.East;
		} else if (IsInMapBoundaries (door.location.x - 1, door.location.y) &&
		           tiles [door.location.x - 1, door.location.y].curTileState == Tile.TileState.Ungenerated) {
			return Direction.West;
		}
		if (generationDebugLogs) {
			Debug.Log ("No valid direction to place room");
		}
		return Direction.North;
	}

	bool IsInMapBoundaries(int x, int y){
		return (y >= 0 && y < mapXSize && x >= 0 && x < mapYSize);
	}

	bool IsCorner(int i, int j, int xLength, int yLength){
		return ((i == -xLength && j == 0)
		|| (i == -xLength && j == (yLength - 1))
		|| (i == xLength && j == 0)
		|| (i == xLength && j == (yLength - 1)));
	}

	// All of the functions below are currently placeholders.  These are where we will put the logic to actually place sprites
	// and models when we have them.  For now they are just differently-colored squared

	void SetWall(Tile tile){
		tile.curTileState = Tile.TileState.Wall;
		tile.gameObject.GetComponent<Renderer> ().material.color = Color.grey;
		tile.transform.localScale = new Vector3 (tile.transform.localScale.x, 2f, tile.transform.localScale.z);
	}

	void SetFloor(Tile tile){
		tile.curTileState = Tile.TileState.Open;
		tile.gameObject.GetComponent<Renderer> ().material.color = Color.white;
	}

	void SetDoor(Tile tile){
		tile.curTileState = Tile.TileState.Door;
		tile.gameObject.GetComponent<Renderer> ().material.color = Color.magenta;
		tile.transform.localScale = new Vector3 (tile.transform.localScale.x, 2.5f, tile.transform.localScale.z);
	}

	void SetUngenerated(Tile tile){
		tile.curTileState = Tile.TileState.Ungenerated;
		tile.gameObject.GetComponent<Renderer> ().material.color = Color.yellow;
	}
}