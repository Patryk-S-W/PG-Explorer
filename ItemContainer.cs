
using UnityEngine;
using Mirror;
public abstract class ItemContainer : NetworkBehaviourNonAlloc
{
    public SyncListItemSlot slots = new SyncListItemSlot();
    public int GetItemIndexByName(string itemName)
    {
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            if (slot.amount > 0 && slot.item.name == itemName)
                return i;
        }
        return -1;
    }
}
