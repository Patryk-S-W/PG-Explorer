
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkProximityGridChecker : NetworkBehaviourNonAlloc
{
    public static int visRange = 100;
    public static int resolution => visRange / 3;
    static Grid2D<NetworkConnection> grid = new Grid2D<NetworkConnection>();
    [TooltipAttribute("How often (in seconds) that this object should update the set of players that can see it.")]
    public float visUpdateInterval = 1; 
    [TooltipAttribute("Enable to force this object to be hidden from players.")]
    public bool forceHidden;
    [TooltipAttribute("Which method to use for checking proximity of players.\n\nPhysics3D uses xz to determine proximity.\n\nPhysics2D uses xy to determine proximity.")]
    public NetworkProximityChecker.CheckMethod checkMethod = NetworkProximityChecker.CheckMethod.Physics3D;
    Vector2Int previous = new Vector2Int(int.MaxValue, int.MaxValue);
    float m_VisUpdateTime;
    public override bool OnCheckObserver(NetworkConnection newObserver)
    {
        if (forceHidden)
            return false;
        Vector2Int projected = ProjectToGrid(transform.position);
        Vector2Int observerProjected = ProjectToGrid(newObserver.identity.transform.position);
        return (projected - observerProjected).sqrMagnitude <= 2;
    }
    Vector2Int ProjectToGrid(Vector3 position)
    {
        if (checkMethod == NetworkProximityChecker.CheckMethod.Physics3D)
        {
            return Vector2Int.RoundToInt(new Vector2(position.x, position.z) / resolution);
        }
        else
        {
            return Vector2Int.RoundToInt(new Vector2(position.x, position.y) / resolution);
        }
    }
    void Update()
    {
        if (!NetworkServer.active) return;
        if (connectionToClient != null)
        {
            Vector2Int current = ProjectToGrid(transform.position);
            if (current != previous)
            {
                grid.Remove(previous, connectionToClient);
                grid.Add(current, connectionToClient);
                previous = current;
            }
        }
        if (Time.time - m_VisUpdateTime > visUpdateInterval)
        {
            netIdentity.RebuildObservers(false);
            m_VisUpdateTime = Time.time;
        }
    }
    void OnDestroy()
    {
        if (connectionToClient != null)
            grid.Remove(ProjectToGrid(transform.position), connectionToClient);
    }
    public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
    {
        if (forceHidden)
            return true;
        Vector2Int current = ProjectToGrid(transform.position);
        grid.GetWithNeighbours(current, observers);
        return true;
    }
    public override void OnSetHostVisibility(bool visible)
    {
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            rend.enabled = visible;
        }
    }
}
