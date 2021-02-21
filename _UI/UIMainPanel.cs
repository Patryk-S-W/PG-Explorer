using UnityEngine;
using UnityEngine.UI;
public class UIMainPanel : MonoBehaviour
{
    public static UIMainPanel singleton;
    public KeyCode hotKey = KeyCode.Tab;
    public GameObject panel;
    public Button quitButton;
    public UIMainPanel()
    {
        if (singleton == null) singleton = this;
    }
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);
            string quitPrefix = "";
            if (player.remainingLogoutTime > 0)
                quitPrefix = "(" + Mathf.CeilToInt((float)player.remainingLogoutTime) + ") ";
            quitButton.GetComponent<UIShowToolTip>().text = quitPrefix + "Quit";
            quitButton.interactable = player.remainingLogoutTime == 0;
            quitButton.onClick.SetListener(NetworkManagerSurvival.Quit);
        }
        else panel.SetActive(false);
    }
    public void Show()
    {
        panel.SetActive(true);
    }
}
