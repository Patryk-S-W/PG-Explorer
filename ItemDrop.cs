using System.Collections.Generic;
using UnityEngine;
using Mirror;
[RequireComponent(typeof(NetworkProximityGridChecker))] 
[RequireComponent(typeof(Collider))] 
public class ItemDrop : NetworkBehaviourNonAlloc, Interactable
{
    [Header("Components")]
    public NetworkProximityGridChecker proximityChecker;
    [Header("Item")]
#pragma warning disable CS0649 
    [SerializeField] ScriptableItem itemData; 
#pragma warning restore CS0649 
    [SyncVar] public int amount = 1; 
    [SyncVar, HideInInspector] public Item item;
    [Header("Item Spawning")]
    public bool respawn; 
    public float respawnInterval;
    Collider[] colliders;
    void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }
    public override void OnStartServer()
    {
        if (item.hash == 0 && itemData != null)
            item = new Item(itemData);
    }
    public string GetInteractionText()
    {
        if (Player.localPlayer != null && itemData != null && amount > 0)
            return amount > 1 ? item.name + " x " + amount : item.name;
        return "";
    }
    [Client]
    public void OnInteractClient(Player player) {}
    [Server]
    public void OnInteractServer(Player player)
    {
        if (proximityChecker.forceHidden)
            return;
        if (amount > 0 && player.inventory.Add(item, amount))
        {
            if (respawn)
            {
                
                Disappear();
            }
            else
            {
                amount = 0;
                NetworkServer.Destroy(gameObject);
            }
        }
    }
    [Server]
    void Disappear()
    {
        foreach (Collider co in colliders)
            co.enabled = false;
        proximityChecker.forceHidden = true;
        netIdentity.RebuildObservers(false);
        Invoke(nameof(Reappear), respawnInterval);
    }
    [Server]
    void Reappear()
    {
        foreach (Collider co in colliders)
            co.enabled = true;
        proximityChecker.forceHidden = false;
    }
}
