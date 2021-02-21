using UnityEngine;
using Mirror;
public class DropRandomItemsOnDeath : NetworkBehaviourNonAlloc
{
    public ItemDropChance[] dropChances;
    public float radiusMultiplier = 1;
    public int dropSolverAttempts = 3; 
    [Server]
    void DropItemAtRandomPosition(GameObject dropPrefab)
    {
        Vector3 position = Utils.ReachableRandomUnitCircleOnNavMesh(transform.position, radiusMultiplier, dropSolverAttempts);
        GameObject drop = Instantiate(dropPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(drop);
    }
    [Server]
    public void OnDeath()
    {
        foreach (ItemDropChance itemChance in dropChances)
            if (Random.value <= itemChance.probability)
                DropItemAtRandomPosition(itemChance.drop.gameObject);
    }
}
