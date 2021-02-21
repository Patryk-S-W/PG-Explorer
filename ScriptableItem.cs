
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName="uSurvival Item/General", order=999)]
public class ScriptableItem : ScriptableObjectNonAlloc
{
    [Header("Base Stats")]
    public int maxStack = 1; 
    [Tooltip("Durability is only allowed for non-stackable items (if MaxStack is 1))")]
    public int maxDurability = 1000; 
    public bool destroyable;
    [TextArea(1, 30)] public string toolTip;
    public Sprite image;
    [Header("3D Representation")]
    public ItemDrop drop; 
    public GameObject modelPrefab; 
    
    /*
    <b>{NAME}</b>
    Description here...
    Destroyable: {DESTROYABLE}
    Amount: {AMOUNT}
    */
    public virtual string ToolTip()
    {
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{NAME}", name);
        tip.Replace("{DESTROYABLE}", (destroyable ? "Yes" : "No"));
        return tip.ToString();
    }
    protected virtual void OnValidate()
    {
        if (maxStack > 1 && maxDurability != 0)
        {
            maxDurability = 0;
            Debug.LogWarning(name + " maxDurability was reset to 0 because it's not stackable. Set maxStack to 1 if you want to use durability.");
        }
    }
    static Dictionary<int, ScriptableItem> cache;
    public static Dictionary<int, ScriptableItem> dict
    {
        get
        {
            if (cache == null)
            {
                ScriptableItem[] items = Resources.LoadAll<ScriptableItem>("");
                List<string> duplicates = items.ToList().FindDuplicates(item => item.name);
                if (duplicates.Count == 0)
                {
                    cache = items.ToDictionary(item => item.name.GetStableHashCode(), item => item);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableItems with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cache;
        }
    }
}
[Serializable]
public struct ScriptableItemAndAmount
{
    public ScriptableItem item;
    public int amount;
}
