
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Storage : Inventory, Interactable
{
    public int size = 22;
    public static Dictionary<string, Storage> storages = new Dictionary<string, Storage>();
    public override void OnStartServer()
    {
        if (!storages.ContainsKey(name))
        {
            storages[name] = this;
            Database.singleton.LoadStorage(this);
        }
        else Debug.LogWarning("A Storage with name " + name + " already exists. Use a different name for each Storage, otherwise it won't be saved to the Database.");
    }
    void OnDestroy()
    {
        storages.Remove(name);
    }
    public string GetInteractionText()
    {
        return "Open";
    }
    [Client]
    public void OnInteractClient(Player player)
    {
        UIMainPanel.singleton.Show();
    }
    [Server]
    public void OnInteractServer(Player player) {}
}
