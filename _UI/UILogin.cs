
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class UILogin : MonoBehaviour
{
    public UIPopup uiPopup;
    public NetworkManagerSurvival manager; 
    public NetworkAuthenticatorSurvival auth;
    public GameObject panel;
    public Text statusText;
    public InputField accountInput;
    public InputField passwordInput;
    public Dropdown serverDropdown;
    public Button loginButton;
    public Button registerButton;
    [TextArea(1, 30)] public string registerMessage = "Zaloguj się a konto zostanie utworzone automatycznie.";
    public Button hostButton;
    public Button dedicatedButton;
    public Button cancelButton;
    public Button quitButton;
    void Start()
    {
        if (PlayerPrefs.HasKey("LastServer"))
        {
            string last = PlayerPrefs.GetString("LastServer", "");
            serverDropdown.value = manager.serverList.FindIndex(s => s.name == last);
        }
    }
    void OnDestroy()
    {
        PlayerPrefs.SetString("LastServer", serverDropdown.captionText.text);
    }
    void Update()
    {
        if (manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake)
        {
            panel.SetActive(true);
            if (manager.IsConnecting())
                statusText.text = "Łączenie...";
            else if (manager.state == NetworkState.Handshake)
                statusText.text = "Handshake...";
            else
                statusText.text = "";
            registerButton.interactable = !manager.isNetworkActive;
            registerButton.onClick.SetListener(() => { uiPopup.Show(registerMessage); });
            loginButton.interactable = !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text);
            loginButton.onClick.SetListener(() => { manager.StartClient(); });
            hostButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive && auth.IsAllowedAccountName(accountInput.text);
            hostButton.onClick.SetListener(() => { manager.StartHost(); });
            cancelButton.gameObject.SetActive(manager.IsConnecting());
            cancelButton.onClick.SetListener(() => { manager.StopClient(); });
            dedicatedButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer && !manager.isNetworkActive;
            dedicatedButton.onClick.SetListener(() => { manager.StartServer(); });
            quitButton.onClick.SetListener(() => { NetworkManagerSurvival.Quit(); });
            auth.loginAccount = accountInput.text;
            auth.loginPassword = passwordInput.text;
            serverDropdown.interactable = !manager.isNetworkActive;
            serverDropdown.options = manager.serverList.Select(
                sv => new Dropdown.OptionData(sv.name)
            ).ToList();
            manager.networkAddress = manager.serverList[serverDropdown.value].ip;
        }
        else panel.SetActive(false);
    }
}
