
using UnityEngine;
using Mirror;
[RequireComponent(typeof(NetworkTransform))] 
public class UseRigidbodyDuringDeath : NetworkBehaviourNonAlloc
{
    public Rigidbody rigidBody;
    public float applyForce = 1000;
    public bool resetPositionWhenRespawning = true;
    Vector3 startPosition;
    Quaternion startRotation;
    bool dirty;
    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }
    [Server]
    public void OnDeath()
    {
        StartFall();
        RpcStartFall();
    }
    [Server]
    public void OnDeathTimeElapsed()
    {
        StopFall();
        RpcStopFall();
    }
    [Server]
    public void OnRespawn()
    {
        if (resetPositionWhenRespawning)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
    }
    void StartFall()
    {
        rigidBody.isKinematic = false;
        rigidBody.AddForce(transform.forward * applyForce);
    }
    void StopFall()
    {
        rigidBody.isKinematic = true;
    }
    [ClientRpc]
    void RpcStartFall()
    {
        if (isServer) return; 
        StartFall();
    }
    [ClientRpc]
    void RpcStopFall()
    {
        if (isServer) return; 
        StopFall();
    }
}
