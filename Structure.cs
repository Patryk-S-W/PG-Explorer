
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Structure : Entity
{
    public static HashSet<Structure> structures = new HashSet<Structure>();
    public override void OnStartServer()
    {
        structures.Add(this);
    }
    void OnDestroy()
    {
        structures.Remove(this);
    }
}
