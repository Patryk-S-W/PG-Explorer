using UnityEngine;
using Mirror;
public class Door : NetworkBehaviourNonAlloc, Interactable
{
    public Animator animator;
    [SyncVar] public bool open;
    void Update()
    {
        animator.SetBool("Open", open);
    }
    public string GetInteractionText()
    {
        return (open ? "Close" : "Open") + " door";
    }
    [Client]
    public void OnInteractClient(Player player) {}
    [Server]
    public void OnInteractServer(Player player)
    {
        open = !open;
    }
    void OnValidate()
    {
        if (animator != null &&
            animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
        {
            Debug.LogWarning(name + " animator cull mode needs to be set to Always, otherwise the door collider won't move in host or server-only mode.");
        }
    }
}
