
using UnityEngine;
using Mirror;
[DisallowMultipleComponent]
[RequireComponent(typeof(Inventory))]
public class PlayerInteraction : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public Player player;
    [Header("Interaction")]
    public float range = 3;
    public KeyCode key = KeyCode.F;
    [HideInInspector] public Interactable current;
    Interactable RaycastFindInteractable(Vector3 direction)
    {
        if (Utils.RaycastWithout(player.look.headPosition, direction, out RaycastHit hit, range, gameObject, player.look.raycastLayers) &&
            Vector3.Distance(player.look.headPosition, hit.transform.position) <= range)
            return hit.transform.GetComponent<Interactable>();
        return null;
    }
    
    [Command]
    public void CmdInteract(Vector3 lookAt)
    {
        if (player.health.current > 0)
        {
            Vector3 direction = lookAt - player.look.headPosition;
            Interactable interactable = RaycastFindInteractable(direction);
            if (interactable != null)
                interactable.OnInteractServer(player);
        }
    }
    [ClientCallback]
    void Update()
    {
        if (!isLocalPlayer) return;
        if (!Utils.IsCursorOverUserInterface() && Input.touchCount <= 1)
        {
            Vector3 direction = player.look.lookDirectionRaycasted;
            current = RaycastFindInteractable(direction);
            if (current != null && Input.GetKeyDown(key))
            {
                current.OnInteractClient(player);
                CmdInteract(player.look.lookPositionRaycasted);
            }
        }
    }
}
