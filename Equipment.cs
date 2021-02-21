using UnityEngine;
using Mirror;
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Inventory))]
public abstract class Equipment : ItemContainer, IHealthBonus, ICombatBonus
{
    public Health health;
    public Inventory inventory;
    public int GetHealthBonus(int baseHealth)
    {
        int bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).healthBonus;
        return bonus;
    }
    public int GetHealthRecoveryBonus()
    {
        return 0;
    }
    public int GetDamageBonus()
    {
        int bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).damageBonus;
        return bonus;
    }
    public int GetDefenseBonus()
    {
        int bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).defenseBonus;
        return bonus;
    }
}