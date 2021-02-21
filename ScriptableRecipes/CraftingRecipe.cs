
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(fileName="New Recipe", menuName="uSurvival Recipe/Crafting", order=999)]
public class CraftingRecipe : ScriptableRecipe
{
    public static int recipeSize = 6;
    public List<ScriptableItemAndAmount> ingredients = new List<ScriptableItemAndAmount>(6);
    [Range(0, 1)] public float probability = 1;
    bool IngredientsNotEmpty()
    {
        foreach (ScriptableItemAndAmount slot in ingredients)
            if (slot.amount > 0 && slot.item != null)
                return true;
        return false;
    }
    int FindMatchingStack(List<ItemSlot> items, ScriptableItemAndAmount ingredient)
    {
        for (int i = 0; i < items.Count; ++i)
            if (items[i].amount >= ingredient.amount &&
                items[i].item.data == ingredient.item)
                return i;
        return -1;
    }
    public virtual bool CanCraftWith(List<ItemSlot> items)
    {
        List<ItemSlot> checkItems = new List<ItemSlot>();
        foreach (ItemSlot slot in items)
            if (slot.amount > 0)
                checkItems.Add(slot);
        if (IngredientsNotEmpty())
        {
            foreach (ScriptableItemAndAmount ingredient in ingredients)
            {
                if (ingredient.amount > 0 && ingredient.item != null)
                {
                    int index = FindMatchingStack(checkItems, ingredient);
                    if (index != -1)
                        checkItems.RemoveAt(index);
                    else
                        return false;
                }
            }
            return checkItems.Count == 0;
        }
        else return false;
    }
    
    static Dictionary<string, CraftingRecipe> cacheCrafting = null;
    public static Dictionary<string, CraftingRecipe> dictCrafting
    {
        get
        {
            if (cacheCrafting == null)
            {
                CraftingRecipe[] recipes = Resources.LoadAll<CraftingRecipe>("");
                List<string> duplicates = recipes.ToList().FindDuplicates(recipe => recipe.name);
                if (duplicates.Count == 0)
                {
                    cacheCrafting = recipes.ToDictionary(recipe => recipe.name, recipe => recipe);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple CraftingRecipes with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cacheCrafting;
        }
    }
    public static CraftingRecipe Find(List<ItemSlot> items)
    {
        foreach (CraftingRecipe recipe in dictCrafting.Values)
            if (recipe.CanCraftWith(items))
                return recipe;
        return null;
    }
    void OnValidate()
    {
        for (int i = ingredients.Count; i < recipeSize; ++i)
            ingredients.Add(new ScriptableItemAndAmount());
        for (int i = recipeSize; i < ingredients.Count; ++i)
            ingredients.RemoveAt(i);
        for (int i = 0; i < ingredients.Count; ++i)
        {
            ScriptableItemAndAmount ingredient = ingredients[i];
            if (ingredient.item != null)
            {
                ingredient.amount = Mathf.Clamp(ingredient.amount, 1, ingredient.item.maxStack);
                ingredients[i] = ingredient;
            }
        }
    }
}
