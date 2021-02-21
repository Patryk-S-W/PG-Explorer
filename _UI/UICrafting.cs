using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class UICrafting : MonoBehaviour
{
    public UICraftingIngredientSlot ingredientSlotPrefab;
    public Transform ingredientContent;
    public Image resultSlotImage;
    public UIShowToolTip resultSlotToolTip;
    public Button craftButton;
    public Text resultText;
    public Color successColor = Color.green;
    public Color failedColor = Color.red;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            UIUtils.BalancePrefabs(ingredientSlotPrefab.gameObject, player.crafting.indices.Count, ingredientContent);
            for (int i = 0; i < player.crafting.indices.Count; ++i)
            {
                UICraftingIngredientSlot slot = ingredientContent.GetChild(i).GetComponent<UICraftingIngredientSlot>();
                slot.dragAndDropable.name = i.ToString(); 
                int itemIndex = player.crafting.indices[i];
                if (0 <= itemIndex && itemIndex < player.inventory.slots.Count &&
                    player.inventory.slots[itemIndex].amount > 0)
                {
                    ItemSlot itemSlot = player.inventory.slots[itemIndex];
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }
                else
                {
                    player.crafting.indices[i] = -1;
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }
            List<int> validIndices = player.crafting.indices.Where(
                index => 0 <= index && index < player.inventory.slots.Count &&
                       player.inventory.slots[index].amount > 0
            ).ToList();
            List<ItemSlot> items = validIndices.Select(index => player.inventory.slots[index]).ToList();
            CraftingRecipe recipe = CraftingRecipe.Find(items);
            if (recipe != null)
            {
                Item item = new Item(recipe.result);
                resultSlotToolTip.enabled = true;
                if (resultSlotToolTip.IsVisible())
                    resultSlotToolTip.text = new ItemSlot(item).ToolTip(); 
                resultSlotImage.color = Color.white;
                resultSlotImage.sprite = recipe.result.image;
            }
            else
            {
                resultSlotToolTip.enabled = false;
                resultSlotImage.color = Color.clear;
                resultSlotImage.sprite = null;
            }
            if (player.crafting.craftingState == CraftingState.Success)
            {
                resultText.color = successColor;
                resultText.text = "Success!";
            }
            else if (player.crafting.craftingState == CraftingState.Failed)
            {
                resultText.color = failedColor;
                resultText.text = "Failed :(";
            }
            else
            {
                resultText.text = "";
            }
            craftButton.GetComponentInChildren<Text>().text = recipe != null &&
                                                              recipe.probability < 1 ? "Try Craft" : "Craft";
            craftButton.interactable = recipe != null &&
                                       player.crafting.craftingState != CraftingState.InProgress &&
                                       player.inventory.CanAdd(new Item(recipe.result), 1);
            craftButton.onClick.SetListener(() => {
                player.crafting.craftingState = CraftingState.InProgress; 
                player.crafting.CmdCraft(recipe.name, validIndices.ToArray());
            });
        }
    }
}
