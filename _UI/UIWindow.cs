
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public enum CloseOption
{
    DoNothing,
    DeactivateWindow,
    DestroyWindow
}
public class UIWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CloseOption onClose = CloseOption.DeactivateWindow;
    Transform window;
    void Awake()
    {
        window = transform.parent;
    }
    public void HandleDrag(PointerEventData d)
    {
        window.SendMessage("OnWindowDrag", d, SendMessageOptions.DontRequireReceiver);
        window.Translate(d.delta);
    }
    public void OnBeginDrag(PointerEventData d)
    {
        HandleDrag(d);
    }
    public void OnDrag(PointerEventData d)
    {
        HandleDrag(d);
    }
    public void OnEndDrag(PointerEventData d)
    {
        HandleDrag(d);
    }
    public void OnClose()
    {
        window.SendMessage("OnWindowClose", SendMessageOptions.DontRequireReceiver);
        if (onClose == CloseOption.DeactivateWindow)
            window.gameObject.SetActive(false);
        if (onClose == CloseOption.DestroyWindow)
            Destroy(window.gameObject);
    }
}
