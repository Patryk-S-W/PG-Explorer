
using System.Text;
using UnityEngine;
public abstract class WeaponItem : UsableItem
{
    [Header("Weapon")]
    public float attackRange = 20; 
    public int damage = 10;
    public string upperBodyAnimationParameter;
    public override Usability CanUseInventory(Player player, int inventoryIndex) { return Usability.Never; }
    public override void UseInventory(Player player, int inventoryIndex) {}
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{ATTACKRANGE}", attackRange.ToString());
        tip.Replace("{DAMAGE}", damage.ToString());
        return tip.ToString();
    }
}
