using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class PlayerRespawning : NetworkBehaviourNonAlloc
{
    public Health health;
    public PlayerMovement movement;
    public float respawnTime = 10;
    [SyncVar, HideInInspector] public double respawnTimeEnd; 
    [Header("Events")]
    public UnityEvent onRespawn;
    [ServerCallback]
    void Update()
    {
        if (health.current == 0 && NetworkTime.time >= respawnTimeEnd)
            onRespawn.Invoke();
    }
    [Server]
    public void OnDeath()
    {
        respawnTimeEnd = NetworkTime.time + respawnTime;
    }
    [Server]
    public void OnRespawn()
    {
        Vector3 start = NetworkManager.singleton.GetStartPosition().position;
        movement.Warp(start);
        foreach (Energy energy in GetComponents<Energy>())
            energy.current = energy.max;
        print(name + " respawned");
    }
}
