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
        Text mintxt = GameObject.Find("MinRoomDiameter").GetComponent<Text>();
        Text maxtxt = GameObject.Find("MaxRoomDiameter").GetComponent<Text>();
		int minRoomDiameter = 1;
        if (mintxt.text == "")
            minRoomDiameter = -1;
        else
            minRoomDiameter = int.Parse(mintxt.text);
        if (minRoomDiameter < MIN_ALLOWED_ROOM_DIAM)
            minRoomDiameter = MIN_ALLOWED_ROOM_DIAM;

		int maxRoomDiameter;
        if (maxtxt.text == "")
            maxRoomDiameter = -1;
        else
            maxRoomDiameter = int.Parse(maxtxt.text);
        if (minRoomDiameter > maxRoomDiameter){
            int temp = minRoomDiameter;
            minRoomDiameter = maxRoomDiameter;
            maxRoomDiameter = temp;
        }

		int[] range = {minRoomDiameter, maxRoomDiameter};
		return range;
    }
    float GetShapeFactor(){
        Text sftxt = GameObject.Find("ShapeFactor").GetComponent<Text>();
		float shapeFactor;
        if (sftxt.text == "")
            shapeFactor = -11f;
        else
            shapeFactor = float.Parse(sftxt.text);
        if (shapeFactor > 10f)
            shapeFactor = 10f;
        if (shapeFactor < -10f)
            shapeFactor = -10f;
		return shapeFactor;
    }
    int GetMaxAttempts(){
        Text atttxt = GameObject.Find("MaxAttempts").GetComponent<Text>();
		int maxAttempts;
        if (atttxt.text == "")
            maxAttempts = -1;
        else
            maxAttempts = int.Parse(atttxt.text);
		if (maxAttempts < 1)
            maxAttempts = 1;
        return maxAttempts;
    }

    int GetXSize(){
        int tempsize;
        Text xtext = GameObject.Find("XSize").GetComponent<Text>();
        if (xtext.text == "")
            tempsize = -1;
        else
            tempsize = int.Parse(xtext.text);
        if (tempsize < MIN_ALLOWED_SIZE)
            tempsize = MIN_ALLOWED_SIZE;
        return tempsize;
    }

    int GetYSize(){
        Text ytext = GameObject.Find("YSize").GetComponent<Text>();
        int tempsize;
        if (ytext.text == "")
            tempsize = -1;
        else
            tempsize = int.Parse(ytext.text);
        if (tempsize < MIN_ALLOWED_SIZE)
            tempsize = MIN_ALLOWED_SIZE;
        return tempsize;
    }

    public Map MakeMeAMap(){
        return generatorScript.GenerateMap(xSize: GetXSize(), ySize: GetYSize(), maxDiam: GetRoomDiameterRange()[1], minDiam: GetRoomDiameterRange()[0], sFact: GetShapeFactor(), maxAtmp: GetMaxAttempts());
    }
}