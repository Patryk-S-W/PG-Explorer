using UnityEngine;
using Mirror;
public class Zoom : NetworkBehaviourNonAlloc
{
    public PlayerHotbar hotbar;
    Camera[] cameras;
    float defaultFieldOfView;
    void Awake()
    {
        cameras = Camera.main.GetComponentsInChildren<Camera>();
        defaultFieldOfView = cameras[0].fieldOfView;
    }
    void AssignFieldOfView(float value)
    {
        foreach (Camera cam in cameras)
            cam.fieldOfView = value;
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        UsableItem itemData = hotbar.GetCurrentUsableItemOrHands();
        if (Input.GetMouseButton(1) && itemData is RangedWeaponItem)
        {
            AssignFieldOfView(defaultFieldOfView - ((RangedWeaponItem)itemData).zoom);
        }
        else AssignFieldOfView(defaultFieldOfView);
    }
}
