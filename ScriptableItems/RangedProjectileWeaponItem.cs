
using UnityEngine;
using Mirror;
[CreateAssetMenu(menuName="uSurvival Item/Weapon(Ranged Projectile)", order=999)]
public class RangedProjectileWeaponItem : RangedWeaponItem
{
    [Header("Projectile")]
    public Projectile projectile; 
    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        if (projectile != null)
        {
            WeaponDetails details = GetWeaponDetails(player.equipment);
            if (details != null && details.muzzleLocation != null)
            {
                Vector3 spawnPosition = details.muzzleLocation.position;
                Quaternion spawnRotation = details.muzzleLocation.rotation;
                GameObject go = Instantiate(projectile.gameObject, spawnPosition, spawnRotation);
                Projectile proj = go.GetComponent<Projectile>();
                proj.owner = player.gameObject;
                proj.damage = damage;
                proj.direction = lookAt - spawnPosition;
                NetworkServer.Spawn(go);
            }
            else Debug.LogWarning("weapon details or muzzle location not found for player: " + player.name);
        }
        else Debug.LogWarning(name + ": missing projectile");
        base.UseHotbar(player, hotbarIndex, lookAt);
    }
}
