
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public abstract class ScriptableRecipe : ScriptableObjectNonAlloc
{
    public ScriptableItem result;
    static Dictionary<string, ScriptableRecipe> cache = null;
    public static Dictionary<string, ScriptableRecipe> dict
    {
        get
        {
            if (cache == null)
            {
                ScriptableRecipe[] recipes = Resources.LoadAll<ScriptableRecipe>("");
                List<string> duplicates = recipes.ToList().FindDuplicates(recipe => recipe.name);
                if (duplicates.Count == 0)
                {
                    cache = recipes.ToDictionary(recipe => recipe.name, recipe => recipe);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableRecipes with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                }
            }
            return cache;
        }
    }
}
