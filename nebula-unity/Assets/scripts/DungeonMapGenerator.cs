using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// All generated rooms will be an odd number of tiles in at least one dimension to allow for a central door
public class DungeonMapGenerator : MonoBehaviour {
    [Header("Physical prefabs for generation objects")]
    public GameObject floorTile;    // Prefab of a floor tile model to generate.  

    // Generation variables to mess with
    [Header("Generation Variables")]
    [Tooltip("Toggles debug logs for map generation")]
    public bool generationDebugLogs = false;

    [Tooltip("Smallest dimension a room can have.  Includes walls")]
    public int minRoomDiameter = 3;
    [Tooltip("Largest dimension a room can have.  Includes walls")]
    public int maxRoomDiameter = 20;

    [Range(0.0f, 10.0f)]
    [Tooltip("Larger the number, more likely that walls with more open space are chosen as doors")]
    public float shapeFactor = 1f;

    [Tooltip("Number of failed attempts to place a major feature until generator believes it is done")]
    public int maxAttempts = 100;

    public bool _____________________;

    private const int XDIMDEFAULT = 100;  // Default size of the dungeon floor in the X direction
    private const int YDIMDEFAULT = 100;  // Default size of the dungeon floor in the Z direction

    private Vector2Int mapSize;        // Given map maximum allowed boundaries
    private Vector2Int curMaxBounds = new Vector2Int(int.MinValue, int.MinValue);   // Current maximum generated bounds (x,y)
    private Vector2Int curMinBounds = new Vector2Int(int.MaxValue, int.MaxValue);   // Current minimum generated bounds (X,y)

    private int curRoomFailures = 0;  // How many times the algorithm has currently failed to place a room

    Tile[,] tiles;
    GameObject map;

    public enum Direction {
        North,  // +z(y)
        South,  // -z(y)
        East,   // +x
        West,   // -x
        INVALID // Sentinel value
    }

    public class PotentialDoorsList {
        float weightPower = 1f;
        float totalWeight = 0f;
        List<PotentialDoor> potentialDoors;

        public PotentialDoorsList() {
            potentialDoors = new List<PotentialDoor>();
            totalWeight = 0;
            weightPower = 1.0f;
        }

        public PotentialDoorsList(float shapeFactor) {
            potentialDoors = new List<PotentialDoor>();
            totalWeight = 0;
            weightPower = shapeFactor;
        }

        public void AddPotentialDoor(PotentialDoor p) {
            potentialDoors.Add(p);
            totalWeight += Mathf.Pow(p.distance, weightPower);
        }

        public PotentialDoor SelectPotentialDoor() {
            // If weightPower == 0, weights don't matter so we should skip all the logic and iteration and just return a random element.
            if (weightPower == 0) { return potentialDoors[Random.Range(0, potentialDoors.Count)]; }

            // Select a random number from 0-1, then multiply it by the total potential door weight to normalize it
            float selection = Random.Range(0f, 1f) * totalWeight;
            float curWeight = 0f;

            // For each item, add to our cumulative probability function and check if we hit our selection or not
            foreach (PotentialDoor p in potentialDoors) {
                curWeight += Mathf.Pow(p.distance, weightPower);
                if (curWeight >= selection) { return p; }
            }
            return null; // This line will ONLY be reached if the list of potential doors is empty
        }
    }

    public class PotentialDoor {
        public Tile tile;
        public int distance;
        public Direction direction;

        public PotentialDoor(Tile t, int dis, Direction dir) {
            tile = t;
            distance = dis;
            direction = dir;
        }
    };

    void Awake() {
        map = null;
    }

    void ResetGenerationVariables() {
        curRoomFailures = 0;
        curMaxBounds = new Vector2Int(int.MinValue, int.MinValue);
        curMinBounds = new Vector2Int(int.MaxValue, int.MaxValue);
    }

    // This function will generate a map given some bounding dimensions
    public Map GenerateMap(int xSize = -1, int ySize = -1, int minDiam = -1, int maxDiam = -1, float sfact = -1f, int maxatt = -1) {

        ResetGenerationVariables();

        ReplaceSentinels(ref xSize, ref ySize, ref minDiam, ref maxDiam, ref sfact, ref maxatt);

        // Create an empty parent object for the map
        map = new GameObject("Map");
        map.transform.position = new Vector3(0f, 0f, 0f);
        Map mapScript = map.AddComponent<Map>();
        mapScript.SetMapSize(xSize, ySize);
        mapSize = new Vector2Int(xSize, ySize);

        tiles = new Tile[xSize, ySize];

        // Create the initial room in the center (ish)
        int xLength = Random.Range(minDiam, minDiam + 1) / 2;
        int yLength = Random.Range(maxDiam, maxDiam + 1);

        int xLocation = (xSize / 2) - (xLength / 2);
        int yLocation = (ySize / 2) - (yLength / 2);

        if (generationDebugLogs) {
            Debug.Log("Placing starting room at (" + xLocation + "," + yLocation + ")...\n" +
                "Dimensions of " + (xLength * 2 + 1) + "x" + yLength + "...\n" +
                "Direction: " + Direction.North);
        }

        GenerateRoom(xLocation, yLocation, xLength, yLength, Direction.North);

        // Keep going!
        while (curRoomFailures < maxatt) {
            // Randomly decide if we are going to attach a room vertically or horizontally
            Direction direction = Direction.INVALID;  // Placeholder, this will be updated later
            int checkDirection = Random.Range(0, 2);

            // Take a slice of the map and check along it for potential door tiles!
            Vector2Int sliceChoiceBounds = new Vector2Int(); // (min, max)
            Vector2Int sliceEndpoints = new Vector2Int();  // (min, max)
            switch (checkDirection) {
                case 0: // Vertical
                    sliceChoiceBounds = new Vector2Int(curMinBounds.x, curMaxBounds.x);
                    sliceEndpoints = new Vector2Int(curMinBounds.y, curMaxBounds.y);
                    break;
                case 1: // Horizontal
                    sliceChoiceBounds = new Vector2Int(curMinBounds.y, curMaxBounds.y);
                    sliceEndpoints = new Vector2Int(curMinBounds.x, curMaxBounds.x);
                    break;
            }

            // Pick a random line between the currently generated bounds
            // NOTE: The edges of the bounds are not actually available to put a door,
            //       therefore, we want our allowed choices to be min + 1 to max - 1.  
            //       Random.Range(int, int) is (inclusive, exclusive], so our random function looks like below
            int lineToCheck = Random.Range(sliceChoiceBounds.x + 1, sliceChoiceBounds.y);
            PotentialDoorsList pDoors = new PotentialDoorsList(sfact);

            for (int i = sliceEndpoints.x; i <= sliceEndpoints.y; ++i) {
                Tile curTile = null;
                switch (checkDirection) {
                    case 0: // Vertical
                        curTile = tiles[lineToCheck, i];
                        break;
                    case 1: // Horizontal
                        curTile = tiles[i, lineToCheck];
                        break;
                }

                // Look along the slice line for walls we intersect.  These will be our potential doors
                if (curTile != null && curTile.curTileState == Tile.TileState.Wall && !IsCorner(curTile.location.x, curTile.location.y)) {

                    // Now we want to give each potential door a weight based on how much space it has to generate a room, so
                    // determine which direction we should check space in (there should only be one) and give that tile a weight
                    direction = GetRoomDirection(curTile);
                    pDoors.AddPotentialDoor(new PotentialDoor(curTile, GetDirectedTileWeight(curTile, direction), direction));
                }
            }

            // At this point we should have a list of potential door tiles populated and weighted, so all we need to do is pick one!
            PotentialDoor newDoor = pDoors.SelectPotentialDoor();

            // If we never found any potential doors, we should try again with a new slice
            if (newDoor == null) { continue; }

            // The distance variable of a tile here is actually the max depth of the room, so lets use that information in choosing room size to reduce failures
            int halfRoomWidth = Random.Range(minDiam, maxDiam + 1) / 2;
            int roomDepth = Random.Range(minRoomDiameter, Mathf.Min(newDoor.distance, maxRoomDiameter) + 1);
            
            GenerateRoom(newDoor.tile.location.x, newDoor.tile.location.y, halfRoomWidth, roomDepth, newDoor.direction);
        }

        // Now that all the tiles are created, assign them
        mapScript.tileMap = tiles;

        return mapScript;
    }

    void ReplaceSentinels(ref int xSize, ref int ySize, ref int minDiam, ref int maxDiam, ref float sfact, ref int maxatt){
        if (xSize == -1)
            xSize = XDIMDEFAULT;
        if (ySize == -1)
            ySize = YDIMDEFAULT;
        if (minDiam == -1)
            minDiam = minRoomDiameter;
        if (maxDiam == -1)
            maxDiam = maxRoomDiameter;
        if (sfact == -1f)
            sfact = shapeFactor;
        if (maxatt == -1)
            maxatt = maxAttempts;
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
		if (CheckTiles(xLocation, yLocation, xLength, yLength, direction)) {
            // Good, lets generate it and update our current map boundaries!
            UpdateMapBoundaries(xLocation, yLocation, xLength, yLength, direction);

            for (int i = -xLength; i <= xLength; ++i) {
				for (int j = 0; j < yLength; ++j) {
					Tile curTile = null;

					// A door is a door regardless of the direction
					if (i == 0 && j == 0) {
						curTile = tiles [xLocation, yLocation];

						// The previous state of the tile should be a wall, but now its a door!
						if (curTile != null && curTile.curTileState == Tile.TileState.Wall) {
							SetDoor(curTile);
						} else {
							// This should be the first room.
							if (generationDebugLogs){
								Debug.Log ("Creating first room starting point");
							}

                            // Create a new tile and set it to a wall
							SetWall(CreateTile(xLocation, yLocation));
						}
					} else {
                        Vector2Int loc = new Vector2Int(0,0);
						switch (direction) {
						    case Direction.North:
                                loc.x = xLocation + i;
                                loc.y = yLocation + j;
							    break;
						    case Direction.South:
                                loc.x = xLocation + i;
                                loc.y = yLocation - j;
							    break;
						    case Direction.East:
                                loc.x = xLocation + j;
                                loc.y = yLocation + i;
							    break;
						    case Direction.West:
                                loc.x = xLocation - j;
                                loc.y = yLocation + i;
                                break;
                            default:
                                if (generationDebugLogs) {
                                    Debug.LogError("Invalid room direction.");
                                }
                                break;
						}

						if (Mathf.Abs (i) == xLength || j == 0 || j == (yLength - 1)) {
							SetWall(CreateTile(loc.x, loc.y));
						} else {
							SetFloor(CreateTile(loc.x, loc.y));
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
		3: 		If even one tile that we check is a generated tile, return false as it overlaps with an existing room
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
						curTile = tiles[xLocation + i, yLocation + j];
					} else {
						// This room goes past the edge of the map!
						if (generationDebugLogs) {
							Debug.Log("Result: False (Room would extend past map boundaries)");
						}
						return false;
					}
					break;
				case Direction.South:
					if (IsInMapBoundaries(xLocation + i, yLocation - j)) {
						curTile = tiles [xLocation + i, yLocation - j];
					} else {
						if (generationDebugLogs) {
							Debug.Log("Result: False (Room would extend past map boundaries)");
						}
						return false;
					}
					break;
				case Direction.East:
					if (IsInMapBoundaries(xLocation + j, yLocation + i)) {
						curTile = tiles [xLocation + j, yLocation + i];
					} else {
						if (generationDebugLogs) {
							Debug.Log("Result: False (Room would extend past map boundaries)");
						}
						return false;
					}
					break;

				case Direction.West:
					if (IsInMapBoundaries(xLocation - j, yLocation + i)) {
						curTile = tiles [xLocation - j, yLocation + i];
					} else {
						if (generationDebugLogs) {
							Debug.Log("Result: False (Room would extend past map boundaries)");
						}
						return false;
					}
					break;
				}

				// Check if this tile is ungenerated
				if (curTile != null && curTile.curTileState != Tile.TileState.Ungenerated) {
					// If this tile as already been generated, check if it is a wall
					if (curTile.curTileState != Tile.TileState.Wall) {

						// If the tile is not a wall, we have overlap and should return false
						if (generationDebugLogs) {
							Debug.Log("Result: False...Overlap at (" + (curTile.location.x) + "," + (curTile.location.y) + ")");
						}
						return false;
					} else {
						// If we have reached this point, we found a wall.  Now check if our current room would also put a wall there
						if (Mathf.Abs(i) == xLength || j == 0 || j == (yLength - 1)) {
							// Cool, this would also be a wall!  Allow it!
						} else {
							// Bad.  We wouldnt put a wall here.  That means something will be overlapping!
							if (generationDebugLogs) {
								Debug.Log("Result: False...Overlap at (" + (curTile.location.x) + "," + (curTile.location.y) + ")");
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
		if (IsInMapBoundaries(door.location.x, door.location.y + 1) && tiles[door.location.x, door.location.y + 1] == null) {
			   return Direction.North;
		} else if (IsInMapBoundaries(door.location.x, door.location.y - 1) && tiles[door.location.x, door.location.y - 1] == null) { 
			   return Direction.South;
		} else if (IsInMapBoundaries(door.location.x + 1, door.location.y) && tiles[door.location.x + 1, door.location.y] == null) {
			    return Direction.East;
		} else if (IsInMapBoundaries(door.location.x - 1, door.location.y) && tiles[door.location.x - 1, door.location.y] == null) {
			    return Direction.West;
		}
		if (generationDebugLogs) {
			Debug.LogError("No valid direction to place room");
		}
		return Direction.INVALID;
	}

    int GetDirectedTileWeight(Tile startingTile, Direction direction) {
        int weight = 0;
        Vector2Int curLoc = new Vector2Int(startingTile.location.x, startingTile.location.y);

        switch (direction) {
            case Direction.North:
                ++curLoc.y;
                break;
            case Direction.South:
                --curLoc.y;
                break;
            case Direction.East:
                ++curLoc.x;
                break;
            case Direction.West:
                --curLoc.x;
                break;
        }

        while (IsInMapBoundaries(curLoc.x, curLoc.y) && tiles[curLoc.x, curLoc.y] == null) {
            // Currently we will set weight equal to the amount of consecutive empty tiles in direction
            // We could square this number or perform other operations to get more interesting choices
            ++weight;

            switch (direction) {
                case Direction.North:
                    ++curLoc.y;
                    break;
                case Direction.South:
                    --curLoc.y;
                    break;
                case Direction.East:
                    ++curLoc.x;
                    break;
                case Direction.West:
                    --curLoc.x;
                    break;
            }
        }

        return weight;
    }

    // start:       Tile to start from
    // direction:   Direction to search in
    // distance:    How far to jump (default 1)
    Tile GetNextTile(Tile start, Direction direction, int distance = 1) {
        Tile returnTile = null;
        switch (direction) {
            case Direction.North:
                if (IsInMapBoundaries(start.location.x, start.location.y + distance)) {
                    returnTile = tiles[start.location.x, start.location.y + distance];
                }
                break;
            case Direction.South:
                if (IsInMapBoundaries(start.location.x, start.location.y - distance)) {
                    returnTile = tiles[start.location.x, start.location.y - distance];
                }
                break;
            case Direction.East:
                if (IsInMapBoundaries(start.location.x + distance, start.location.y)) {
                    returnTile = tiles[start.location.x + distance, start.location.y];
                }
                break;
            case Direction.West:
                if (IsInMapBoundaries(start.location.x - distance, start.location.y)) {
                    returnTile = tiles[start.location.x - distance, start.location.y];
                }
                break;
        }
        return returnTile;
    }

    bool IsInMapBoundaries(int x, int y){
		return (y >= 0 && y < mapSize.x && x >= 0 && x < mapSize.y);
	}

    // xLocation, yLocation: 	Coordinates to a door of this room.
    // halfRoomWidth:           How far perpendicular to the inner door face the room extends from the door tile
    // roomDepth:				How far from the door the room extends in depth towards Direction
    // direction:				In what direction the room extends from the door
    void UpdateMapBoundaries(int xLocation, int yLocation, int halfRoomWidth, int roomDepth, Direction direction) {
        roomDepth -= 1; // Small error-correction term to not double-count the door itself

        Vector2Int generatedMax = new Vector2Int();
        Vector2Int generatedMin = new Vector2Int();

        // Since the algorithm generates room directions, we need to translate these relative values into absolute ones.
        switch (direction) {
            case Direction.North:
                generatedMax.x = xLocation + halfRoomWidth;
                generatedMin.x = xLocation - halfRoomWidth;

                generatedMax.y = yLocation + roomDepth;
                generatedMin.y = yLocation;
                break;
            case Direction.South:
                generatedMax.x = xLocation + halfRoomWidth;
                generatedMin.x = xLocation - halfRoomWidth;

                generatedMax.y = yLocation;
                generatedMin.y = yLocation - roomDepth;
                break;
            case Direction.East:
                generatedMax.x = xLocation + roomDepth;
                generatedMin.x = xLocation;

                generatedMax.y = yLocation + halfRoomWidth;
                generatedMin.y = yLocation - halfRoomWidth;
                break;
            case Direction.West:
                generatedMax.x = xLocation;
                generatedMin.x = xLocation - roomDepth;

                generatedMax.y = yLocation + halfRoomWidth;
                generatedMin.y = yLocation - halfRoomWidth;
                break;
            default:
                if (generationDebugLogs) {
                    Debug.LogError("Invalid room direction when calculating new map boundaries");
                }
                break;
        }

        if (generatedMax.x > curMaxBounds.x) { curMaxBounds.x = generatedMax.x; }
        if (generatedMax.y > curMaxBounds.y) { curMaxBounds.y = generatedMax.y; }
        if (generatedMin.x < curMinBounds.x) { curMinBounds.x = generatedMin.x; }
        if (generatedMin.y < curMinBounds.y) { curMinBounds.y = generatedMin.y; }
    }

    // x, y is the postion of the tile in question
    bool IsCorner(int x, int y) {
        // A tile is a corner if any of the below combinations are both walls:
        // (top, right) (right, bottom) (bottom, left) (left, top)

        Tile top, bottom, left, right;
        top = bottom = left = right = null;

        if (IsInMapBoundaries(x, y + 1)) { top = tiles[x, y + 1]; }
        if (IsInMapBoundaries(x, y - 1)) { bottom = tiles[x, y - 1]; }
        if (IsInMapBoundaries(x - 1, y)) { left = tiles[x - 1, y]; }
        if (IsInMapBoundaries(x + 1, y)) { right = tiles[x + 1, y]; }

        // right, bottom
        if ((right != null && bottom != null) &&
            right.curTileState == Tile.TileState.Wall && bottom.curTileState == Tile.TileState.Wall) {
                return true;
        }
        // bottom, left
        if ((bottom != null && left != null) &&
            bottom.curTileState == Tile.TileState.Wall && left.curTileState == Tile.TileState.Wall) {
            return true;
        }
        // left, top
        if ((left != null && top != null) &&
            left.curTileState == Tile.TileState.Wall && top.curTileState == Tile.TileState.Wall) {
            return true;
        }
        // top, right
        if ((top != null && right != null) &&
            top.curTileState == Tile.TileState.Wall && right.curTileState == Tile.TileState.Wall) {
            return true;
        }

        return false;
    }

    Tile CreateTile(int xLoc, int zLoc) {
        // Create a tile and give it a name based on its location
        GameObject curTileObject = Instantiate(floorTile, new Vector3(xLoc, 0, zLoc), Quaternion.identity) as GameObject;
        curTileObject.name = "(" + xLoc + ", " + zLoc + ")";

        // When instantiating a Tile, attach a Tile script to it and set variables
        Tile tileScript = curTileObject.AddComponent<Tile>();
        tileScript.location = new Vector2Int(xLoc, zLoc);

        // Add the generated tile to the tiles array and set the object's parent to the map GameObject
        tiles[xLoc, zLoc] = tileScript;
        curTileObject.transform.SetParent(map.transform);
        return tileScript;
    }

    // All of the functions below are currently placeholders.  These are where we will put the logic to actually place sprites
    // and models when we have them.  For now they are just differently-colored squares

    void SetWall(Tile tile){
		tile.curTileState = Tile.TileState.Wall;
		tile.gameObject.GetComponent<Renderer>().material.color = Color.grey;
		tile.transform.localScale = new Vector3(tile.transform.localScale.x, 2f, tile.transform.localScale.z);
	}

	void SetFloor(Tile tile){
		tile.curTileState = Tile.TileState.Open;
		tile.gameObject.GetComponent<Renderer>().material.color = Color.white;
	}

	void SetDoor(Tile tile){
		tile.curTileState = Tile.TileState.Door;
		tile.gameObject.GetComponent<Renderer>().material.color = Color.magenta;
		tile.transform.localScale = new Vector3(tile.transform.localScale.x, 2.5f, tile.transform.localScale.z);
	}

}