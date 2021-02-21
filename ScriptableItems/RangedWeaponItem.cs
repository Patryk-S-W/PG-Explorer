
using System.Text;
using UnityEngine;
using Mirror;
public abstract class RangedWeaponItem : WeaponItem
{
    public AmmoItem requiredAmmo;
    public int magazineSize = 20;
    public float reloadTime = 1;
    public AudioClip reloadSound;
    public float zoom = 20;
    public GameObject decalPrefab;
    public float decalOffset = 0.01f;
    [Header("Recoil")]
    [Range(0, 30)] public float recoilHorizontal;
    [Range(0, 30)] public float recoilVertical;
    public override Usability CanUseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        Usability baseUsable = base.CanUseHotbar(player, hotbarIndex, lookAt);
        if (baseUsable != Usability.Usable)
            return baseUsable;
        if (player.reloading.ReloadTimeRemaining() > 0)
            return Usability.Cooldown;
        if (requiredAmmo != null && player.hotbar.slots[hotbarIndex].item.ammo == 0)
            return Usability.Empty;
        return Usability.Usable;
    }
    protected WeaponDetails GetWeaponDetails(PlayerEquipment equipment)
    {
        if (equipment.weaponMount != null && equipment.weaponMount.childCount > 0)
            return equipment.weaponMount.GetChild(0).GetComponentInChildren<WeaponDetails>();
        return null;
    }
    protected void ShowMuzzleFlash(PlayerEquipment equipment)
    {
        WeaponDetails details = GetWeaponDetails(equipment);
        if (details != null)
        {
            if (details.muzzleFlash != null) details.muzzleFlash.Fire();
        }
        else Debug.LogWarning("weapon details not found for player: " + equipment.name);
    }
    protected bool RaycastToLookAt(Player player, Vector3 lookAt, out RaycastHit hit)
    {
        Transform head = player.animator.GetBoneTransform(HumanBodyBones.Head);
        Vector3 direction = lookAt - head.position;
        Debug.DrawLine(head.position, direction, Color.yellow, 1);
        if (Utils.RaycastWithout(head.position, direction, out hit, attackRange, player.gameObject, player.look.raycastLayers))
        {
            Debug.DrawLine(head.position, hit.point, Color.red, 1);
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 1);
            return true;
        }
        hit = new RaycastHit();
        return false;
    }
    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        base.UseHotbar(player, hotbarIndex, lookAt);
        ItemSlot slot = player.hotbar.slots[hotbarIndex];
        if (requiredAmmo != null)
        {
            --slot.item.ammo;
            player.hotbar.slots[hotbarIndex] = slot;
        }
        slot.item.durability = Mathf.Max(slot.item.durability - 1, 0);
        player.hotbar.slots[hotbarIndex] = slot;
    }
    public override void OnUsedHotbar(Player player, Vector3 lookAt)
    {
        if (successfulUseSound) player.audioSource.PlayOneShot(successfulUseSound);
        ShowMuzzleFlash(player.equipment);
        if (decalPrefab != null &&
            RaycastToLookAt(player, lookAt, out RaycastHit hit) &&
            !hit.transform.GetComponent<Health>())
        {
            GameObject go = Instantiate(decalPrefab, hit.point + hit.normal * decalOffset, Quaternion.LookRotation(-hit.normal));
            
            go.transform.parent = hit.collider.transform;
        }
        if (player.isLocalPlayer)
        {
            float horizontal = Random.Range(-recoilHorizontal / 2, recoilHorizontal / 2);
            float vertical = Random.Range(0, recoilVertical);
            player.transform.Rotate(new Vector3(0, horizontal, 0));
            Camera.main.transform.Rotate(new Vector3(-vertical, 0, 0));
        }
    }
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{REQUIREDAMMO}", requiredAmmo != null ? requiredAmmo.name : "");
        tip.Replace("{MAGAZINESIZE}", magazineSize.ToString());
        return tip.ToString();
    }
}
