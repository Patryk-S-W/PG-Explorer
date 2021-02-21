
using System;
using UnityEngine;
using UnityEngine.Events;
[Serializable] public class UnityEventEntityInt : UnityEvent<Entity, int> {}
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Combat))]
[RequireComponent(typeof(NetworkProximityGridChecker))]
public abstract class Entity : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public Health health;
    public Combat combat;
    public NetworkProximityGridChecker proximityChecker;
#pragma warning disable CS0109 
    new public Collider collider; 
#pragma warning restore CS0109 
}
