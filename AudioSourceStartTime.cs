using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioSourceStartTime : MonoBehaviour
{
#pragma warning disable CS0109 
    new public AudioSource audio;
#pragma warning restore CS0109 
    public float time = 0;
    void Start()
    {
        audio.time = time;
    }
}
