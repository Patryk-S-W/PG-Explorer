
using UnityEngine;
using Mirror;
public class PlayerFurnaceUsage : NetworkBehaviourNonAlloc
{
    public Health health;
    public PlayerLook look;
    public PlayerInteraction interaction;
    public PlayerInventory inventory;
    public KeyCode[] splitKeys = {KeyCode.LeftShift, KeyCode.RightShift};
    [Command]
    public void CmdSwapInventoryFurnaceIngredient(GameObject furnaceGameObject, int inventoryIndex)
    {
        if (furnaceGameObject != null)
        {
            Furnace furnace = furnaceGameObject.GetComponent<Furnace>();
            if (furnace != null &&
                Vector3.Distance(look.headPosition, furnace.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count)
            {
                furnace.CancelBaking();
                ItemSlot temp = furnace.ingredientSlot;
                furnace.ingredientSlot = inventory.slots[inventoryIndex];
                inventory.slots[inventoryIndex] = temp;
            }
        }
    }
    [Command]
    public void CmdSwapInventoryFurnaceFuel(GameObject furnaceGameObject, int inventoryIndex)
    {
        if (furnaceGameObject != null)
        {
            Furnace furnace = furnaceGameObject.GetComponent<Furnace>();
            if (furnace != null &&
                Vector3.Distance(look.headPosition, furnace.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count)
            {
                furnace.CancelBaking();
                ItemSlot temp = furnace.fuelSlot;
                furnace.fuelSlot = inventory.slots[inventoryIndex];
                inventory.slots[inventoryIndex] = temp;
            }
        }
    }
    [Command]
    public void CmdMoveFurnaceResultInventory(GameObject furnaceGameObject, int inventoryIndex)
    {
        if (furnaceGameObject != null)
        {
            Furnace furnace = furnaceGameObject.GetComponent<Furnace>();
            if (furnace != null &&
                Vector3.Distance(look.headPosition, furnace.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                inventory.slots[inventoryIndex].amount == 0 &&
                furnace.resultSlot.amount > 0)
            {
                inventory.slots[inventoryIndex] = furnace.resultSlot;
                furnace.resultSlot = new ItemSlot();
            }
        }
    }
    [Command]
    public void CmdMergeInventoryFurnaceIngredient(GameObject furnaceGameObject, int inventoryIndex)
    {
        if (furnaceGameObject != null)
        {
            Furnace furnace = furnaceGameObject.GetComponent<Furnace>();
            if (furnace != null &&
                Vector3.Distance(look.headPosition, furnace.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count)
            {
                furnace.CancelBaking();
                ItemSlot slotFrom = inventory.slots[inventoryIndex];
                ItemSlot slotTo = furnace.ingredientSlot;
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);
                        inventory.slots[inventoryIndex] = slotFrom;
                        furnace.ingredientSlot = slotTo;
                    }
                }
            }
        }
    }
    [Command]
    public void CmdMergeInventoryFurnaceFuel(GameObject furnaceGameObject, int inventoryIndex)
    {
        if (furnaceGameObject != null)
        {
            Furnace furnace = furnaceGameObject.GetComponent<Furnace>();
            if (furnace != null &&
                Vector3.Distance(look.headPosition, furnace.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count)
            {
                furnace.CancelBaking();
                ItemSlot slotFrom = inventory.slots[inventoryIndex];
                ItemSlot slotTo = furnace.fuelSlot;
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);
                        inventory.slots[inventoryIndex] = slotFrom;
                        furnace.fuelSlot = slotTo;
                    }
                }
            }
        }
    }
    [Command]
    public void CmdMergeFurnaceIngredientInventory(GameObject furnaceGameObject, int inventoryIndex)
    {
        if (furnaceGameObject != null)
        {
            Furnace furnace = furnaceGameObject.GetComponent<Furnace>();
            if (furnace != null &&
                Vector3.Distance(look.headPosition, furnace.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count)
            {
                furnace.CancelBaking();
                ItemSlot slotFrom = furnace.ingredientSlot;
                ItemSlot slotTo = inventory.slots[inventoryIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);
                        furnace.ingredientSlot = slotFrom;
                        inventory.slots[inventoryIndex] = slotTo;
                    }
                }
            }
        }
    }
    [Command]
    public void CmdMergeFurnaceFuelInventory(GameObject furnaceGameObject, int inventoryIndex)
    {
        if (furnaceGameObject != null)
        {
            Furnace furnace = furnaceGameObject.GetComponent<Furnace>();
            if (furnace != null &&
                Vector3.Distance(look.headPosition, furnace.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count)
            {
                furnace.CancelBaking();
                ItemSlot slotFrom = furnace.fuelSlot;
                ItemSlot slotTo = inventory.slots[inventoryIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);
                        furnace.fuelSlot = slotFrom;
                        inventory.slots[inventoryIndex] = slotTo;
                    }
                }
            }
        }
    }
    [Command]
    public void CmdMergeFurnaceResultInventory(GameObject furnaceGameObject, int inventoryIndex)
    {
        if (furnaceGameObject != null)
        {
            Furnace furnace = furnaceGameObject.GetComponent<Furnace>();
            if (furnace != null &&
                Vector3.Distance(look.headPosition, furnace.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count)
            {
                ItemSlot slotFrom = furnace.resultSlot;
                ItemSlot slotTo = inventory.slots[inventoryIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);
                        furnace.resultSlot = slotFrom;
                        inventory.slots[inventoryIndex] = slotTo;
                    }
                }
            }
        }
    }
    void OnDragAndDrop_InventorySlot_FurnaceIngredientSlot(int[] slotIndices)
    {
        if (interaction.current != null)
        {
            Furnace furnace = ((NetworkBehaviour)interaction.current).GetComponent<Furnace>();
            if (furnace != null)
            {
                if (inventory.slots[slotIndices[0]].amount > 0 && furnace.ingredientSlot.amount > 0 &&
                    inventory.slots[slotIndices[0]].item.Equals(furnace.ingredientSlot.item))
                {
                    CmdMergeInventoryFurnaceIngredient(furnace.gameObject, slotIndices[0]);
                }
                else
                {
                    CmdSwapInventoryFurnaceIngredient(furnace.gameObject, slotIndices[0]);
                }
            }
        }
    }
    void OnDragAndDrop_FurnaceIngredientSlot_InventorySlot(int[] slotIndices)
    {
        if (interaction.current != null)
        {
            Furnace furnace = ((NetworkBehaviour)interaction.current).GetComponent<Furnace>();
            if (furnace != null)
            {
                if (furnace.ingredientSlot.amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
                    furnace.ingredientSlot.item.Equals(inventory.slots[slotIndices[1]].item))
                {
                    CmdMergeFurnaceIngredientInventory(furnace.gameObject, slotIndices[1]);
                }
                else
                {
                    CmdSwapInventoryFurnaceIngredient(furnace.gameObject, slotIndices[1]);
                }
            }
        }
    }
    void OnDragAndDrop_InventorySlot_FurnaceFuelSlot(int[] slotIndices)
    {
        if (interaction.current != null)
        {
            Furnace furnace = ((NetworkBehaviour)interaction.current).GetComponent<Furnace>();
            if (furnace != null)
            {
                if (inventory.slots[slotIndices[0]].amount > 0 && furnace.fuelSlot.amount > 0 &&
                    inventory.slots[slotIndices[0]].item.Equals(furnace.fuelSlot.item))
                {
                    CmdMergeInventoryFurnaceFuel(furnace.gameObject, slotIndices[0]);
                }
                else
                {
                    CmdSwapInventoryFurnaceFuel(furnace.gameObject, slotIndices[0]);
                }
            }
        }
    }
    void OnDragAndDrop_FurnaceFuelSlot_InventorySlot(int[] slotIndices)
    {
        if (interaction.current != null)
        {
            Furnace furnace = ((NetworkBehaviour)interaction.current).GetComponent<Furnace>();
            if (furnace != null)
            {
                if (furnace.fuelSlot.amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
                    furnace.fuelSlot.item.Equals(inventory.slots[slotIndices[1]].item))
                {
                    CmdMergeFurnaceFuelInventory(furnace.gameObject, slotIndices[1]);
                }
                else
                {
                    CmdSwapInventoryFurnaceFuel(furnace.gameObject, slotIndices[1]);
                }
            }
        }
    }
    void OnDragAndDrop_FurnaceResultSlot_InventorySlot(int[] slotIndices)
    {
        if (interaction.current != null)
        {
            Furnace furnace = ((NetworkBehaviour)interaction.current).GetComponent<Furnace>();
            if (furnace != null)
            {
                if (furnace.resultSlot.amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
                    furnace.resultSlot.item.Equals(inventory.slots[slotIndices[1]].item))
                {
                    CmdMergeFurnaceResultInventory(furnace.gameObject, slotIndices[1]);
                }
                else if (furnace.resultSlot.amount > 0 && inventory.slots[slotIndices[1]].amount == 0)
                {
                    CmdMoveFurnaceResultInventory(furnace.gameObject, slotIndices[1]);
                }
            }
        }
    }
}
