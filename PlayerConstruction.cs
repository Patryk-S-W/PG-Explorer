
using UnityEngine;
using Mirror;
public class PlayerConstruction : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public PlayerHotbar hotbar;
    public PlayerLook look;
    [Header("Configuration")]
    public KeyCode rotationKey = KeyCode.R;
    public Color canBuildColor = Color.cyan;
    public Color cantBuildColor = Color.red;
    [HideInInspector] public GameObject preview;
    [SyncVar] int rotationIndex;
    public StructureItem GetCurrentStructure()
    {
        UsableItem itemData = hotbar.GetCurrentUsableItemOrHands();
        return itemData is StructureItem ? (StructureItem)itemData : null;
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        StructureItem structure = GetCurrentStructure();
        if (structure != null)
        {
            if (preview != null && preview.name != structure.structurePrefab.name)
                Destroy(preview);
            if (preview == null || preview.name != structure.structurePrefab.name)
            {
                preview = Instantiate(structure.structurePrefab,
                                      CalculatePreviewPosition(structure, look.headPosition, look.lookDirectionRaycasted),
                                      CalculatePreviewRotation(structure));
                preview.name = structure.structurePrefab.name;
                
                Behaviour[] behaviours = preview.GetComponentsInChildren<Behaviour>();
                for (int i = behaviours.Length - 1; i >= 0; --i)
                    if (!(behaviours[i] is NetworkIdentity))
                        Destroy(behaviours[i]);
                foreach (Collider co in preview.GetComponentsInChildren<Collider>())
                    Destroy(co);
            }
            preview.transform.position = CalculatePreviewPosition(structure, look.headPosition, look.lookDirectionRaycasted);
            preview.transform.rotation = CalculatePreviewRotation(structure);
            if (Input.GetKeyDown(rotationKey))
            {
                int newIndex = (rotationIndex + 1) % structure.availableRotations.Length;
                CmdSetRotationIndex(newIndex);
            }
            Bounds bounds = preview.GetComponentInChildren<Renderer>().bounds;
            bool canBuild = structure.CanBuildThere(look.headPosition, bounds, look.raycastLayers);
            foreach (Renderer renderer in preview.GetComponentsInChildren<Renderer>())
                renderer.material.color = canBuild ? canBuildColor : cantBuildColor;
        }
        else if (preview != null) Destroy(preview);
    }
    [Command]
    void CmdSetRotationIndex(int index)
    {
        rotationIndex = index;
    }
    static float RoundToGrid(float value, int resolution)
    {
        return Mathf.Round(value * resolution) / resolution;
    }
    public Vector3 CalculatePreviewPosition(StructureItem structure, Vector3 headPosition, Vector3 lookDirection)
    {
        Vector3 inFront = headPosition + lookDirection * structure.previewDistance;
        inFront.x = RoundToGrid(inFront.x, structure.gridResolution);
        inFront.y = RoundToGrid(inFront.y, structure.gridResolution);
        inFront.z = RoundToGrid(inFront.z, structure.gridResolution);
        rotationIndex = rotationIndex % structure.availableRotations.Length;
        Vector3 offset = structure.availableRotations[rotationIndex].positionOffset;
        return inFront + offset;
    }
    public Quaternion CalculatePreviewRotation(StructureItem structure)
    {
        rotationIndex = rotationIndex % structure.availableRotations.Length;
        Vector3 euler = structure.availableRotations[rotationIndex].rotation;
        return Quaternion.Euler(euler);
    }
    void OnDrawGizmos()
    {
        if (preview != null)
        {
            StructureItem structure = GetCurrentStructure();
            Bounds bounds = preview.GetComponentInChildren<Renderer>().bounds;
            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(bounds.center, bounds.size * (1 - structure.buildToleranceCollision));
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size * (1 + structure.buildToleranceAir));
        }
    }
}
