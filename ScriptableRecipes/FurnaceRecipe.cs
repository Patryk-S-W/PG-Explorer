
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName="New Recipe", menuName="uSurvival Recipe/Furnace", order=999)]
public class FurnaceRecipe : ScriptableRecipe
{
    public ScriptableItem ingredient;
    public ScriptableItem fuel;
    [Tooltip("The baking time in seconds.")]
    public float bakingTime = 5;
    public bool CanCraftWith(ItemSlot ingredientSlot, ItemSlot fuelSlot)
    {
        return ingredientSlot.amount > 0 &&
               ingredientSlot.item.data == ingredient &&
               fuelSlot.amount > 0 &&
               fuelSlot.item.data == fuel;
    }
    
    static Dictionary<string, FurnaceRecipe> cacheFurnace = null;
    public static Dictionary<string, FurnaceRecipe> dictFurnace
    {
        get
        {
            if (cacheFurnace == null)
            {
                FurnaceRecipe[] recipes = Resources.LoadAll<FurnaceRecipe>("");
                List<string> duplicates = recipes.ToList().FindDuplicates(recipe => recipe.name);
                if (duplicates.Count == 0)
                {
                    cacheFurnace = recipes.ToDictionary(recipe => recipe.name, recipe => recipe);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple FurnaceRecipes with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cacheFurnace;
        }
    }
    public static FurnaceRecipe Find(ItemSlot ingredientSlot, ItemSlot fuelSlot)
    {
        foreach (FurnaceRecipe recipe in dictFurnace.Values)
            if (recipe.CanCraftWith(ingredientSlot, fuelSlot))
                return recipe;
        return null;
    }
}
