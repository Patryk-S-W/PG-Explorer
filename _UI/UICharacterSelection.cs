
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class UICharacterSelection : MonoBehaviour
{
    public UICharacterCreation uiCharacterCreation;
    public UIConfirmation uiConfirmation;
    public NetworkManagerSurvival manager; 
    public GameObject panel;
    public UICharacterSelectionSlot slotPrefab;
    public Transform content;
    public Button createButton;
    public Button quitButton;
    void Update()
    {
        if (manager.state == NetworkState.Lobby && !uiCharacterCreation.IsVisible())
        {
            panel.SetActive(true);
            if (manager.charactersAvailableMsg != null)
            {
                CharactersAvailableMsg.CharacterPreview[] characters = manager.charactersAvailableMsg.characters;
                UIUtils.BalancePrefabs(slotPrefab.gameObject, characters.Length, content);
                for (int i = 0; i < characters.Length; ++i)
                {
                    GameObject prefab = manager.playerClasses.Find(p => p.name == characters[i].className);
                    UICharacterSelectionSlot slot = content.GetChild(i).GetComponent<UICharacterSelectionSlot>();
                    slot.nameText.text = characters[i].name;
                    slot.image.sprite = prefab.GetComponent<Player>().classIcon;
                    
                    
                    slot.selectButton.interactable = ClientScene.localPlayer == null;
                    int icopy = i; 
                    slot.selectButton.onClick.SetListener(() => {
                        ClientScene.Ready(NetworkClient.connection);
                        NetworkClient.connection.Send(new CharacterSelectMsg{ value=icopy });
                        panel.SetActive(false);
                    });
                    slot.deleteButton.onClick.SetListener(() => {
                        uiConfirmation.Show(
                            "Na pewno chcesz usunąć <b>" + characters[icopy].name + "</b>?",
                            () => { NetworkClient.Send(new CharacterDeleteMsg{value=icopy}); }
                        );
                    });
                }
                createButton.interactable = characters.Length < manager.characterLimit;
                createButton.onClick.SetListener(() => {
                    panel.SetActive(false);
                    uiCharacterCreation.Show();
                });
                quitButton.onClick.SetListener(() => { NetworkManagerSurvival.Quit(); });
            }
        }
        else panel.SetActive(false);
    }
}
