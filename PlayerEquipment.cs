using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Mirror;
[Serializable]
public struct EquipmentInfo
{
    public string requiredCategory;
    public Transform location;
    public ScriptableItemAndAmount defaultItem;
}
[RequireComponent(typeof(Animator))]
public class PlayerEquipment : Equipment
{
    public Animator animator;
    public EquipmentInfo[] slotInfo =
    {
        new EquipmentInfo{requiredCategory="Head", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Chest", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Legs", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Feet", location=null, defaultItem=new ScriptableItemAndAmount()}
    };
    [FormerlySerializedAs("rightHandLocation")]
    public Transform weaponMount;
    Dictionary<string, Transform> skinBones = new Dictionary<string, Transform>();
    void Awake()
    {
        foreach (SkinnedMeshRenderer skin in GetComponentsInChildren<SkinnedMeshRenderer>())
            foreach (Transform bone in skin.bones)
                skinBones[bone.name] = bone;
        if (weaponMount != null && weaponMount.childCount > 0)
            Debug.LogWarning(name + " PlayerEquipment.weaponMount should have no children, otherwise they will be destroyed.");
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        slots.Callback += OnEquipmentChanged;
        for (int i = 0; i < slots.Count; ++i)
            RefreshLocation(i);
    }
    bool CanReplaceAllBones(SkinnedMeshRenderer equipmentSkin)
    {
        foreach (Transform bone in equipmentSkin.bones)
            if (!skinBones.ContainsKey(bone.name))
                return false;
        return true;
    }
    void ReplaceAllBones(SkinnedMeshRenderer equipmentSkin)
    {
        Transform[] bones = equipmentSkin.bones;
        for (int i = 0; i < bones.Length; ++i)
        {
            string boneName = bones[i].name;
            if (!skinBones.TryGetValue(boneName, out bones[i]))
                Debug.LogWarning(equipmentSkin.name + " bone " + boneName + " not found in original player bones. Make sure to check CanReplaceAllBones before.");
        }
        equipmentSkin.bones = bones;
    }
    void RebindAnimators()
    {
        foreach (var anim in GetComponentsInChildren<Animator>())
            anim.Rebind();
    }
    public void RefreshLocation(Transform location, ItemSlot slot)
    {
        if (slot.amount > 0)
        {
            ScriptableItem itemData = slot.item.data;
            
            if (location.childCount == 0 || itemData.modelPrefab == null ||
                location.GetChild(0).name != itemData.modelPrefab.name)
            {
                if (location.childCount > 0)
                    Destroy(location.GetChild(0).gameObject);
                if (itemData.modelPrefab != null)
                {
                    GameObject go = Instantiate(itemData.modelPrefab, location, false);
                    go.name = itemData.modelPrefab.name; 
                    
                    
                    SkinnedMeshRenderer equipmentSkin = go.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (equipmentSkin != null && CanReplaceAllBones(equipmentSkin))
                        ReplaceAllBones(equipmentSkin);
                    
                    
                    Animator anim = go.GetComponent<Animator>();
                    if (anim != null)
                    {
                        anim.runtimeAnimatorController = animator.runtimeAnimatorController;
                        RebindAnimators();
                    }
                }
            }
        }
        else
        {
            if (location.childCount > 0)
                Destroy(location.GetChild(0).gameObject);
        }
    }
    void RefreshLocation(int index)
    {
        ItemSlot slot = slots[index];
        EquipmentInfo info = slotInfo[index];
        if (info.requiredCategory != "" && info.location != null)
            RefreshLocation(info.location, slot);
    }
    void OnEquipmentChanged(SyncListItemSlot.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        ScriptableItem oldItem = oldSlot.amount > 0 ? oldSlot.item.data : null;
        ScriptableItem newItem = newSlot.amount > 0 ? newSlot.item.data : null;
        if (oldItem != newItem)
        {
            RefreshLocation(index);
        }
    }
    [Server]
    public void SwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            ItemSlot slot = inventory.slots[inventoryIndex];
            if (slot.amount == 0 ||
                slot.item.data is EquipmentItem &&
                ((EquipmentItem)slot.item.data).CanEquip(this, inventoryIndex, equipmentIndex))
            {
                ItemSlot temp = slots[equipmentIndex];
                slots[equipmentIndex] = slot;
                inventory.slots[inventoryIndex] = temp;
            }
        }
    }
    [Command]
    public void CmdSwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        SwapInventoryEquip(inventoryIndex, equipmentIndex);
    }
    [Command]
    public void CmdMergeInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            ItemSlot slotFrom = inventory.slots[inventoryIndex];
            ItemSlot slotTo = slots[equipmentIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                if (slotFrom.item.Equals(slotTo.item))
                {
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);
                    inventory.slots[inventoryIndex] = slotFrom;
                    slots[equipmentIndex] = slotTo;
                }
            }
        }
    }
    [Command]
    public void CmdMergeEquipInventory(int equipmentIndex, int inventoryIndex)
    {
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            ItemSlot slotFrom = slots[equipmentIndex];
            ItemSlot slotTo = inventory.slots[inventoryIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                if (slotFrom.item.Equals(slotTo.item))
                {
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);
                    slots[equipmentIndex] = slotFrom;
                    inventory.slots[inventoryIndex] = slotTo;
                }
            }
        }
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
    public int GetEquipmentTypeIndex(string category)
    {
        for (int i = 0; i < slotInfo.Length; ++i)
            if (slotInfo[i].requiredCategory == category)
                return i;
        return -1;
    }
    [Server]
    public void DropItemAndClearSlot(int index)
    {
        ItemSlot slot = slots[index];
        ((PlayerInventory)inventory).DropItem(slot.item, slot.amount);
        slot.amount = 0;
        slots[index] = slot;
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
        for (int i = 0; i < slotInfo.Length; ++i)
            slots[i] = slotInfo[i].defaultItem.item != null ? new ItemSlot(new Item(slotInfo[i].defaultItem.item), slotInfo[i].defaultItem.amount) : new ItemSlot();
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
    void OnDragAndDrop_InventorySlot_EquipmentSlot(int[] slotIndices)
    {
        if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
            inventory.slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
        {
            CmdMergeInventoryEquip(slotIndices[0], slotIndices[1]);
        }
        else
        {
            CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
        }
    }
    void OnDragAndDrop_EquipmentSlot_InventorySlot(int[] slotIndices)
    {
        if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
            slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
        {
            CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
        }
        else
        {
            CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]);
        }
    }
    void OnDragAndClear_EquipmentSlot(int slotIndex)
    {
        CmdDropItem(slotIndex);
    }
    void OnValidate()
    {
        for (int i = 0; i < slotInfo.Length; ++i)
            if (slotInfo[i].defaultItem.item != null && slotInfo[i].defaultItem.amount == 0)
                slotInfo[i].defaultItem.amount = 1;
    }
}