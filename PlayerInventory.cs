using UnityEngine;
using Mirror;
[RequireComponent(typeof(Health))]
public class PlayerInventory : Inventory
{
    [Header("Components")]
    public Player player;
    [Header("Inventory")]
    public int size = 10;
    public ScriptableItemAndAmount[] defaultItems;
    public KeyCode[] splitKeys = {KeyCode.LeftShift, KeyCode.RightShift};
    [Header("Item Drops")]
    public float dropRadius = 1;
    public int dropSolverAttempts = 3; 
    [Command]
    public void CmdSwapInventoryInventory(int fromIndex, int toIndex)
    {
        if (player.health.current > 0 &&
            0 <= fromIndex && fromIndex < slots.Count &&
            0 <= toIndex && toIndex < slots.Count &&
            fromIndex != toIndex)
        {
            ItemSlot temp = slots[fromIndex];
            slots[fromIndex] = slots[toIndex];
            slots[toIndex] = temp;
        }
    }
    [Command]
    public void CmdInventorySplit(int fromIndex, int toIndex)
    {
        if (player.health.current > 0 &&
            0 <= fromIndex && fromIndex < slots.Count &&
            0 <= toIndex && toIndex < slots.Count &&
            fromIndex != toIndex)
        {
            ItemSlot slotFrom = slots[fromIndex];
            ItemSlot slotTo = slots[toIndex];
            if (slotFrom.amount >= 2 && slotTo.amount == 0) {
                slotTo = slotFrom; 
                slotTo.amount = slotFrom.amount / 2;
                slotFrom.amount -= slotTo.amount; 
                slots[fromIndex] = slotFrom;
                slots[toIndex] = slotTo;
            }
        }
    }
    [Command]
    public void CmdInventoryMerge(int fromIndex, int toIndex)
    {
        if (player.health.current > 0 &&
            0 <= fromIndex && fromIndex < slots.Count &&
            0 <= toIndex && toIndex < slots.Count &&
            fromIndex != toIndex)
        {
            ItemSlot slotFrom = slots[fromIndex];
            ItemSlot slotTo = slots[toIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                if (slotFrom.item.Equals(slotTo.item))
                {
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);
                    slots[fromIndex] = slotFrom;
                    slots[toIndex] = slotTo;
                }
            }
        }
    }
    [ClientRpc]
    public void RpcUsedItem(Item item)
    {
        if (item.data is UsableItem usable)
        {
            usable.OnUsedInventory(player);
        }
    }
    [Command]
    public void CmdUseItem(int index)
    {
        if (player.health.current > 0 &&
            0 <= index && index < slots.Count &&
            slots[index].amount > 0 &&
            slots[index].item.CheckDurability() &&
            slots[index].item.data is UsableItem usable)
        {
            if (usable.CanUseInventory(player, index) == Usability.Usable)
            {
                Item item = slots[index].item;
                usable.UseInventory(player, index);
                RpcUsedItem(item);
            }
        }
    }
    [Server]
    public void DropItem(Item item, int amount)
    {
        Vector3 position = Utils.ReachableRandomUnitCircleOnNavMesh(transform.position, dropRadius, dropSolverAttempts);
        GameObject go = Instantiate(item.data.drop.gameObject, position, Quaternion.identity);
        ItemDrop drop = go.GetComponent<ItemDrop>();
        drop.item = item;
        drop.amount = amount;
        NetworkServer.Spawn(go);
    }
    [Server]
    public void DropItemAndClearSlot(int index)
    {
        ItemSlot slot = slots[index];
        DropItem(slot.item, slot.amount);
        slot.amount = 0;
        slots[index] = slot;
    }
    [Command]
    public void CmdDropItem(int index)
    {
        if (player.health.current > 0 &&
            0 <= index && index < slots.Count && slots[index].amount > 0)
        {
            DropItemAndClearSlot(index);
        }
    }
    public void OnReceivedDamage(Entity attacker, int damage)
    {
        for (int i = 0; i < slots.Count; ++i)
        {
            if (slots[i].amount > 0)
            {
                ItemSlot slot = slots[i];
                slot.item.durability = Mathf.Clamp(slot.item.durability - damage, 0, slot.item.maxDurability);
                slots[i] = slot;
            }
        }
    }
    [Server]
    public void OnDeath()
    {
        for (int i = 0; i < slots.Count; ++i)
            if (slots[i].amount > 0)
                DropItemAndClearSlot(i);
    }
    [Server]
    public void OnRespawn()
    {
        for (int i = 0; i < slots.Count; ++i)
            slots[i] = i < defaultItems.Length ? new ItemSlot(new Item(defaultItems[i].item), defaultItems[i].amount) : new ItemSlot();
    }
    void OnDragAndDrop_InventorySlot_InventorySlot(int[] slotIndices)
    {
        if (slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
            slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
        {
            CmdInventoryMerge(slotIndices[0], slotIndices[1]);
        }
        else if (Utils.AnyKeyPressed(splitKeys))
        {
            CmdInventorySplit(slotIndices[0], slotIndices[1]);
        }
        else if (slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
                 player.reloading.CanLoadAmmoIntoWeapon(slots[slotIndices[0]], slots[slotIndices[1]].item))
        {
            player.reloading.CmdReloadWeaponInInventory(slotIndices[0], slotIndices[1]);
        }
        else
        {
            CmdSwapInventoryInventory(slotIndices[0], slotIndices[1]);
        }
    }
    void OnDragAndClear_InventorySlot(int slotIndex)
    {
        CmdDropItem(slotIndex);
    }
    void OnValidate()
    {
        for (int i = 0; i < defaultItems.Length; ++i)
            if (defaultItems[i].item != null && defaultItems[i].amount == 0)
                defaultItems[i].amount = 1;
    }
}