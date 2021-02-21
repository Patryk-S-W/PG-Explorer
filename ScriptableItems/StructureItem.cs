using System;
using UnityEngine;
using Mirror;
[Serializable]
public struct CustomRotation
{
    public Vector3 positionOffset;
    public Vector3 rotation;
}
[CreateAssetMenu(menuName="uSurvival Item/Structure", order=999)]
public class StructureItem : UsableItem
{
    [Header("Structure")]
    public GameObject structurePrefab;
    public float previewDistance = 2;
    [Range(1, 10)] public int gridResolution = 1;
    public CustomRotation[] availableRotations = { new CustomRotation() };
    [Range(0, 1)] public float buildToleranceCollision = 0.1f;
    [Range(0, 1)] public float buildToleranceAir = 0.1f;
    public override Usability CanUseInventory(Player player, int inventoryIndex) { return Usability.Never; }
    public override Usability CanUseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        Usability baseUsable = base.CanUseHotbar(player, hotbarIndex, lookAt);
        if (baseUsable != Usability.Usable)
            return baseUsable;
        Vector3 lookDirection = (lookAt - player.look.headPosition).normalized;
        Vector3 position = player.construction.CalculatePreviewPosition(this, player.look.headPosition, lookDirection);
        Quaternion rotation = player.construction.CalculatePreviewRotation(this);
        /*
            Bounds bounds = new Bounds();
            Bounds originalBounds = structurePrefab.GetComponentInChildren<Renderer>().bounds;
            Vector3 p0 = new Vector3(originalBounds.center.x - bounds.size.x,
                                     originalBounds.center.y - bounds.size.y,
                                     originalBounds.center.z - bounds.size.z);
            Vector3 p1 = new Vector3(originalBounds.center.x + bounds.size.x,
                                     originalBounds.center.y - bounds.size.y,
                                     originalBounds.center.z - bounds.size.z);
            Vector3 p2 = new Vector3(originalBounds.center.x - bounds.size.x,
                                     originalBounds.center.y + bounds.size.y,
                                     originalBounds.center.z - bounds.size.z);
            Vector3 p3 = new Vector3(originalBounds.center.x - bounds.size.x,
                                     originalBounds.center.y - bounds.size.y,
                                     originalBounds.center.z + bounds.size.z);
            Vector3 p4 = new Vector3(originalBounds.center.x + bounds.size.x,
                                     originalBounds.center.y + bounds.size.y,
                                     originalBounds.center.z - bounds.size.z);
            Vector3 p5 = new Vector3(originalBounds.center.x + bounds.size.x,
                                     originalBounds.center.y - bounds.size.y,
                                     originalBounds.center.z + bounds.size.z);
            Vector3 p6 = new Vector3(originalBounds.center.x - bounds.size.x,
                                     originalBounds.center.y + bounds.size.y,
                                     originalBounds.center.z + bounds.size.z);
            Vector3 p7 = new Vector3(originalBounds.center.x + bounds.size.x,
                                     originalBounds.center.y + bounds.size.y,
                                     originalBounds.center.z + bounds.size.z);
            bounds.Encapsulate(position + rotation * p0);
            bounds.Encapsulate(position + rotation * p1);
            bounds.Encapsulate(position + rotation * p2);
            bounds.Encapsulate(position + rotation * p3);
            bounds.Encapsulate(position + rotation * p4);
            bounds.Encapsulate(position + rotation * p5);
            bounds.Encapsulate(position + rotation * p6);
            bounds.Encapsulate(position + rotation * p7);
        */
        Vector3 prefabPosition = structurePrefab.transform.position;
        Quaternion prefabRotation = structurePrefab.transform.rotation;
          structurePrefab.transform.position = position;
          structurePrefab.transform.rotation = rotation;
        Bounds bounds = structurePrefab.GetComponentInChildren<Renderer>().bounds;
          structurePrefab.transform.position = prefabPosition;
          structurePrefab.transform.rotation = prefabRotation;
        return CanBuildThere(player.look.headPosition, bounds, player.look.raycastLayers)
               ? Usability.Usable
               : Usability.Empty; 
    }
    public override void UseInventory(Player player, int inventoryIndex) {}
    public override void UseHotbar(Player player, int hotbarIndex, Vector3 lookAt)
    {
        base.UseHotbar(player, hotbarIndex, lookAt);
        Vector3 lookDirection = (lookAt - player.look.headPosition).normalized;
        Vector3 position = player.construction.CalculatePreviewPosition(this, player.look.headPosition, lookDirection);
        Quaternion rotation = player.construction.CalculatePreviewRotation(this);
        GameObject go = Instantiate(structurePrefab, position, rotation);
        go.name = structurePrefab.name; 
        NetworkServer.Spawn(go);
        ItemSlot slot = player.hotbar.slots[hotbarIndex];
        slot.DecreaseAmount(1);
        player.hotbar.slots[hotbarIndex] = slot;
    }
    public virtual bool CanBuildThere(Vector3 headPosition, Bounds bounds, LayerMask raycastLayers)
    {
        if (Physics.CheckBox(bounds.center, bounds.extents * (1 - buildToleranceCollision), Quaternion.identity, raycastLayers))
            return false;
        if (!Physics.CheckBox(bounds.center, bounds.extents * (1 + buildToleranceAir), Quaternion.identity, raycastLayers))
            return false;
        return !Physics.Linecast(headPosition, bounds.center, raycastLayers);
    }
    protected override void OnValidate()
    {
        base.OnValidate();
        if (availableRotations.Length == 0)
            availableRotations = new CustomRotation[]{ new CustomRotation() };
    }
}
