using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName="uSurvival Item/Potion", order=999)]
public class PotionItem : UsableItem
{
    [Header("Potion")]
    public int usageHealth;
    void ApplyEffects(Player player)
    {
        player.health.current += usageHealth;
    }
    public override void UseInventory(Player player, int inventoryIndex)
    {
        base.UseInventory(player, inventoryIndex);
        ApplyEffects(player);
        ItemSlot slot = player.inventory.slots[inventoryIndex];
        slot.DecreaseAmount(1);
        player.inventory.slots[inventoryIndex] = slot;
    }
    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        base.UseHotbar(player, hotbarIndex, lookAt);
        ApplyEffects(player);
        ItemSlot slot = player.hotbar.slots[hotbarIndex];
        slot.DecreaseAmount(1);
        player.hotbar.slots[hotbarIndex] = slot;
    }
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{USAGEHEALTH}", usageHealth.ToString());
        return tip.ToString();
    }
}
