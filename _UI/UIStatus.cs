
using UnityEngine;
using UnityEngine.UI;
public class UIStatus : MonoBehaviour
{
    public GameObject panel;
    public Slider healthSlider;
    public Text healthStatus;
    public Text damageText;
    public Text defenseText;
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            healthSlider.value = player.health.Percent();
            healthStatus.text = player.health.current + " / " + player.health.max;
            damageText.text = player.combat.damage.ToString();
            defenseText.text = player.combat.defense.ToString();
        }
    }
}
