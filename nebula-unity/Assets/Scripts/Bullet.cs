using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    float damage;

    void Start()
    {
        damage = 25f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnBecameInvisible(){
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision){
        if (collision.gameObject.tag != "Bullet" && collision.gameObject.tag != "Ground"){
            collision.gameObject.SendMessage("TakeDamage", damage);
        }
        Destroy(gameObject);
    }
}
