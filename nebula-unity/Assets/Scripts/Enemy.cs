using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CharacterSuper
{

    public GameObject bullet;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        maxhp = 50f;
        hp = maxhp;
        moveSpeed = 0.025f;
        player = GameObject.Find("Player");   
    }

    void Awake(){
        InvokeRepeating("BasicAttack", 1.5f, 1.5f); //starting in 1.5s, attack every 1.5s
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player.transform);
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed);
    }

    void BasicAttack(){
        GameObject firedbullet = Instantiate(bullet, transform.TransformPoint(Vector3.forward*2), transform.rotation);
        Rigidbody _rb = firedbullet.GetComponent<Rigidbody>();
        _rb.AddForce(firedbullet.transform.forward*500f);

    }

}
