
using System;
using UnityEngine;
[Serializable]
public class ShoulderInfo
{
    public Transform bone;
    [HideInInspector] public Quaternion initialRotation;
    public Vector3 rotationOffset; 
}
public class ShouldersLookAt : MonoBehaviour
{
    [Header("Components")]
    public PlayerHotbar hotbar;
    public PlayerLook look;
    [Header("Shoulders/Arms")]
    public ShoulderInfo leftShoulder;
    public ShoulderInfo rightShoulder;
    void Awake()
    {
        Quaternion backup = transform.rotation;
        transform.rotation = Quaternion.identity;
        leftShoulder.initialRotation = leftShoulder.bone.rotation;
        rightShoulder.initialRotation = rightShoulder.bone.rotation;
        transform.rotation = backup;
    }
    void AdjustShoulder(ShoulderInfo shoulder)
    {
        Quaternion lookRotation = Quaternion.LookRotation(look.lookPositionFar - shoulder.bone.position);
        shoulder.bone.rotation = lookRotation * shoulder.initialRotation * Quaternion.Euler(shoulder.rotationOffset);
    }
    void LateUpdate()
    {
        if (look.IsFreeLooking()) return;
        if (hotbar.GetCurrentUsableItemOrHands().shoulderLookAtWhileHolding)
        {
            AdjustShoulder(leftShoulder);
            AdjustShoulder(rightShoulder);
        }
    }
}
