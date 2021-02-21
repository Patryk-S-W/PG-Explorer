
using UnityEngine;
public class MuzzleFlash : MonoBehaviour
{
    public float visibleTime = 0.1f;
    public bool rotate = true;
    float endTime;
    void FixedUpdate()
    {
        transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
        if (rotate) transform.Rotate(0, 0, Random.Range(0f, 90f));
        if (Time.time >= endTime) gameObject.SetActive(false);
    }
    public void Fire()
    {
        gameObject.SetActive(true);
        endTime = Time.time + visibleTime;
    }
}
