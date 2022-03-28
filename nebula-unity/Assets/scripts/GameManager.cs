using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The GameManager tracks and stores gamedata other classes may need to access
public class GameManager : MonoBehaviour {

	public DungeonMapGenerator generatorScript;

	public bool _______________;

	public Map map;
    public static int MIN_ALLOWED_ROOM_DIAM = 3;
    public static int MIN_ALLOWED_SIZE = 50;

	void Awake(){
		DontDestroyOnLoad(this.gameObject);
	}

	// TODO - Sterilize Input
	public void GenerateNewMap(){
        if (map != null) { Destroy (map.gameObject); }

        map = MakeMeAMap ();
    }

	/*
    =
    =
    =
    EXPOSE GENERATOR VARIABLES
    =
    =
    =
    */
    int[] GetRoomDiameterRange(){
		int minRoomDiameter = 1;
        if (GameObject.Find("MinRoomDiameter").GetComponent<Text>().text == "")
            minRoomDiameter = -1;
        else
            minRoomDiameter = int.Parse(GameObject.Find("MinRoomDiameter").GetComponent<Text>().text);
        if (minRoomDiameter < MIN_ALLOWED_ROOM_DIAM)
            minRoomDiameter = MIN_ALLOWED_ROOM_DIAM;

		int maxRoomDiameter;
        if (GameObject.Find("MaxRoomDiameter").GetComponent<Text>().text == "")
            maxRoomDiameter = -1;
        else
            maxRoomDiameter = int.Parse(GameObject.Find("MaxRoomDiameter").GetComponent<Text>().text);
        if (minRoomDiameter > maxRoomDiameter){
            int temp = minRoomDiameter;
            minRoomDiameter = maxRoomDiameter;
            maxRoomDiameter = temp;
        }

		int[] range = {minRoomDiameter, maxRoomDiameter};
		return range;
    }
    float GetShapeFactor(){
		float shapeFactor;
        if (GameObject.Find("ShapeFactor").GetComponent<Text>().text == "")
            shapeFactor = -11f;
        else
            shapeFactor = float.Parse(GameObject.Find("ShapeFactor").GetComponent<Text>().text);
        if (shapeFactor > 10f)
            shapeFactor = 10f;
        if (shapeFactor < -10f)
            shapeFactor = -10f;
		return shapeFactor;
    }
    int GetMaxAttempts(){
		int maxAttempts;
        if (GameObject.Find("MaxAttempts").GetComponent<Text>().text == "")
            maxAttempts = -1;
        else
            maxAttempts = int.Parse(GameObject.Find("MaxAttempts").GetComponent<Text>().text);
		if (maxAttempts < 1)
            maxAttempts = 1;
        return maxAttempts;
    }

    int GetXSize(){
        int tempsize;
        if (GameObject.Find("XSize").GetComponent<Text>().text == "")
            tempsize = -1;
        else
            tempsize = int.Parse(GameObject.Find("Size").GetComponent<Text>().text);
        if (tempsize < MIN_ALLOWED_SIZE)
            tempsize = MIN_ALLOWED_SIZE;
        return tempsize;
    }

    int GetYSize(){
        int tempsize;
        if (GameObject.Find("YSize").GetComponent<Text>().text == "")
            tempsize = -1;
        else
            tempsize = int.Parse(GameObject.Find("Size").GetComponent<Text>().text);
        if (tempsize < MIN_ALLOWED_SIZE)
            tempsize = MIN_ALLOWED_SIZE;
        return tempsize;
    }

    public Map MakeMeAMap(){
        return generatorScript.GenerateMap(xSize: GetXSize(), ySize: GetYSize(), maxDiam: GetRoomDiameterRange()[1], minDiam: GetRoomDiameterRange()[0], sFact: GetShapeFactor(), maxAtmp: GetMaxAttempts());
    }
}