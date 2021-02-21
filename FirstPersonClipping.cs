
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class FirstPersonClipping : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public PlayerLook look;
    public PlayerEquipment equipment;
    [Header("Mesh Hiding")]
    public Transform[] hideRenderers; 
    Dictionary<Renderer, int> layerBackups = new Dictionary<Renderer, int>();
    public string hideInFirstPersonLayer = "HideInFirstPerson"; 
    [Header("Disable Depth Check (to avoid clipping)")]
    public string noDepthLayer = "NoDepthInFirstPerson";
    public Renderer[] disableArmsDepthCheck;
    Camera weaponCamera;
    public override void OnStartLocalPlayer()
    {
        foreach (Transform t in Camera.main.transform)
            if (t.CompareTag("WeaponCamera"))
                weaponCamera = t.GetComponent<Camera>();
    }
    void HideMeshes(bool firstPerson)
    {
        int hiddenLayer = LayerMask.NameToLayer(hideInFirstPersonLayer);
        foreach (Transform tf in hideRenderers)
        {
            foreach (Renderer rend in tf.GetComponentsInChildren<Renderer>())
            {
                if (rend.gameObject.layer != hiddenLayer)
                    layerBackups[rend] = rend.gameObject.layer;
                rend.gameObject.layer = firstPerson ? hiddenLayer : layerBackups[rend];
            }
        }
    }
    void DisableDepthCheck(bool firstPerson)
    {
        if (weaponCamera != null)
            weaponCamera.enabled = firstPerson;
        int noDepth = LayerMask.NameToLayer(noDepthLayer);
        foreach (Renderer renderer in disableArmsDepthCheck)
            renderer.gameObject.layer = noDepth;
        foreach (Renderer renderer in equipment.weaponMount.GetComponentsInChildren<Renderer>())
            renderer.gameObject.layer = noDepth;
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        bool firstPerson = look.InFirstPerson();
        HideMeshes(firstPerson);
        DisableDepthCheck(firstPerson);
    }
}
