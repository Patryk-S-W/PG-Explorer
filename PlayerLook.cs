using System;
using UnityEngine;
using Mirror;
public class PlayerLook : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public IKHandling ik;
    public PlayerMovement movement;
    public Health health;
#pragma warning disable CS0109 
    new Camera camera;
#pragma warning restore CS0109 
    float lastClientSendTime;
    [Header("Camera")]
    public float XSensitivity = 2;
    public float YSensitivity = 2;
    public float MinimumX = -90;
    public float MaximumX = 90;
    public Transform firstPersonParent;
    public Vector3 headPosition => firstPersonParent.position;
    public Transform freeLookParent;
    Vector3 originalCameraPosition;
    public KeyCode freeLookKey = KeyCode.LeftAlt;
    [SyncVar] bool syncedIsFreeLooking;
    public LayerMask viewBlockingLayers;
    public float zoomSpeed = 0.5f;
    public float distance = 0;
    public float minDistance = 0;
    public float maxDistance = 7;
    [Header("Physical Interaction")]
    [Tooltip("Layers to use for raycasting. Check Default, Walls, Player, Monster, Doors, Interactables, Item, etc. Uncheck IgnoreRaycast, AggroArea, Water, UI, etc.")]
    public LayerMask raycastLayers = Physics.DefaultRaycastLayers;
    [Header("Offsets - Standing")]
    public Vector2 firstPersonOffsetStanding = Vector2.zero;
    public Vector2 thirdPersonOffsetStanding = Vector2.up;
    public Vector2 thirdPersonOffsetStandingMultiplier = Vector2.zero;
    [Header("Offsets - Crouching")]
    public Vector2 firstPersonOffsetCrouching = Vector2.zero;
    public Vector2 thirdPersonOffsetCrouching = Vector2.up / 2;
    public Vector2 thirdPersonOffsetCrouchingMultiplier = Vector2.zero;
    public float crouchOriginMultiplier = 0.65f;
    [Header("Offsets - Crawling")]
    public Vector2 firstPersonOffsetCrawling = Vector2.zero;
    public Vector2 thirdPersonOffsetCrawling = Vector2.up;
    public Vector2 thirdPersonOffsetCrawlingMultiplier = Vector2.zero;
    public float crawlOriginMultiplier = 0.65f;
    [Header("Offsets - Swimming")]
    public Vector2 firstPersonOffsetSwimming = Vector2.zero;
    public Vector2 thirdPersonOffsetSwimming = Vector2.up;
    public Vector2 thirdPersonOffsetSwimmingMultiplier = Vector2.zero;
    public float swimOriginMultiplier = 0.65f;
    
    
    [SyncVar, HideInInspector] public byte syncedLookDirectionFarXrot;
    [SyncVar, HideInInspector] public byte syncedLookDirectionFarYrot;
    Vector3 syncedLookDirectionFar
    {
        get
        {
            return UncompressDirection(syncedLookDirectionFarXrot, syncedLookDirectionFarYrot);
        }
    }
    public Vector3 lookDirectionFar
    {
        get
        {
            return isLocalPlayer ? camera.transform.forward : syncedLookDirectionFar;
        }
    }
    public Vector3 lookDirectionRaycasted
    {
        get
        {
            return (lookPositionRaycasted - headPosition).normalized;
        }
    }
    public Vector3 lookPositionFar
    {
        get
        {
            Vector3 position = isLocalPlayer ? camera.transform.position : headPosition;
            return position + lookDirectionFar * 9999f;
        }
    }
    public Vector3 lookPositionRaycasted
    {
        get
        {
            if (isLocalPlayer)
            {
                return Utils.RaycastWithout(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity, gameObject, raycastLayers)
                       ? hit.point
                       : lookPositionFar;
            }
            else
            {
                
                
                Debug.LogError("PlayerLook.lookPositionRaycasted isn't synced so you can only call it on the local player right now.\n" + Environment.StackTrace);
                return Vector3.zero;
            }
       }
    }
    void Awake()
    {
        camera = Camera.main;
    }
    void Start()
    {
        if (isLocalPlayer) 
        {
            camera.transform.SetParent(transform, false);
            camera.transform.rotation = transform.rotation;
            camera.transform.position = headPosition;
        }
        originalCameraPosition = camera.transform.localPosition;
    }
    [Command]
    void CmdSetLookDirection(byte rotX, byte rotY)
    {
        syncedLookDirectionFarXrot = rotX;
        syncedLookDirectionFarYrot = rotY;
    }
    [Command]
    void CmdSetFreeLooking(bool state)
    {
        syncedIsFreeLooking = state;
    }
    static void CompressDirection(Vector3 direction, out byte xRotation, out byte yRotation)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);
        Vector3 euler = rotation.eulerAngles;
        xRotation = FloatBytePacker.ScaleFloatToByte(euler.x, 0, 360, byte.MinValue, byte.MaxValue);
        yRotation = FloatBytePacker.ScaleFloatToByte(euler.y, 0, 360, byte.MinValue, byte.MaxValue);
    }
    static Vector3 UncompressDirection(byte xRotation, byte yRotation)
    {
        float x = FloatBytePacker.ScaleByteToFloat(xRotation, byte.MinValue, byte.MaxValue, 0, 360);
        float y = FloatBytePacker.ScaleByteToFloat(yRotation, byte.MinValue, byte.MaxValue, 0, 360);
        Quaternion rotation = Quaternion.Euler(x, y, 0);
        return rotation * Vector3.forward;
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        if (Time.time - lastClientSendTime >= syncInterval)
        {
            CompressDirection(lookDirectionFar, out byte x, out byte y);
            if (x != syncedLookDirectionFarXrot || y != syncedLookDirectionFarYrot)
                CmdSetLookDirection(x, y); 
            bool freeLooking = IsFreeLooking();
            if (freeLooking != syncedIsFreeLooking)
                CmdSetFreeLooking(freeLooking);
            lastClientSendTime = Time.time;
        }
        if (health.current > 0 && Cursor.lockState == CursorLockMode.Locked)
        {
            float xExtra = Input.GetAxis("Mouse X") * XSensitivity;
            float yExtra = Input.GetAxis("Mouse Y") * YSensitivity;
            
            if (movement.state == MoveState.CLIMBING ||
                (Input.GetKey(freeLookKey) && !UIUtils.AnyInputActive() && distance > 0))
            {
                if (camera.transform.parent != freeLookParent)
                    InitializeFreeLook();
                freeLookParent.Rotate(new Vector3(0, xExtra, 0));
                camera.transform.Rotate(new Vector3(-yExtra, 0, 0));
            }
            else
            {
                if (camera.transform.parent != transform)
                    InitializeForcedLook();
                transform.Rotate(new Vector3(0, xExtra, 0));
                camera.transform.Rotate(new Vector3(-yExtra, 0, 0));
            }
        }
    }
    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        camera.transform.localRotation = Utils.ClampRotationAroundXAxis(camera.transform.localRotation, MinimumX, MaximumX);
        if (!Utils.IsCursorOverUserInterface())
        {
            float step = Utils.GetZoomUniversal() * zoomSpeed;
            distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
        }
        if (distance == 0) 
        {
            Vector3 headLocal = transform.InverseTransformPoint(headPosition);
            Vector3 origin = Vector3.zero;
            Vector3 offset = Vector3.zero;
            if (movement.state == MoveState.CROUCHING)
            {
                origin = headLocal * crouchOriginMultiplier;
                offset = firstPersonOffsetCrouching;
            }
            else if (movement.state == MoveState.CRAWLING)
            {
                origin = headLocal * crawlOriginMultiplier;
                offset = firstPersonOffsetCrawling;
            }
            else if (movement.state == MoveState.SWIMMING)
            {
                origin = headLocal;
                offset = firstPersonOffsetSwimming;
            }
            else
            {
                origin = headLocal;
                offset = firstPersonOffsetStanding;
            }
            Vector3 target = transform.TransformPoint(origin + offset);
            camera.transform.position = target;
        }
        else 
        {
            Vector3 origin = Vector3.zero;
            Vector3 offsetBase = Vector3.zero;
            Vector3 offsetMult = Vector3.zero;
            if (movement.state == MoveState.CROUCHING)
            {
                origin = originalCameraPosition * crouchOriginMultiplier;
                offsetBase = thirdPersonOffsetCrouching;
                offsetMult = thirdPersonOffsetCrouchingMultiplier;
            }
            else if (movement.state == MoveState.CRAWLING)
            {
                origin = originalCameraPosition * crawlOriginMultiplier;
                offsetBase = thirdPersonOffsetCrawling;
                offsetMult = thirdPersonOffsetCrawlingMultiplier;
            }
            else if (movement.state == MoveState.SWIMMING)
            {
                origin = originalCameraPosition * swimOriginMultiplier;
                offsetBase = thirdPersonOffsetSwimming;
                offsetMult = thirdPersonOffsetSwimmingMultiplier;
            }
            else
            {
                origin = originalCameraPosition;
                offsetBase = thirdPersonOffsetStanding;
                offsetMult = thirdPersonOffsetStandingMultiplier;
            }
            Vector3 target = transform.TransformPoint(origin + offsetBase + offsetMult * distance);
            Vector3 newPosition = target - (camera.transform.rotation * Vector3.forward * distance);
            
            float finalDistance = distance;
            Debug.DrawLine(target, camera.transform.position, Color.white);
            if (Physics.Linecast(target, newPosition, out RaycastHit hit, viewBlockingLayers))
            {
                finalDistance = Vector3.Distance(target, hit.point) - 0.1f;
                Debug.DrawLine(target, hit.point, Color.red);
            }
            else Debug.DrawLine(target, newPosition, Color.green);
            camera.transform.position = target - (camera.transform.rotation * Vector3.forward * finalDistance);
        }
    }
    public bool InFirstPerson()
    {
        return distance == 0;
    }
    public bool IsFreeLooking()
    {
        if (isLocalPlayer)
        {
            return camera != null && 
                   camera.transform.parent == freeLookParent;
        }
        return syncedIsFreeLooking;
    }
    public void InitializeFreeLook()
    {
        camera.transform.SetParent(freeLookParent, false);
        freeLookParent.localRotation = Quaternion.identity; 
        ik.lookAtBodyWeightActive = false;
    }
    public void InitializeForcedLook()
    {
        camera.transform.SetParent(transform, false);
        ik.lookAtBodyWeightActive = true;
    }
    void OnDrawGizmos()
    {
        if (!isLocalPlayer) return;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(headPosition, camera.transform.position + camera.transform.forward * 9999f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(headPosition, lookPositionFar);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(headPosition, lookPositionRaycasted);
    }
}
