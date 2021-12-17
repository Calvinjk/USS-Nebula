using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharControl : MonoBehaviour {

    bool debugLogs = false;
    float currRot;

    [SerializeField]
    float moveSpeed = 4f; //Change in inspector to adjust move speed
    Vector3 forward, right; // Keeps track of our relative forward and right vectors
    
    void Start()
    {
        forward = Camera.main.transform.forward; // Set forward to equal the camera's forward vector
        forward.y = 0; // make sure y is 0
        forward = Vector3.Normalize(forward); // make sure the length of vector is set to a max of 1.0
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward; // set the right-facing vector to be facing right relative to the camera's forward vector
    }
    
    void Update()
    {
        if(Input.anyKey) // only execute if a key is being pressed
            Move();
        //get the current camera rotation
        currRot = GameObject.Find("Camera").GetComponent<CamControl>().currRot;

    }
    
    void Move()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal Key"), 0, Input.GetAxis("Vertical Key")); // setup a direction Vector based on keyboard input. GetAxis returns a value between -1.0 and 1.0. If the A key is pressed, GetAxis(HorizontalKey) will return -1.0. If D is pressed, it will return 1.0
        if (debugLogs)
            Debug.Log(direction);
        Vector3 rightMovement = right * moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal Key"); // Our right movement is based on the right vector, movement speed, and our GetAxis command. We multiply by Time.deltaTime to make the movement smooth.
        Vector3 forwardMovement = forward * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical Key"); // Up movement uses the forward vector, movement speed, and the vertical axis 
        Vector3 heading = Vector3.Normalize(rightMovement + forwardMovement); // This creates our new direction. By combining our right and forward movements and normalizing them, we create a new vector that points in the appropriate direction with a length no greater than 1.0
        transform.forward = heading; // Sets forward direction of our game object to whatever direction we're moving in
        
        //rotate the direction of movement depending on camera current rotation
        transform.position += Quaternion.Euler(0, currRot, 0) * rightMovement; // move our transform's position right/left
        transform.position += Quaternion.Euler(0, currRot, 0) * forwardMovement; // Move our transform's position up/down
    }

}