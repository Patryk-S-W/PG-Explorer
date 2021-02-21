
using UnityEngine;
using Mirror;
public class PlayerHeartbeat : NetworkBehaviourNonAlloc
{
    public AudioSource audioSource;
    public Health health;
    void Update()
    {
        audioSource.volume = isLocalPlayer ? (1 - health.Percent()) : 0;
    }
}
