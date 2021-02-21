﻿using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class UIStorage : MonoBehaviour
{
    public GameObject panel;
    public UIStorageSlot slotPrefab;
    public Transform content;
    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            if (player.interaction.current != null &&
                ((NetworkBehaviour)player.interaction.current).GetComponent<Storage>() != null)
            {
                panel.SetActive(true);
                Storage storage = ((NetworkBehaviour)player.interaction.current).GetComponent<Storage>();
                UIUtils.BalancePrefabs(slotPrefab.gameObject, storage.slots.Count, content);
                for (int i = 0; i < storage.slots.Count; ++i)
                {
                    UIStorageSlot slot = content.GetChild(i).GetComponent<UIStorageSlot>();
                    slot.dragAndDropable.name = i.ToString(); 
                    ItemSlot itemSlot = storage.slots[i];
                    if (itemSlot.amount > 0)
                    {
                        slot.tooltip.enabled = true;
                        if (slot.tooltip.IsVisible())
                            slot.tooltip.text = itemSlot.ToolTip();
                        slot.dragAndDropable.dragable = true;
                        if (itemSlot.item.maxDurability > 0)
                        {
                            if (itemSlot.item.durability == 0)
                                slot.image.color = brokenDurabilityColor;
                            else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                                slot.image.color = lowDurabilityColor;
                            else
                                slot.image.color = Color.white;
                        }
                        else slot.image.color = Color.white; 
                        slot.image.sprite = itemSlot.item.image;
                        slot.amountOverlay.SetActive(itemSlot.amount > 1);
                        slot.amountText.text = itemSlot.amount.ToString();
                    }
                    else
                    {
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.amountOverlay.SetActive(false);
                    }
                }
            }
            else panel.SetActive(false);
        }
    }
}
