
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Controller2k;
using Mirror;
public class SyncDictionaryIntDouble : SyncDictionary<int, double> {}
[Serializable] public class UnityEventPlayer : UnityEvent<Player> {}
public class Player : Entity
{
    [Header("Components")]
    public Animator animator;
    public AudioSource audioSource;
    public CharacterController2k controller;
    public PlayerChat chat;
    public PlayerConstruction construction;
    public PlayerCrafting crafting;
    public PlayerEquipment equipment;
    public PlayerFurnaceUsage furnaceUsage;
    public PlayerHeartbeat heartbeat;
    public PlayerHotbar hotbar;
    public PlayerInteraction interaction;
    public PlayerInventory inventory;
    public PlayerLook look;
    public PlayerMovement movement;
    public PlayerReloading reloading;
    public PlayerRespawning respawning;
    public PlayerStorageUsage storageUsage;
    [Header("Class")]
    public Sprite classIcon; 
    [HideInInspector] public string account = "";
    [HideInInspector] public string className = "";
    [Header("Animation")]
    public float animationDirectionDampening = 0.05f;
    public float animationTurnDampening = 0.1f;
    Vector3 lastForward;
    public double allowedLogoutTime => combat.lastCombatTime + ((NetworkManagerSurvival)NetworkManager.singleton).combatLogoutDelay;
    public double remainingLogoutTime => NetworkTime.time < allowedLogoutTime ? (allowedLogoutTime - NetworkTime.time) : 0;
    public static Dictionary<string, Player> onlinePlayers = new Dictionary<string, Player>();
    public static Player localPlayer;
    public bool isNonHostLocalPlayer => !isServer && isLocalPlayer;
    SyncDictionaryIntDouble itemCooldowns = new SyncDictionaryIntDouble();
    Dictionary<int, double> itemCooldownsPrediction = new Dictionary<int, double>();
    public override void OnStartLocalPlayer()
    {
        localPlayer = this;
    }
    public override void OnStartServer()
    {
        onlinePlayers[name] = this;
    }
    void Start()
    {
        lastForward = transform.forward;
    }
    void OnDestroy()
    {
        if (onlinePlayers.TryGetValue(name, out Player entry) && entry == this)
            onlinePlayers.Remove(name);
    }
    public float GetItemCooldown(string cooldownCategory)
    {
        int hash = cooldownCategory.GetStableHashCode();
        if (isNonHostLocalPlayer)
        {
            if (itemCooldownsPrediction.TryGetValue(hash, out double cooldownPredictionEnd))
            {
                return NetworkTime.time >= cooldownPredictionEnd ? 0 : (float)(cooldownPredictionEnd - NetworkTime.time);
            }
        }
        if (itemCooldowns.TryGetValue(hash, out double cooldownEnd))
        {
            return NetworkTime.time >= cooldownEnd ? 0 : (float)(cooldownEnd - NetworkTime.time);
        }
        return 0;
    }
    public void SetItemCooldown(string cooldownCategory, float cooldown)
    {
        int hash = cooldownCategory.GetStableHashCode();
        double cooldownEnd = NetworkTime.time + cooldown;
        if (isNonHostLocalPlayer)
            itemCooldownsPrediction[hash] = cooldownEnd;
        else
            itemCooldowns[hash] = cooldownEnd;
    }
    float GetAnimationJumpLeg()
    {
        return isLocalPlayer
            ? movement.jumpLeg
            : 1; 
    }
    
    static float AnimationDeltaUnclamped(Vector3 lastForward, Vector3 currentForward)
    {
        Quaternion rotationDelta = Quaternion.FromToRotation(lastForward, currentForward);
        float turnAngle = rotationDelta.eulerAngles.y;
        return turnAngle >= 180 ? turnAngle - 360 : turnAngle;
    }
    [ClientCallback] 
    void LateUpdate()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(movement.velocity);
        float jumpLeg = GetAnimationJumpLeg();
        float turnAngle = AnimationDeltaUnclamped(lastForward, transform.forward);
        lastForward = transform.forward;
        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.SetBool("DEAD", health.current == 0);
            animator.SetFloat("DirX", localVelocity.x, animationDirectionDampening, Time.deltaTime); 
            animator.SetFloat("DirY", localVelocity.y, animationDirectionDampening, Time.deltaTime); 
            animator.SetFloat("DirZ", localVelocity.z, animationDirectionDampening, Time.deltaTime); 
            animator.SetFloat("LastFallY", movement.lastFall.y);
            animator.SetFloat("Turn", turnAngle, animationTurnDampening, Time.deltaTime); 
            animator.SetBool("CROUCHING", movement.state == MoveState.CROUCHING);
            animator.SetBool("CRAWLING", movement.state == MoveState.CRAWLING);
            animator.SetBool("CLIMBING", movement.state == MoveState.CLIMBING);
            animator.SetBool("SWIMMING", movement.state == MoveState.SWIMMING);
            if (movement.state == MoveState.CLIMBING)
                animator.speed = localVelocity.y == 0 ? 0 : 1;
            else
                animator.speed = 1;
            
            animator.SetBool("OnGround", movement.state != MoveState.AIRBORNE);
            if (controller.isGrounded) animator.SetFloat("JumpLeg", jumpLeg);
            animator.SetBool("UPPERBODY_HANDS", hotbar.slots[hotbar.selection].amount == 0);
            animator.SetBool("UPPERBODY_RIFLE", false);
            animator.SetBool("UPPERBODY_PISTOL", false);
            animator.SetBool("UPPERBODY_AXE", false);
            if (movement.state != MoveState.CLIMBING && 
                hotbar.slots[hotbar.selection].amount > 0 &&
                hotbar.slots[hotbar.selection].item.data is WeaponItem)
            {
                WeaponItem weapon = (WeaponItem)hotbar.slots[hotbar.selection].item.data;
                if (!string.IsNullOrWhiteSpace(weapon.upperBodyAnimationParameter))
                    animator.SetBool(weapon.upperBodyAnimationParameter, true);
            }
        }
    }
}