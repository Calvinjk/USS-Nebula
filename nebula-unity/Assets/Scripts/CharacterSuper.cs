using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSuper : MonoBehaviour
{
    // Start is called before the first frame update
    public float hp;
    public float maxhp;
    public float moveSpeed;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TakeDamage(float damage){
        hp -= damage;
        DeathCheck();
    }

    void DeathCheck(){
        if (hp <= 0f){
            Destroy(gameObject);
        if (this.gameObject.name == "Player")
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

}
