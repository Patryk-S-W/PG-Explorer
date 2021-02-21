﻿
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class IKHandling : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public PlayerLook look;
    [Header("Right Hand Target")]
    public Transform rightHandTarget;
    Vector3 rightHandTargetTruePosition;
    Quaternion rightHandTargetTrueRotation;
    [Range(0, 1)] public float rightHandWeight = 1;
    public Transform rightElbowTarget;
    Vector3 rightElbowTargetTruePosition;
    [Range(0, 1)] public float rightElbowWeight = 1;
    [Header("Left Hand Target")]
    public Transform leftHandTarget;
    Vector3 leftHandTargetTruePosition;
    Quaternion leftHandTargetTrueRotation;
    [Range(0, 1)] public float leftHandWeight = 1;
    public Transform leftElbowTarget;
    Vector3 leftElbowTargetTruePosition;
    [Range(0, 1)] public float leftElbowWeight = 1;
    [Header("LookAt Target")]
    public bool lookAtCameraDirection = true;
    [Range(0, 1)] public float lookAtWeight = 1;
    [Range(0, 1)] public float lookAtBodyWeight = 1;
    public bool lookAtBodyWeightActive = true;
    [Range(0, 1)] public float lookAtHeadWeight = 1;
    public bool lookAtHeadWeightActive = true;
    [Range(0, 1)] public float lookAtEyesWeight = 0;
    public bool lookAtEyesWeightActive = true;
    [Range(0, 1)] public float lookAtClampWeight = 0.3f;
    public bool lookAtClampWeightActive = true;
    void Update()
    {
        if (rightHandTarget != null)
        {
            rightHandTargetTruePosition = rightHandTarget.position;
            rightHandTargetTrueRotation = rightHandTarget.rotation;
        }
        if (rightElbowTarget != null)
        {
            rightElbowTargetTruePosition = rightElbowTarget.position;
        }
        if (leftHandTarget != null)
        {
            leftHandTargetTruePosition = leftHandTarget.position;
            leftHandTargetTrueRotation = leftHandTarget.rotation;
        }
        if (leftElbowTarget != null)
        {
            leftElbowTargetTruePosition = leftElbowTarget.position;
        }
    }
    void OnAnimatorIK(int layerIndex)
    {
        if(lookAtCameraDirection)
        {
            animator.SetLookAtWeight(lookAtWeight,
                                     lookAtBodyWeightActive ? lookAtBodyWeight : 0,
                                     lookAtHeadWeightActive ? lookAtHeadWeight : 0,
                                     lookAtEyesWeightActive ? lookAtEyesWeight : 0,
                                     lookAtClampWeightActive ? lookAtClampWeight : 1);
            animator.SetLookAtPosition(look.lookPositionFar);
        }
        else
        {
            animator.SetLookAtWeight(0);
        }
        if(rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTargetTruePosition);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTargetTrueRotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }
        if (rightElbowTarget != null)
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightElbowWeight);
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowTargetTruePosition);
        }
        else
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
        }
        if(leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTargetTruePosition);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTargetTrueRotation);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
        if (leftElbowTarget != null)
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftElbowWeight);
            animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowTargetTruePosition);
        }
        else
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
        }
    }
}