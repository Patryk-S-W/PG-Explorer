using UnityEngine;
public abstract class ScriptableObjectNonAlloc : ScriptableObject
{
    string cachedName;
    public new string name
    {
        get
        {
            if (string.IsNullOrWhiteSpace(cachedName))
                cachedName = base.name;
            return cachedName;
        }
    }
}
