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
	public void GenerateNewMap(int size = 50){
		if (map != null) { Destroy (map.gameObject); }

		map = generatorScript.GenerateMap (size, size);
	}
}