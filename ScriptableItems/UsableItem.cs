
using System.Text;
using UnityEngine;
public enum Usability : byte { Usable, Cooldown, Empty, Never }
public abstract class UsableItem : ScriptableItem
{
    [Header("Category")]
    public string category;
    [Header("Usage")]
    public bool keepUsingWhileButtonDown; 
    public AudioClip successfulUseSound; 
    public AudioClip failedUseSound; 
    public AudioClip emptySound; 
    [Header("Cooldown")]
    public float cooldown; 
    [Tooltip("Cooldown category can be used if different potion items should share the same cooldown. Cooldown applies only to this item name if empty.")]
#pragma warning disable CS0649 
    [SerializeField] string _cooldownCategory; 
#pragma warning restore CS0649 
    public string cooldownCategory =>
        string.IsNullOrWhiteSpace(_cooldownCategory) ? name : _cooldownCategory;
    public bool shoulderLookAtWhileHolding;
    public bool useDirectly;
    public virtual Usability CanUseInventory(Player player, int inventoryIndex)
    {
        return player.GetItemCooldown(cooldownCategory) > 0
               ? Usability.Cooldown
               : Usability.Usable;
    }
    public virtual Usability CanUseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        return player.GetItemCooldown(cooldownCategory) > 0
               ? Usability.Cooldown
               : Usability.Usable;
    }
    public virtual void UseInventory(Player player, int inventoryIndex)
    {
        if (cooldown > 0)
            player.SetItemCooldown(cooldownCategory, cooldown);
    }
    public virtual void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        if (cooldown > 0)
            player.SetItemCooldown(cooldownCategory, cooldown);
    }
    public virtual void OnUsedInventory(Player player) {}
    public virtual void OnUsedHotbar(Player player, Vector3 lookAt) {}
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{COOLDOWN}", cooldown.ToString());
        return tip.ToString();
    }
}
