using UnityEngine;
public class FollowMouse : MonoBehaviour {
    void Update() {
        Vector3 temp = Input.mousePosition;
        temp.z = 21f; // Set this to be the distance you want the object to be placed in front of the camera.
        transform.position = Camera.main.ScreenToWorldPoint(temp);
    }
}
