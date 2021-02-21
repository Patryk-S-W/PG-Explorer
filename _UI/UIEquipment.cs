﻿using UnityEngine;
using UnityEngine.UI;
public class UIEquipment : MonoBehaviour
{
    public GameObject panel;
    public UIEquipmentSlot slotPrefab;
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
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.equipment.slots.Count, content);
            for (int i = 0; i < player.equipment.slots.Count; ++i)
            {
                UIEquipmentSlot slot = content.GetChild(i).GetComponent<UIEquipmentSlot>();
                slot.dragAndDropable.name = i.ToString(); 
                ItemSlot itemSlot = player.equipment.slots[i];
                slot.categoryOverlay.SetActive(player.equipment.slotInfo[i].requiredCategory != "");
                string overlay = Utils.ParseLastNoun(player.equipment.slotInfo[i].requiredCategory);
                slot.categoryText.text = overlay != "" ? overlay : "?";
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
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }
                else
                {
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                }
            }
        }
    }
}
