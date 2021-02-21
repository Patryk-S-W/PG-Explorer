
using UnityEngine;
using UnityEngine.AI;
public class NavMeshPathfindingIterationsPerFrame : MonoBehaviour
{
    public int iterations = 100; 
    void Awake()
    {
        print("Setting NavMesh Pathfinding Iterations Per Frame from " + NavMesh.pathfindingIterationsPerFrame + " to " + iterations);
        NavMesh.pathfindingIterationsPerFrame = iterations;
    }
}
