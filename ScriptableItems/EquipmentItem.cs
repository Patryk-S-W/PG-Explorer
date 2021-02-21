using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName="uSurvival Item/Equipment", order=999)]
public class EquipmentItem : UsableItem
{
    [Header("Equipment")]
    public int healthBonus;
    public int hydrationBonus;
    public int nutritionBonus;
    public int damageBonus;
    public int defenseBonus;
    public override Usability CanUseInventory(Player player, int inventoryIndex)
    {
        Usability baseUsable = base.CanUseInventory(player, inventoryIndex);
        if (baseUsable != Usability.Usable)
            return baseUsable;
        return FindEquipableSlotFor(player.equipment, inventoryIndex) != -1
               ? Usability.Usable
               : Usability.Never;
    }
    public override Usability CanUseHotbar(Player player, int hotbarIndex, Vector3 lookAt) { return Usability.Never; }
    public bool CanEquip(PlayerEquipment equipment, int inventoryIndex, int equipmentIndex)
    {
        string requiredCategory = equipment.slotInfo[equipmentIndex].requiredCategory;
        return requiredCategory != "" &&
               category.StartsWith(requiredCategory);
    }
    int FindEquipableSlotFor(PlayerEquipment equipment, int inventoryIndex)
    {
        for (int i = 0; i < equipment.slots.Count; ++i)
            if (CanEquip(equipment, inventoryIndex, i))
                return i;
        return -1;
    }
    public override void UseInventory(Player player, int inventoryIndex)
    {
        int slot = FindEquipableSlotFor(player.equipment, inventoryIndex);
        if (slot != -1)
        {
            if (player.inventory.slots[inventoryIndex].amount > 0 && player.equipment.slots[slot].amount > 0 &&
                player.inventory.slots[inventoryIndex].item.Equals(player.equipment.slots[slot].item))
            {
                ItemSlot slotFrom = player.inventory.slots[inventoryIndex];
                ItemSlot slotTo = player.equipment.slots[slot];
                int put = slotTo.IncreaseAmount(slotFrom.amount);
                slotFrom.DecreaseAmount(put);
                player.equipment.slots[slot] = slotTo;
                player.inventory.slots[inventoryIndex] = slotFrom;
            }
            else
            {
                player.equipment.SwapInventoryEquip(inventoryIndex, slot);
            }
        }
    }
    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt) {}
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{CATEGORY}", category);
        tip.Replace("{HEALTHBONUS}", healthBonus.ToString());
        tip.Replace("{HYDRATIONBONUS}", hydrationBonus.ToString());
        tip.Replace("{NUTRITIONBONUS}", nutritionBonus.ToString());
        tip.Replace("{DAMAGEBONUS}", damageBonus.ToString());
        tip.Replace("{DEFENSEBONUS}", defenseBonus.ToString());
        return tip.ToString();
    }
}
