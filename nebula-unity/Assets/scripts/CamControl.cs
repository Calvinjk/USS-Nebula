using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour {

    public float freeCameraSpeed = 1f;
    public float lerpSpeed = 0.1f;
    public float minSnapDistance = 0.025f;

    public bool ________________;

    public enum CameraState {
        Idle,
        VectorTarget,
        ObjectTarget
    };

    public CameraState cameraState = CameraState.Idle;
    public bool followTarget = false;

    public Vector3 vectorTarget;
    public GameObject objectTarget;

    public void setCameraTarget(Vector3 target) {
        vectorTarget = target;
        cameraState = CameraState.VectorTarget;

        objectTarget = null;
        followTarget = false;
    }

    public void setCameraTarget(GameObject target, bool follow) {
        objectTarget = target;
        cameraState = CameraState.ObjectTarget;
        followTarget = follow;

        vectorTarget = Vector3.zero;
    }

    public void removeCameraTarget() {
        vectorTarget = Vector3.zero;
        objectTarget = null;
        followTarget = false;
        cameraState = CameraState.Idle;
    }

    void FixedUpdate() {
        switch (cameraState) {
            // Move towards target vector
            case CameraState.VectorTarget:
                transform.position = Vector3.Lerp(transform.position, vectorTarget, lerpSpeed);

                // Snap into place if close enough to target position
                if (Vector3.Distance(transform.position, vectorTarget) < minSnapDistance) {
                    transform.position = vectorTarget;

                    // No reason to keep this location saved if we have reached it already
                    removeCameraTarget();
                }
                break;
            // Move towards target object
            case CameraState.ObjectTarget:
                Vector3 targetPosition = objectTarget.transform.position;
                transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed);

                if (!followTarget) {
                    // Snap into place if close enough to target object
                    if (Vector3.Distance(transform.position, targetPosition) < minSnapDistance) {
                        transform.position = vectorTarget;

                        // No reason to keep this location saved if we have reached it already
                        removeCameraTarget();
                    }
                }
                break;
            // Free target the camera
            case CameraState.Idle:
                if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow)) {
                    //transform.position = new Vector3(transform.position.x - (freeCameraSpeed * Time.deltaTime), transform.position.y, transform.position.z + (freeCameraSpeed * Time.deltaTime));
                    transform.RotateAround(Vector3.zero, Vector3.up, 5*freeCameraSpeed*Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.RightArrow)) {
                    //transform.position = new Vector3(transform.position.x + (freeCameraSpeed * Time.deltaTime), transform.position.y, transform.position.z - (freeCameraSpeed * Time.deltaTime));
                    transform.RotateAround(Vector3.zero, Vector3.down, 5*freeCameraSpeed*Time.deltaTime);
                }
                /*
                if (Input.GetKey(KeyCode.UpArrow)) {
                    transform.position = new Vector3(transform.position.x + (freeCameraSpeed * Time.deltaTime), transform.position.y, transform.position.z + (freeCameraSpeed * Time.deltaTime));
                }
                if (Input.GetKey(KeyCode.DownArrow)) {
                    transform.position = new Vector3(transform.position.x - (freeCameraSpeed * Time.deltaTime), transform.position.y, transform.position.z - (freeCameraSpeed * Time.deltaTime));
                }
                */
                break;
            default:
                Debug.LogError("Invalid CameraState");
                break;
        }
    }
}