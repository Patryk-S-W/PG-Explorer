
using UnityEngine;
using Mirror;
public class PlayerStorageUsage : NetworkBehaviourNonAlloc
{
    public Health health;
    public PlayerLook look;
    public PlayerInteraction interaction;
    public PlayerInventory inventory;
    public KeyCode[] splitKeys = {KeyCode.LeftShift, KeyCode.RightShift};
    [Command]
    public void CmdSwapStorageStorage(GameObject storageGameObject, int fromIndex, int toIndex)
    {
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= fromIndex && fromIndex < storage.slots.Count &&
                0 <= toIndex && toIndex < storage.slots.Count &&
                fromIndex != toIndex)
            {
                ItemSlot temp = storage.slots[fromIndex];
                storage.slots[fromIndex] = storage.slots[toIndex];
                storage.slots[toIndex] = temp;
            }
        }
    }
    [Command]
    public void CmdStorageSplit(GameObject storageGameObject, int fromIndex, int toIndex)
    {
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= fromIndex && fromIndex < storage.slots.Count &&
                0 <= toIndex && toIndex < storage.slots.Count &&
                fromIndex != toIndex)
            {
                ItemSlot slotFrom = storage.slots[fromIndex];
                ItemSlot slotTo = storage.slots[toIndex];
                if (slotFrom.amount >= 2 && slotTo.amount == 0) {
                    slotTo = slotFrom; 
                    slotTo.amount = slotFrom.amount / 2;
                    slotFrom.amount -= slotTo.amount; 
                    storage.slots[fromIndex] = slotFrom;
                    storage.slots[toIndex] = slotTo;
                }
            }
        }
    }
    [Command]
    public void CmdStorageMerge(GameObject storageGameObject, int fromIndex, int toIndex)
    {
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= fromIndex && fromIndex < storage.slots.Count &&
                0 <= toIndex && toIndex < storage.slots.Count &&
                fromIndex != toIndex)
            {
                ItemSlot slotFrom = storage.slots[fromIndex];
                ItemSlot slotTo = storage.slots[toIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);
                        storage.slots[fromIndex] = slotFrom;
                        storage.slots[toIndex] = slotTo;
                    }
                }
            }
        }
    }
    [Command]
    public void CmdSwapInventoryStorage(GameObject storageGameObject, int inventoryIndex, int storageIndex)
    {
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= storageIndex && storageIndex < storage.slots.Count)
            {
                ItemSlot temp = storage.slots[storageIndex];
                storage.slots[storageIndex] = inventory.slots[inventoryIndex];
                inventory.slots[inventoryIndex] = temp;
            }
        }
    }
    [Command]
    public void CmdMergeInventoryStorage(GameObject storageGameObject, int inventoryIndex, int storageIndex)
    {
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= storageIndex && storageIndex < storage.slots.Count)
            {
                ItemSlot slotFrom = inventory.slots[inventoryIndex];
                ItemSlot slotTo = storage.slots[storageIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);
                        inventory.slots[inventoryIndex] = slotFrom;
                        storage.slots[storageIndex] = slotTo;
                    }
                }
            }
        }
    }
    [Command]
    public void CmdMergeStorageInventory(GameObject storageGameObject, int storageIndex, int inventoryIndex)
    {
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= storageIndex && storageIndex < storage.slots.Count)
            {
                ItemSlot slotFrom = storage.slots[storageIndex];
                ItemSlot slotTo = inventory.slots[inventoryIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);
                        storage.slots[storageIndex] = slotFrom;
                        inventory.slots[inventoryIndex] = slotTo;
                    }
                }
            }
        }
    }
    void OnDragAndDrop_StorageSlot_StorageSlot(int[] slotIndices)
    {
        if (interaction.current != null)
        {
            Storage storage = ((NetworkBehaviour)interaction.current).GetComponent<Storage>();
            if (storage != null)
            {
                if (storage.slots[slotIndices[0]].amount > 0 && storage.slots[slotIndices[1]].amount > 0 &&
                    storage.slots[slotIndices[0]].item.Equals(storage.slots[slotIndices[1]].item))
                {
                    CmdStorageMerge(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                else if (Utils.AnyKeyPressed(splitKeys))
                {
                    CmdStorageSplit(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                else
                {
                    CmdSwapStorageStorage(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
            }
        }
    }
    void OnDragAndDrop_InventorySlot_StorageSlot(int[] slotIndices)
    {
        if (interaction.current != null)
        {
            Storage storage = ((NetworkBehaviour)interaction.current).GetComponent<Storage>();
            if (storage != null)
            {
                if (inventory.slots[slotIndices[0]].amount > 0 && storage.slots[slotIndices[1]].amount > 0 &&
                    inventory.slots[slotIndices[0]].item.Equals(storage.slots[slotIndices[1]].item))
                {
                    CmdMergeInventoryStorage(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                else
                {
                    CmdSwapInventoryStorage(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
            }
        }
    }
    void OnDragAndDrop_StorageSlot_InventorySlot(int[] slotIndices)
    {
        if (interaction.current != null)
        {
            Storage storage = ((NetworkBehaviour)interaction.current).GetComponent<Storage>();
            if (storage != null)
            {
                if (storage.slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
                    storage.slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
                {
                    CmdMergeStorageInventory(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                else
                {
                    CmdSwapInventoryStorage(storage.gameObject, slotIndices[1], slotIndices[0]);
                }
            }
        }
    }
}
