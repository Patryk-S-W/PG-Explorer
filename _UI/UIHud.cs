using UnityEngine;
using UnityEngine.UI;
public class UIHud : MonoBehaviour
{
    public GameObject panel;
    public Slider healthSlider;
    public Text healthStatus;
    public Text ammoText;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);
            healthSlider.value = player.health.Percent();
            healthStatus.text = player.health.current + " / " + player.health.max;
            ItemSlot slot = player.hotbar.slots[player.hotbar.selection];
            if (slot.amount > 0 && slot.item.data is RangedWeaponItem itemData)
            {
                if (itemData.requiredAmmo != null)
                {
                    ammoText.text = slot.item.ammo + " / " + itemData.magazineSize;
                }
                else ammoText.text = "0 / 0";
            }
            else ammoText.text = "0 / 0";
        }
        else panel.SetActive(false);
    }
}