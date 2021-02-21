
using UnityEngine;
[CreateAssetMenu(menuName="uSurvival Item/Weapon(Ranged Raycast)", order=999)]
public class RangedRaycastWeaponItem : RangedWeaponItem
{
    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        if (RaycastToLookAt(player, lookAt, out RaycastHit hit))
        {
            Entity victim = hit.transform.GetComponent<Entity>();
            if (victim)
            {
                player.combat.DealDamageAt(victim, damage, hit.point, hit.normal, hit.collider);
            }
        }
        base.UseHotbar(player, hotbarIndex, lookAt);
    }
}
