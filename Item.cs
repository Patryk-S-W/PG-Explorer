
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;
[Serializable]
public struct Item
{
    public int hash;
    public int ammo;
    public int durability;
    public Item(ScriptableItem data)
    {
        hash = data.name.GetStableHashCode();
        ammo = 0;
        durability = data.maxDurability;
    }
    public ScriptableItem data
    {
        get
        {
            
            if (!ScriptableItem.dict.ContainsKey(hash))
                throw new KeyNotFoundException("There is no ScriptableItem with hash=" + hash + ". Make sure that all ScriptableItems are in the Resources folder so they are loaded properly.");
            return ScriptableItem.dict[hash];
        }
    }
    public string name => data.name;
    public int maxStack => data.maxStack;
    public int maxDurability => data.maxDurability;
    public float DurabilityPercent()
    {
        return (durability != 0 && maxDurability != 0) ? (float)durability / (float)maxDurability : 0;
    }
    public bool destroyable => data.destroyable;
    public Sprite image => data.image;
    public bool CheckDurability() =>
        maxDurability == 0 || durability > 0;
    public string ToolTip()
    {
        StringBuilder tip = new StringBuilder(data.ToolTip());
        tip.Replace("{AMMO}", ammo.ToString());
        if (maxDurability > 0)
            tip.Replace("{DURABILITY}", (DurabilityPercent() * 100).ToString("F0"));
        return tip.ToString();
    }
}