
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class OnDeathRespawn : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public NetworkProximityGridChecker proximityChecker;
#pragma warning disable CS0109 
    new public Collider collider;
#pragma warning restore CS0109 
    [Header("Death")]
    public float deathTime = 30; 
    [Header("Respawn")]
    public float respawnTime = 10;
    [Header("Events")]
    public UnityEvent onRespawn;
    public UnityEvent onDeathTimeElapsed;
    [Server]
    public void OnDeath()
    {
        Invoke(nameof(Disappear), deathTime);
    }
    [Server]
    void Disappear()
    {
        proximityChecker.forceHidden = true;
        collider.enabled = false;
        onDeathTimeElapsed.Invoke();
        Invoke(nameof(Reappear), respawnTime);
    }
    [Server]
    void Reappear()
    {
        proximityChecker.forceHidden = false;
        collider.enabled = true;
        foreach (Energy energy in GetComponents<Energy>())
            energy.current = energy.max;
        onRespawn.Invoke();
    }
}
