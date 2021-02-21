using UnityEngine;
public class CameraRide : MonoBehaviour
{
    public float speed = 0.1f;
    void Update()
    {
        if (Player.localPlayer) Destroy(this);
        transform.position -= transform.forward * speed * Time.deltaTime;
    }
}
