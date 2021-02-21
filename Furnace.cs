using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Furnace : NetworkBehaviourNonAlloc, Interactable
{
    [SyncVar, HideInInspector] public ItemSlot ingredientSlot;
    [SyncVar, HideInInspector] public ItemSlot fuelSlot;
    [SyncVar, HideInInspector] public ItemSlot resultSlot;
    ItemSlot lastIngredientSlot;
    ItemSlot lastFuelSlot;
    public static Dictionary<string, Furnace> furnaces = new Dictionary<string, Furnace>();
    public FurnaceRecipe currentBake;
    [SyncVar] public double bakeTimeStart; 
    [SyncVar] public double bakeTimeEnd;
    public override void OnStartServer()
    {
        if (!furnaces.ContainsKey(name))
        {
            furnaces[name] = this;
            Database.singleton.LoadFurnace(this);
        }
        else Debug.LogWarning("A Furnace with name " + name + " already exists. Use a different name for each Furnace, otherwise it won't be saved to the Database.");
    }
    void OnDestroy()
    {
        furnaces.Remove(name);
    }
    public string GetInteractionText() { return "Furnace"; }
    [Client]
    public void OnInteractClient(Player player)
    {
        UIMainPanel.singleton.Show();
    }
    [Server]
    public void OnInteractServer(Player player) {}
    public bool IsBaking() => NetworkTime.time < bakeTimeEnd;
    [Server]
    public bool StartBaking(FurnaceRecipe recipe)
    {
        if (resultSlot.amount == 0 ||
            (resultSlot.item.data == recipe.result &&
             resultSlot.amount < resultSlot.item.maxStack))
        {
            currentBake = recipe;
            bakeTimeStart = NetworkTime.time;
            bakeTimeEnd = NetworkTime.time + recipe.bakingTime;
            return true;
        }
        return false;
    }
    [Server]
    public void FinishBaking()
    {
        if (currentBake != null)
        {
            ingredientSlot.DecreaseAmount(1);
            fuelSlot.DecreaseAmount(1);
            if (resultSlot.amount > 0 && resultSlot.item.data == currentBake.result)
                resultSlot.IncreaseAmount(1);
            else
                resultSlot = new ItemSlot(new Item(currentBake.result), 1);
            currentBake = null;
        }
    }
    [Server]
    public void CancelBaking()
    {
        currentBake = null;
        bakeTimeEnd = NetworkTime.time;
    }
    [ServerCallback]
    void Update()
    {
        if (IsBaking())
        {
        }
        else if (currentBake != null)
        {
            FinishBaking();
        }
        else
        {
            if (ingredientSlot.amount > 0 &&
                fuelSlot.amount > 0)
            {
                if (!lastIngredientSlot.Equals(ingredientSlot) ||
                    !lastFuelSlot.Equals(fuelSlot))
                {
                    FurnaceRecipe recipe = FurnaceRecipe.Find(ingredientSlot, fuelSlot);
                    if (recipe != null)
                    {
                        StartBaking(recipe);
                    }
                }
            }
            lastIngredientSlot = ingredientSlot;
            lastFuelSlot = fuelSlot;
        }
    }
}
