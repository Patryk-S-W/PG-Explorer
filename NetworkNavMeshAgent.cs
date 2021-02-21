
using UnityEngine;
using UnityEngine.AI;
using Mirror;
[RequireComponent(typeof(NavMeshAgent))]
public class NetworkNavMeshAgent : NetworkBehaviourNonAlloc
{
    public NavMeshAgent agent; 
    Vector3 requiredVelocity; 
    Vector3 lastUpdatePosition;
    Vector3 lastSerializedDestination;
    Vector3 lastSerializedVelocity;
    bool hadPath = false;
    public void LookAtY(Vector3 position)
    {
        transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
    }
    bool HasPath()
    {
        return agent.hasPath || agent.pathPending; 
    }
    void Update()
    {
        if (isServer)
        {
            bool hasPath = HasPath();
            if (hasPath && agent.destination != lastSerializedDestination)
            {
                SetDirtyBit(1);
            }
            else if (!hasPath && agent.velocity != lastSerializedVelocity)
            {
                SetDirtyBit(1);
            }
            else if (!hasPath && Vector3.Distance(transform.position, lastUpdatePosition) > agent.speed)
            {
                
                RpcWarped(transform.position);
            }
            else if (hadPath && !hasPath)
            {
                SetDirtyBit(1);
            }
            lastUpdatePosition = transform.position;
            hadPath = hasPath;
            lastUpdatePosition = transform.position;
        }
        else if (isClient)
        {
            if (requiredVelocity != Vector3.zero)
            {
                agent.ResetMovement(); 
                agent.velocity = requiredVelocity;
                LookAtY(transform.position + requiredVelocity); 
            }
        }
    }
    [ClientRpc]
    public void RpcWarped(Vector3 position)
    {
        agent.Warp(position);
    }
    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        writer.WriteVector3(transform.position);
        writer.WriteSingle(agent.speed);
        bool hasPath = HasPath();
        writer.WriteBoolean(hasPath);
        if (hasPath)
        {
            writer.WriteVector3(agent.destination);
            writer.WriteSingle(agent.stoppingDistance);
            
            lastSerializedDestination = agent.destination;
        }
        else
        {
            writer.WriteVector3(agent.velocity);
            lastSerializedVelocity = agent.velocity;
        }
        return true;
    }
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        Vector3 position = reader.ReadVector3();
        agent.speed = reader.ReadSingle();
        bool hasPath = reader.ReadBoolean();
        if (hasPath)
        {
            Vector3 destination = reader.ReadVector3();
            float stoppingDistance = reader.ReadSingle();
            if (agent.isOnNavMesh)
            {
                agent.stoppingDistance = stoppingDistance;
                agent.destination = destination;
            }
            else Debug.LogWarning("NetworkNavMeshAgent.OnDeserialize: agent not on NavMesh, name=" + name + " position=" + transform.position + " destination=" + destination);
            requiredVelocity = Vector3.zero; 
        }
        else
        {
            Vector3 velocity = reader.ReadVector3();
            
            agent.ResetPath();
            requiredVelocity = velocity;
        }
        if (Vector3.Distance(transform.position, position) > agent.speed * 2 && agent.isOnNavMesh)
        {
            agent.Warp(position);
        }
    }
}
