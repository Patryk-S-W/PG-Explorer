using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
public class UICharacterCreation : MonoBehaviour
{
    public NetworkManagerSurvival manager; 
    public GameObject panel;
    public InputField nameInput;
    public Dropdown classDropdown;
    public Button createButton;
    public Button cancelButton;
    void Update()
    {
        if (panel.activeSelf)
        {
            if (manager.state == NetworkState.Lobby)
            {
                Show();
                createButton.interactable = manager.IsAllowedCharacterName(nameInput.text);
                createButton.onClick.SetListener(() => {
                    CharacterCreateMsg message = new CharacterCreateMsg{
                        name = nameInput.text
                    };
                    NetworkClient.Send(message);
                    Hide();
                });
                cancelButton.onClick.SetListener(() => {
                    nameInput.text = "";
                    Hide();
                });
            }
            else Hide();
        }
        else Hide();
    }
    public void Hide() { panel.SetActive(false); }
    public void Show() { panel.SetActive(true); }
    public bool IsVisible() { return panel.activeSelf; }
}
