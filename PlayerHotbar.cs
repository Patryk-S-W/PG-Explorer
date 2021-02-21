using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[Serializable]
public struct HotbarModelLocation
{
    public string requiredCategory;
    public Transform location;
}
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerEquipment))]
public class PlayerHotbar : ItemContainer, ICombatBonus
{
    [Header("Components")]
    public Player player;
    public AudioSource audioSource;
    public Health health;
    public PlayerInventory inventory;
    public PlayerEquipment equipment;
    public PlayerMovement movement;
    public PlayerReloading reloading;
    public PlayerLook look;
    [Header("Hotbar")]
    public int size = 10;
    public ScriptableItem[] defaultItems;
    public KeyCode[] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
    [SyncVar(hook=nameof(OnSelectionChanged))] public int selection = 0; 
    public MeleeWeaponItem hands;
    public HotbarModelLocation[] modelLocations;
    public int GetDamageBonus()
    {
        ItemSlot slot = slots[selection];
        if (slot.amount > 0 && slot.item.data is WeaponItem)
            return ((WeaponItem)slot.item.data).damage;
        return 0;
    }
    public int GetDefenseBonus() { return 0; }
    public UsableItem GetUsableItemOrHands(int index)
    {
        ItemSlot slot = slots[index];
        return slot.amount > 0 ? (UsableItem)slot.item.data : hands;
    }
    public UsableItem GetCurrentUsableItemOrHands()
    {
        return GetUsableItemOrHands(selection);
    }
    bool IsHandsOrItemWithValidDurability(int slotIndex)
    {
       return slots[slotIndex].amount == 0 ||
              slots[slotIndex].item.CheckDurability();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        slots.Callback += OnHotbarChanged;
        RefreshLocations();
    }
    void RefreshLocations()
    {
        HashSet<int> assignedLocations = new HashSet<int>();
        for (int hotbarIndex = 0; hotbarIndex < slots.Count; ++hotbarIndex)
        {
            ItemSlot slot = slots[hotbarIndex];
            if (slot.amount > 0 && slot.item.data.modelPrefab != null)
            {
                UsableItem itemData = (UsableItem)(slot.item.data);
                for (int locationIndex = 0; locationIndex < modelLocations.Length; ++locationIndex)
                {
                    HotbarModelLocation modelLocation = modelLocations[locationIndex];
                    if (itemData.category.StartsWith(modelLocation.requiredCategory) &&
                        !assignedLocations.Contains(locationIndex))
                    {
                        GameObject model;
                        if (modelLocation.location.childCount == 0)
                        {
                            model = Instantiate(itemData.modelPrefab, modelLocation.location, false);
                            model.name = itemData.modelPrefab.name; 
                        }
                        else if (modelLocation.location.GetChild(0).name == itemData.modelPrefab.name)
                        {
                            model = modelLocation.location.GetChild(0).gameObject;
                        }
                        else
                        {
                            GameObject oldModel = modelLocation.location.GetChild(0).gameObject;
                            oldModel.transform.parent = null;
                            Destroy(oldModel);
                            model = Instantiate(itemData.modelPrefab);
                            model.name = itemData.modelPrefab.name; 
                            model.transform.SetParent(modelLocation.location, false);
                        }
                        model.SetActive(hotbarIndex != selection);
                        assignedLocations.Add(locationIndex);
                        break;
                    }
                }
            }
        }
        for (int locationIndex = 0; locationIndex < modelLocations.Length; ++locationIndex)
        {
            HotbarModelLocation modelLocation = modelLocations[locationIndex];
            if (!assignedLocations.Contains(locationIndex) &&
                modelLocation.location.childCount > 0)
            {
                GameObject oldModel = modelLocation.location.GetChild(0).gameObject;
                oldModel.transform.parent = null;
                Destroy(oldModel);
            }
        }
    }
    void OnHotbarChanged(SyncListItemSlot.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        ScriptableItem oldItem = oldSlot.amount > 0 ? oldSlot.item.data : null;
        ScriptableItem newItem = newSlot.amount > 0 ? newSlot.item.data : null;
        if (oldItem != newItem)
        {
            RefreshLocations();
        }
    }
    void OnSelectionChanged(int oldValue, int newValue)
    {
        RefreshLocations();
    }
    [Client]
    void TryUseItem(UsableItem itemData)
    {
        if (itemData.keepUsingWhileButtonDown || Input.GetMouseButtonDown(0))
        {
            if (IsHandsOrItemWithValidDurability(selection))
            {
                Vector3 lookAt = look.lookPositionRaycasted;
                Usability usability = itemData.CanUseHotbar(player, selection, lookAt);
                if (usability == Usability.Usable)
                {
                    CmdUseItem(selection, lookAt);
                    
                    
                    if (player.isNonHostLocalPlayer)
                        OnUsedItem(itemData, lookAt);
                }
                else if (usability == Usability.Empty)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (itemData.emptySound)
                            audioSource.PlayOneShot(itemData.emptySound);
                    }
                }
            }
        }
    }
    void Update()
    {
        equipment.RefreshLocation(equipment.weaponMount, slots[selection]);
        if (isLocalPlayer)
        {
            if (Input.GetMouseButton(0) &&
                Cursor.lockState == CursorLockMode.Locked &&
                health.current > 0 &&
                movement.state != MoveState.CLIMBING &&
                reloading.ReloadTimeRemaining() == 0 &&
                !look.IsFreeLooking() &&
                !Utils.IsCursorOverUserInterface() &&
                Input.touchCount <= 1)
            {
                TryUseItem(GetCurrentUsableItemOrHands());
            }
        }
    }
    [Command]
    public void CmdSelect(int index)
    {
        if (0 <= index && index < slots.Count &&
            reloading.ReloadTimeRemaining() == 0)
            selection = index;
    }
    [Command]
    public void CmdSwapHotbarHotbar(int fromIndex, int toIndex)
    {
        if (health.current > 0 &&
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
    public void CmdHotbarSplit(int fromIndex, int toIndex)
    {
        if (health.current > 0 &&
            0 <= fromIndex && fromIndex < slots.Count &&
            0 <= toIndex && toIndex < slots.Count &&
            fromIndex != toIndex)
        {
            ItemSlot slotFrom = slots[fromIndex];
            ItemSlot slotTo = slots[toIndex];
            if (slotFrom.amount >= 2 && slotTo.amount == 0)
            {
                slotTo = slotFrom; 
                slotTo.amount = slotFrom.amount / 2;
                slotFrom.amount -= slotTo.amount; 
                slots[fromIndex] = slotFrom;
                slots[toIndex] = slotTo;
            }
        }
    }
    [Command]
    public void CmdHotbarMerge(int fromIndex, int toIndex)
    {
        if (health.current > 0 &&
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
    public bool IsAllowedOnHotbar(Item item)
    {
        return item.data is UsableItem;
    }
    public bool IsAllowedOnHotbar(ItemSlot slot)
    {
        return slot.amount == 0 || IsAllowedOnHotbar(slot.item);
    }
    [Command]
    public void CmdSwapInventoryHotbar(int inventoryIndex, int hotbarIndex)
    {
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= hotbarIndex && hotbarIndex < slots.Count &&
            IsAllowedOnHotbar(inventory.slots[inventoryIndex]))
        {
            ItemSlot temp = slots[hotbarIndex];
            slots[hotbarIndex] = inventory.slots[inventoryIndex];
            inventory.slots[inventoryIndex] = temp;
        }
    }
    [Command]
    public void CmdMergeInventoryHotbar(int inventoryIndex, int hotbarIndex)
    {
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= hotbarIndex && hotbarIndex < slots.Count)
        {
            ItemSlot slotFrom = inventory.slots[inventoryIndex];
            ItemSlot slotTo = slots[hotbarIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                if (slotFrom.item.Equals(slotTo.item))
                {
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);
                    inventory.slots[inventoryIndex] = slotFrom;
                    slots[hotbarIndex] = slotTo;
                }
            }
        }
    }
    [Command]
    public void CmdMergeHotbarInventory(int hotbarIndex, int inventoryIndex)
    {
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= hotbarIndex && hotbarIndex < slots.Count)
        {
            ItemSlot slotFrom = slots[hotbarIndex];
            ItemSlot slotTo = inventory.slots[inventoryIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                if (slotFrom.item.Equals(slotTo.item))
                {
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);
                    slots[hotbarIndex] = slotFrom;
                    inventory.slots[inventoryIndex] = slotTo;
                }
            }
        }
    }
    void OnUsedItem(UsableItem itemData, Vector3 lookAt)
    {
        if (player.isNonHostLocalPlayer)
            player.SetItemCooldown(itemData.cooldownCategory, itemData.cooldown);
        itemData.OnUsedHotbar(player, lookAt);
        if (itemData is WeaponItem)
            foreach (Animator animator in GetComponentsInChildren<Animator>())
                animator.SetTrigger("UPPERBODY_USED");
    }
    [ClientRpc]
    public void RpcUsedItem(int itemNameHash, Vector3 lookAt)
    {
        if (!player.isNonHostLocalPlayer)
        {
            Item item = new Item{hash=itemNameHash};
            OnUsedItem((UsableItem)item.data, lookAt);
        }
    }
    [Command]
    public void CmdUseItem(int index, Vector3 lookAt)
    {
        if (0 <= index && index < slots.Count &&
            health.current > 0 &&
            IsHandsOrItemWithValidDurability(index))
        {
            UsableItem itemData = GetUsableItemOrHands(index);
            if (itemData.CanUseHotbar(player, index, lookAt) == Usability.Usable)
            {
                itemData.UseHotbar(player, index, lookAt);
                
                
                RpcUsedItem(new Item(itemData).hash, lookAt);
            }
            else
            {
                
                Debug.Log("CmdUseItem rejected for: " + name + " item=" + itemData.name + "@" + NetworkTime.time);
            }
        }
    }
    [Server]
    public void DropItemAndClearSlot(int index)
    {
        ItemSlot slot = slots[index];
        inventory.DropItem(slot.item, slot.amount);
        slot.amount = 0;
        slots[index] = slot;
    }
    [Command]
    public void CmdDropItem(int index)
    {
        if (health.current > 0 &&
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
    void OnDragAndDrop_HotbarSlot_HotbarSlot(int[] slotIndices)
    {
        if (slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
            slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
        {
            CmdHotbarMerge(slotIndices[0], slotIndices[1]);
        }
        else if (Utils.AnyKeyPressed(inventory.splitKeys))
        {
            CmdHotbarSplit(slotIndices[0], slotIndices[1]);
        }
        else
        {
            CmdSwapHotbarHotbar(slotIndices[0], slotIndices[1]);
        }
    }
    void OnDragAndDrop_InventorySlot_HotbarSlot(int[] slotIndices)
    {
        if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
            inventory.slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
        {
            CmdMergeInventoryHotbar(slotIndices[0], slotIndices[1]);
        }
        else if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
                 reloading.CanLoadAmmoIntoWeapon(inventory.slots[slotIndices[0]], slots[slotIndices[1]].item))
        {
            reloading.CmdReloadWeaponOnHotbar(slotIndices[0], slotIndices[1]);
        }
        else if (IsAllowedOnHotbar(inventory.slots[slotIndices[0]]))
        {
            CmdSwapInventoryHotbar(slotIndices[0], slotIndices[1]);
        }
    }
    void OnDragAndDrop_HotbarSlot_InventorySlot(int[] slotIndices)
    {
        if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
            slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
        {
            CmdMergeHotbarInventory(slotIndices[0], slotIndices[1]);
        }
        else if (IsAllowedOnHotbar(inventory.slots[slotIndices[1]]))
        {
            CmdSwapInventoryHotbar(slotIndices[1], slotIndices[0]);
        }
    }
    void OnDragAndClear_HotbarSlot(int slotIndex)
    {
        CmdDropItem(slotIndex);
    }
}