using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEnemy : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        maxhp = 75f;
        hp = maxhp;
        moveSpeed = 0.01f;
        player = GameObject.Find("Player");
    }

    void Awake(){
        InvokeRepeating("BasicAttack", 1.5f, 1.5f); //starting in 1.5s, attack every 1.5s
        InvokeRepeating("SuperAttack", 4.0f, 4.0f); //starting in 1.5s, attack every 1.5s
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player.transform);
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed);
    }

    void SuperAttack(){
        GameObject frontbullet = Instantiate(bullet, transform.TransformPoint(Vector3.forward*2), transform.rotation);
        GameObject leftbullet = Instantiate(bullet, transform.TransformPoint(Vector3.left*6), transform.rotation);
        GameObject rightbullet = Instantiate(bullet, transform.TransformPoint(Vector3.right*6), transform.rotation);
        Rigidbody _rb1 = frontbullet.GetComponent<Rigidbody>();
        _rb1.AddForce(frontbullet.transform.forward*500f);
        Rigidbody _rb3 = leftbullet.GetComponent<Rigidbody>();
        _rb3.AddForce(leftbullet.transform.forward*500f);
        Rigidbody _rb4 = rightbullet.GetComponent<Rigidbody>();
        _rb4.AddForce(rightbullet.transform.forward*500f);
    }

}
