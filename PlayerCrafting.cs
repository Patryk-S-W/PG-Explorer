
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public enum CraftingState : byte {None, InProgress, Success, Failed}
[DisallowMultipleComponent]
[RequireComponent(typeof(Inventory))]
public class PlayerCrafting : NetworkBehaviourNonAlloc
{
    public Health health;
    public Inventory inventory;
    public List<int> indices = Enumerable.Repeat(-1, CraftingRecipe.recipeSize).ToList();
    [HideInInspector] public CraftingState craftingState = CraftingState.None; 
    [Command]
    public void CmdCraft(string recipeName, int[] indices)
    {
        if (health.current > 0 &&
            0 < indices.Length && indices.Length <= CraftingRecipe.recipeSize &&
            indices.All(index => 0 <= index && index < inventory.slots.Count && inventory.slots[index].amount > 0) &&
            !indices.ToList().HasDuplicates())
        {
            if (CraftingRecipe.dictCrafting.TryGetValue(recipeName, out CraftingRecipe recipe) &&
                recipe.result != null)
            {
                Item result = new Item(recipe.result);
                if (inventory.CanAdd(result, 1))
                {
                    foreach (ScriptableItemAndAmount ingredient in recipe.ingredients)
                        if (ingredient.amount > 0 && ingredient.item != null)
                            inventory.Remove(new Item(ingredient.item), ingredient.amount);
                    
                    
                    if (new System.Random().NextDouble() < recipe.probability)
                    {
                        inventory.Add(result, 1);
                        TargetCraftingSuccess();
                    }
                    else
                    {
                        TargetCraftingFailed();
                    }
                }
            }
        }
    }
    [TargetRpc] 
    public void TargetCraftingSuccess()
    {
        craftingState = CraftingState.Success;
    }
    [TargetRpc] 
    public void TargetCraftingFailed()
    {
        craftingState = CraftingState.Failed;
    }
    void OnDragAndDrop_InventorySlot_CraftingIngredientSlot(int[] slotIndices)
    {
        if (craftingState != CraftingState.InProgress)
        {
            if (!indices.Contains(slotIndices[0]))
            {
                indices[slotIndices[1]] = slotIndices[0];
                craftingState = CraftingState.None; 
            }
        }
    }
    void OnDragAndDrop_CraftingIngredientSlot_CraftingIngredientSlot(int[] slotIndices)
    {
        if (craftingState != CraftingState.InProgress)
        {
            int temp = indices[slotIndices[0]];
            indices[slotIndices[0]] = indices[slotIndices[1]];
            indices[slotIndices[1]] = temp;
            craftingState = CraftingState.None; 
        }
    }
    void OnDragAndClear_CraftingIngredientSlot(int slotIndex)
    {
        if (craftingState != CraftingState.InProgress)
        {
            indices[slotIndex] = -1;
            craftingState = CraftingState.None; 
        }
    }
}