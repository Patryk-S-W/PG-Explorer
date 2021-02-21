
using System.Text;
using UnityEngine;
using Mirror;
[CreateAssetMenu(menuName="uSurvival Item/Weapon(Melee)", order=999)]
public class MeleeWeaponItem : WeaponItem
{
    public float sphereCastRadius = 0.5f; 
    public override Usability CanUseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        Usability baseUsable = base.CanUseHotbar(player, hotbarIndex, lookAt);
        if (baseUsable != Usability.Usable)
            return baseUsable;
        return player.reloading.ReloadTimeRemaining() > 0
               ? Usability.Cooldown
               : Usability.Usable;
    }
    Entity SphereCastToLookAt(Player player, Collider collider, Vector3 lookAt, out RaycastHit hit)
    {
        
        Vector3 origin = collider.bounds.center;
        Vector3 behindOrigin = origin - player.transform.forward * sphereCastRadius;
        Vector3 direction = (lookAt - origin).normalized;
        Debug.DrawLine(behindOrigin, lookAt, Color.red, 1);
        if (Utils.SphereCastWithout(behindOrigin, sphereCastRadius, direction, out hit, attackRange + sphereCastRadius, player.gameObject, player.look.raycastLayers))
        {
            Debug.DrawLine(behindOrigin, hit.point, Color.cyan, 1);
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 1);
            return hit.transform.GetComponent<Entity>();
        }
        return null;
    }
    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        base.UseHotbar(player, hotbarIndex, lookAt);
        Entity victim = SphereCastToLookAt(player, player.collider, lookAt, out RaycastHit hit);
        if (victim != null)
        {
            player.combat.DealDamageAt(victim, player.combat.damage + damage, hit.point, hit.normal, hit.collider);
            ItemSlot slot = player.hotbar.slots[hotbarIndex];
            if (slot.amount > 0)
            {
                slot.item.durability = Mathf.Max(slot.item.durability - 1, 0);
                player.hotbar.slots[hotbarIndex] = slot;
            }
            else Debug.Log("ignore slot: " + hotbarIndex);
        }
    }
    public override void OnUsedHotbar(Player player, Vector3 lookAt)
    {
        Entity victim = SphereCastToLookAt(player, player.collider, lookAt, out RaycastHit hit);
        if (victim != null)
        {
            if (successfulUseSound)
                player.hotbar.audioSource.PlayOneShot(successfulUseSound);
        }
        else
        {
            if (failedUseSound)
                player.hotbar.audioSource.PlayOneShot(failedUseSound);
        }
    }
}
