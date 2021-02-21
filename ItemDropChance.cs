
using System;
using UnityEngine;
[Serializable]
public class ItemDropChance
{
    public ItemDrop drop;
    [Range(0,1)] public float probability;
}
