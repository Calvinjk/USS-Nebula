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
    int[] SetRoomDiameterRange(){
		int minRoomDiameter = 1;
        if (GameObject.Find("MinRoomDiameter").GetComponent<Text>().text == "" || minRoomDiameter == 3)
            minRoomDiameter = -1;
        else
            minRoomDiameter = int.Parse(GameObject.Find("MinRoomDiameter").GetComponent<Text>().text);
        if (minRoomDiameter < 3)
            minRoomDiameter = 3;

		int maxRoomDiameter;
        if (GameObject.Find("MaxRoomDiameter").GetComponent<Text>().text == "")
            maxRoomDiameter = -1;
        else
            maxRoomDiameter = int.Parse(GameObject.Find("MaxRoomDiameter").GetComponent<Text>().text);
        if (maxRoomDiameter > 20)
            maxRoomDiameter = 20;
        if (minRoomDiameter > maxRoomDiameter){
            minRoomDiameter = 3;
            maxRoomDiameter = 20;
        }

		int[] range = {minRoomDiameter, maxRoomDiameter};
		return range;
    }
    float SetShapeFactor(){
		float shapeFactor;
        if (GameObject.Find("ShapeFactor").GetComponent<Text>().text == "")
            shapeFactor = -1f;
        else
            shapeFactor = float.Parse(GameObject.Find("ShapeFactor").GetComponent<Text>().text);
        if (shapeFactor > 10f)
            shapeFactor = 10f;
		return shapeFactor;
    }
    int SetMaxAttempts(){
		int maxAttempts;
        if (GameObject.Find("MaxAttempts").GetComponent<Text>().text == "")
            maxAttempts = -1;
        else
            maxAttempts = int.Parse(GameObject.Find("MaxAttempts").GetComponent<Text>().text);
		return maxAttempts;
    }
    int SetSize(){
        int tempsize;
        if (GameObject.Find("Size").GetComponent<Text>().text != "")
            tempsize = int.Parse(GameObject.Find("Size").GetComponent<Text>().text);
        else
            tempsize = -1;
        if (tempsize < 50)
            tempsize = 50;
        return tempsize;
    }

    public Map MakeMeAMap(){
        int[] diameterrange = SetRoomDiameterRange();
        float sfactor = SetShapeFactor();
        int maxattempts = SetMaxAttempts();
        int returnsize = SetSize();

        return generatorScript.GenerateMap(xSize: SetSize(), ySize: SetSize(), maxDiam: SetRoomDiameterRange()[1], minDiam: SetRoomDiameterRange()[0], sfact: SetShapeFactor(), maxatt: SetMaxAttempts());
    }
}