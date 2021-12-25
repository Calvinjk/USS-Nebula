using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour {

    bool debugLogs = false;

    public float freeCameraSpeed = 1f;
    public float lerpSpeed = 0.1f;
    public float minSnapDistance = 0.025f;
    public float currRot = 0f;

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

    void Update(){
        RotationCheck();
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
            case CameraState.Idle:
                break;
            default:
                Debug.LogError("Invalid CameraState");
                break;
        }
    }

    void RotationCheck(){
        //rotate cam by 90 degrees around camera target
        if (Input.GetKeyDown(KeyCode.Q)) {
            transform.RotateAround(GameObject.Find("CameraTarget").transform.position, Vector3.up, 90f);
            currRot += 90f;
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            transform.RotateAround(GameObject.Find("CameraTarget").transform.position, Vector3.down, 90f);
            currRot -= 90f;
        }
    }
}