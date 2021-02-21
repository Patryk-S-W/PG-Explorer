using UnityEngine;
using UnityEngine.UI;
public class UIChat : MonoBehaviour
{
    public static UIChat singleton;
    public GameObject panel;
    public InputField messageInput;
    public Transform content;
    public ScrollRect scrollRect;
    public KeyCode[] activationKeys = {KeyCode.Return, KeyCode.KeypadEnter};
    public int keepHistory = 100; 
    bool eatActivation;
    public UIChat()
    {
        if (singleton == null) singleton = this;
    }
    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);
            messageInput.characterLimit = player.chat.maxLength;
            if (Utils.AnyKeyDown(activationKeys) && !eatActivation)
                messageInput.Select();
            eatActivation = false;
            messageInput.onEndEdit.SetListener((value) => {
                if (Utils.AnyKeyDown(activationKeys))
                {
                    string newinput = player.chat.OnSubmit(value);
                    messageInput.text = newinput;
                    messageInput.MoveTextEnd(false);
                    eatActivation = true;
                }
                UIUtils.DeselectCarefully();
            });
        }
        else panel.SetActive(false);
    }
    void AutoScroll()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }
    public void AddMessage(ChatMessage message)
    {
        if (content.childCount >= keepHistory)
            Destroy(content.GetChild(0).gameObject);
        GameObject go = Instantiate(message.textPrefab, content.transform, false);
        go.GetComponent<Text>().text = message.Construct();
        go.GetComponent<UIChatEntry>().message = message;
        AutoScroll();
    }
    public void OnEntryClicked(UIChatEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.message.replyPrefix))
        {
            messageInput.text = entry.message.replyPrefix;
            messageInput.Select();
            Invoke(nameof(MoveTextEnd), 0.1f);
        }
    }
    void MoveTextEnd()
    {
        messageInput.MoveTextEnd(false);
    }
}
