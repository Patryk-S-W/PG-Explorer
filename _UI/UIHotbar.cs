using UnityEngine;
using UnityEngine.UI;
public class UIHotbar : MonoBehaviour
{
    public GameObject panel;
    public UIHotbarSlot slotPrefab;
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
            panel.SetActive(true);
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.hotbar.size, content);
            for (int i = 0; i < player.hotbar.size; ++i)
            {
                UIHotbarSlot slot = content.GetChild(i).GetComponent<UIHotbarSlot>();
                slot.dragAndDropable.name = i.ToString(); 
                ItemSlot itemSlot = player.hotbar.slots[i];
                if (Input.GetKeyDown(player.hotbar.keys[i]) &&
                    player.reloading.ReloadTimeRemaining() == 0 &&
                    !UIUtils.AnyInputActive())
                {
                    if (itemSlot.amount == 0)
                    {
                        player.hotbar.CmdSelect(i);
                    }
                    else if (itemSlot.item.data is UsableItem usable)
                    {
                        if (usable.useDirectly)
                            player.hotbar.CmdUseItem(i, player.look.lookPositionRaycasted);
                        else
                            player.hotbar.CmdSelect(i);
                    }
                }
                slot.hotkeyText.text = player.hotbar.keys[i].ToString().Replace("Alpha", "");
                slot.selectionOutline.SetActive(i == player.hotbar.selection);
                if (itemSlot.amount > 0)
                {
                    slot.tooltip.enabled = true;
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
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    if (itemSlot.item.data is UsableItem)
                    {
                        UsableItem usable = (UsableItem)itemSlot.item.data;
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    if (itemSlot.amount > 1) slot.amountText.text = itemSlot.amount.ToString();
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
        else panel.SetActive(false);
    }
}
