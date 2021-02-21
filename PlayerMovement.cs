
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Controller2k;
using Mirror;





public enum MoveState : byte { IDLE, WALKING, RUNNING, CROUCHING, CRAWLING, AIRBORNE, CLIMBING, SWIMMING, DEAD }

[RequireComponent(typeof(CharacterController2k))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : NetworkBehaviourNonAlloc
{
    
    [Header("Components")]
    public Animator animator;
    public Health health;
    public CharacterController2k controller;
    public AudioSource feetAudio;
    public Combat combat;
    public PlayerLook look;
    
    
    
    public CapsuleCollider controllerCollider;
#pragma warning disable CS0109 
    new Camera camera;
#pragma warning restore CS0109 

    [Header("State")]
    public MoveState state = MoveState.IDLE;
    MoveState lastState = MoveState.IDLE;
    [HideInInspector] public Vector3 moveDir;

    
    
    [Header("Rotation")]
    public float rotationSpeed = 150;

    [Header("Walking")]
    public float walkSpeed = 5;
    public float walkAcceleration = 15; 
    public float walkDeceleration = 20; 

    [Header("Running")]
    public float runSpeed = 8;
    [Range(0f, 1f)] public float runStepLength = 0.7f;
    public float runStepInterval = 3;
    public float runCycleLegOffset = 0.2f; 
    public KeyCode runKey = KeyCode.LeftShift;
    float stepCycle;
    float nextStep;

    [Header("Crouching")]
    public float crouchSpeed = 1.5f;
    public float crouchAcceleration = 5; 
    public float crouchDeceleration = 10; 
    public KeyCode crouchKey = KeyCode.C;
    bool crouchKeyPressed;

    [Header("Crawling")]
    public float crawlSpeed = 1;
    public float crawlAcceleration = 5; 
    public float crawlDeceleration = 10; 
    public KeyCode crawlKey = KeyCode.V;
    bool crawlKeyPressed;

    [Header("Swimming")]
    public float swimSpeed = 4;
    public float swimAcceleration = 15; 
    public float swimDeceleration = 20; 
    public float swimSurfaceOffset = 0.25f;
    Collider waterCollider;
    bool inWater => waterCollider != null; 
    bool underWater; 
    [Range(0, 1)] public float underwaterThreshold = 0.9f; 
    public LayerMask canStandInWaterCheckLayers = Physics.DefaultRaycastLayers; 

    [Header("Jumping")]
    public float jumpSpeed = 7;
    [HideInInspector] public float jumpLeg;
    bool jumpKeyPressed;

    [Header("Falling")]
    public float airborneAcceleration = 15; 
    public float airborneDeceleration = 20; 
    public float fallMinimumMagnitude = 6; 
    public float fallDamageMinimumMagnitude = 13;
    public float fallDamageMultiplier = 2;
    [HideInInspector] public Vector3 lastFall;
    bool sprintingBeforeAirborne; 

    [Header("Climbing")]
    public float climbSpeed = 3;
    Collider ladderCollider;

    [Header("Physics")]
    [Tooltip("Apply a small default downward force while grounded in order to stick on the ground and on rounded surfaces. Otherwise walking on rounded surfaces would be detected as falls, preventing the player from jumping.")]
    public float gravityMultiplier = 2;

    [Header("Synchronization (Best not to modify)")]
    [Tooltip("Buffer at least that many moves before applying them in FixedUpdate. A bigger min buffer offers more lag tolerance with the cost of additional latency.")]
    public int minMoveBuffer = 2;
    [Tooltip("Combine two moves as one after having more than that many pending moves in order to avoid ever growing queues.")]
    public int combineMovesAfter = 5;
    [Tooltip("Buffer at most that many moves before force reseting the client. There is no point in buffering hundreds of moves and having it always lag 3 seconds behind.")]
    public int maxMoveBuffer = 10; 
    [Tooltip("Rubberband movement: player can move freely as long as the server position matches. If server and client get off further than rubberDistance then a force reset happens.")]
    public float rubberDistance = 1;
    byte route = 0; 
    int combinedMoves = 0; 
    int rubbered = 0; 

    
    
    float horizontalSpeed;

    
    
    
    
    public bool isGroundedWithinTolerance =>
        controller.isGrounded || controller.velocity.y > -fallMinimumMagnitude;

    [Header("Sounds")]
    public AudioClip[] footstepSounds;    
    public AudioClip jumpSound;           
    public AudioClip landSound;           

    [Header("Debug")]
    [Tooltip("Debug GUI visibility curve. X axis = distance, Y axis = alpha. Nothing will be displayed if Y = 0.")]
    public AnimationCurve debugVisibilityCurve = new AnimationCurve(new Keyframe(0, 0.3f), new Keyframe(15, 0.3f), new Keyframe(20, 0f));


    
    
    
    public Vector3 velocity { get; private set; }

    void Awake()
    {
        camera = Camera.main;
    }

    void OnValidate()
    {
        minMoveBuffer = Mathf.Clamp(minMoveBuffer, 1, maxMoveBuffer); 
        combineMovesAfter = Mathf.Clamp(combineMovesAfter, minMoveBuffer + 1, maxMoveBuffer);
        maxMoveBuffer = Mathf.Clamp(maxMoveBuffer, combineMovesAfter + 1, 50); 
    }

    
    Vector2 GetInputDirection()
    {
        
        
        float horizontal = 0;
        float vertical = 0;
        if (!UIUtils.AnyInputActive())
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        return new Vector2(horizontal, vertical).normalized;
    }

    Vector3 GetDesiredDirection(Vector2 inputDir)
    {
        
        return transform.forward * inputDir.y + transform.right * inputDir.x;
    }

    
    
    void AdjustControllerCollider()
    {
        
        float ratio = 1;
        if (state == MoveState.CROUCHING)
            ratio = 0.5f;
        else if (state == MoveState.CRAWLING || state == MoveState.SWIMMING || state == MoveState.DEAD)
            ratio = 0.25f;

        controller.TrySetHeight(controller.defaultHeight * ratio, true, true, false);
    }

    
    bool EventDied()
    {
        return health.current == 0;
    }

    bool EventJumpRequested()
    {
        
        
        
        
        
        
        
        return isGroundedWithinTolerance &&
               controller.slidingState == SlidingState.NONE &&
               jumpKeyPressed;
    }

    bool EventCrouchToggle()
    {
        return crouchKeyPressed;
    }

    bool EventCrawlToggle()
    {
        return crawlKeyPressed;
    }

    bool EventFalling()
    {
        
        
        
        return !isGroundedWithinTolerance;
    }

    bool EventLanded()
    {
        return controller.isGrounded;
    }

    bool EventUnderWater()
    {
        
        
        
        if (inWater) 
        {
            
            
            Vector3 origin = new Vector3(transform.position.x,
                                         waterCollider.bounds.max.y,
                                         transform.position.z);
            float distance = controllerCollider.height * underwaterThreshold;
            Debug.DrawLine(origin, origin + Vector3.down * distance, Color.cyan);

            
            return !Utils.RaycastWithout(origin, Vector3.down, out RaycastHit hit, distance, gameObject, canStandInWaterCheckLayers);
        }
        return false;
    }

    bool EventLadderEnter()
    {
        return ladderCollider != null;
    }

    bool EventLadderExit()
    {
        
        
        
        return ladderCollider != null &&
               !ladderCollider.bounds.Intersects(controllerCollider.bounds);
    }

    
    float ApplyGravity(float moveDirY)
    {
        
        if (!controller.isGrounded)
            
            
            
            return moveDirY + Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
        
        
        
        return 0;
    }

    
    float GetWalkOrRunSpeed()
    {
        bool runRequested = !UIUtils.AnyInputActive() && Input.GetKey(runKey);
        return runRequested && 1 > 0 ? runSpeed : walkSpeed;
    }

    void ApplyFallDamage()
    {
        
        
        float fallMagnitude = Mathf.Abs(lastFall.y);
        if (fallMagnitude >= fallDamageMinimumMagnitude)
        {
            int damage = Mathf.RoundToInt(fallMagnitude * fallDamageMultiplier);
            health.current -= damage;
            combat.RpcOnReceivedDamage(damage, transform.position, -lastFall);
        }
    }

    
    float AccelerateSpeed(Vector2 inputDir, float currentSpeed, float targetSpeed, float acceleration)
    {
        
        float desiredSpeed = inputDir.magnitude * targetSpeed;

        
        return Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
    }

    
    void RotateWithKeys()
    {
        if (!UIUtils.AnyInputActive())
        {
            float horizontal2 = Input.GetAxis("Horizontal2");
            transform.Rotate(Vector3.up * horizontal2 * rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void EnterLadder()
    {
        
        
        
        
        
        
        
        
        
        if (isLocalPlayer)
        {
            look.InitializeFreeLook();
            transform.forward = ladderCollider.transform.forward;
        }
    }

    MoveState UpdateIDLE(Vector2 inputDir, Vector3 desiredDir)
    {
        
        RotateWithKeys();

        
        
        horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, 0, walkDeceleration);
        moveDir.x = desiredDir.x * horizontalSpeed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * horizontalSpeed;

        if (EventDied())
        {
            
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            
            return MoveState.DEAD;
        }
        else if (EventFalling())
        {
            sprintingBeforeAirborne = false;
            return MoveState.AIRBORNE;
        }
        else if (EventJumpRequested())
        {
            
            
            moveDir.y = jumpSpeed;
            sprintingBeforeAirborne = false;
            PlayJumpSound();
            return MoveState.AIRBORNE;
        }
        else if (EventCrouchToggle())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false))
            {
                return MoveState.CROUCHING;
            }
        }
        else if (EventCrawlToggle())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.CRAWLING;
            }
        }
        else if (EventLadderEnter())
        {
            EnterLadder();
            return MoveState.CLIMBING;
        }
        else if (EventUnderWater())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.SWIMMING;
            }
        }
        else if (inputDir != Vector2.zero)
        {
            return MoveState.WALKING;
        }

        return MoveState.IDLE;
    }

    MoveState UpdateWALKINGandRUNNING(Vector2 inputDir, Vector3 desiredDir)
    {
        
        RotateWithKeys();

        
        float speed = GetWalkOrRunSpeed();

        
        horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, speed, inputDir != Vector2.zero ? walkAcceleration : walkDeceleration);
        moveDir.x = desiredDir.x * horizontalSpeed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * horizontalSpeed;

        if (EventDied())
        {
            
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            
            return MoveState.DEAD;
        }
        else if (EventFalling())
        {
            sprintingBeforeAirborne = speed == runSpeed;
            return MoveState.AIRBORNE;
        }
        else if (EventJumpRequested())
        {
            
            
            moveDir.y = jumpSpeed;
            sprintingBeforeAirborne = speed == runSpeed;
            PlayJumpSound();
            return MoveState.AIRBORNE;
        }
        else if (EventCrouchToggle())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false))
            {
                
                
                
                
                horizontalSpeed = Mathf.Min(horizontalSpeed, crouchSpeed);
                return MoveState.CROUCHING;
            }
        }
        else if (EventCrawlToggle())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                
                
                
                
                horizontalSpeed = Mathf.Min(horizontalSpeed, crawlSpeed);
                return MoveState.CRAWLING;
            }
        }
        else if (EventLadderEnter())
        {
            EnterLadder();
            return MoveState.CLIMBING;
        }
        else if (EventUnderWater())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.SWIMMING;
            }
        }
        
        else if (moveDir.x == 0 && moveDir.z == 0)
        {
            return MoveState.IDLE;
        }

        ProgressStepCycle(inputDir, speed);
        return speed == walkSpeed ? MoveState.WALKING : MoveState.RUNNING;
    }

    MoveState UpdateCROUCHING(Vector2 inputDir, Vector3 desiredDir)
    {
        
        RotateWithKeys();

        
        horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, crouchSpeed, inputDir != Vector2.zero ? crouchAcceleration : crouchDeceleration);
        moveDir.x = desiredDir.x * horizontalSpeed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * horizontalSpeed;

        if (EventDied())
        {
            
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            
            return MoveState.DEAD;
        }
        else if (EventFalling())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                sprintingBeforeAirborne = false;
                return MoveState.AIRBORNE;
            }
        }
        else if (EventJumpRequested())
        {
            
            

            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                return MoveState.IDLE;
            }
        }
        else if (EventCrouchToggle())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                return MoveState.IDLE;
            }
        }
        else if (EventCrawlToggle())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                
                
                
                
                horizontalSpeed = Mathf.Min(horizontalSpeed, crawlSpeed);
                return MoveState.CRAWLING;
            }
        }
        else if (EventLadderEnter())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
        }
        else if (EventUnderWater())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.SWIMMING;
            }
        }

        ProgressStepCycle(inputDir, crouchSpeed);
        return MoveState.CROUCHING;
    }

    MoveState UpdateCRAWLING(Vector2 inputDir, Vector3 desiredDir)
    {
        
        RotateWithKeys();

        
        horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, crawlSpeed, inputDir != Vector2.zero ? crawlAcceleration : crawlDeceleration);
        moveDir.x = desiredDir.x * horizontalSpeed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * horizontalSpeed;

        if (EventDied())
        {
            
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            
            return MoveState.DEAD;
        }
        else if (EventFalling())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                sprintingBeforeAirborne = false;
                return MoveState.AIRBORNE;
            }
        }
        else if (EventJumpRequested())
        {
            
            

            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                return MoveState.IDLE;
            }
        }
        else if (EventCrouchToggle())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false))
            {
                
                
                
                
                horizontalSpeed = Mathf.Min(horizontalSpeed, crouchSpeed);
                return MoveState.CROUCHING;
            }
        }
        else if (EventCrawlToggle())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                return MoveState.IDLE;
            }
        }
        else if (EventLadderEnter())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
        }
        else if (EventUnderWater())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.SWIMMING;
            }
        }

        ProgressStepCycle(inputDir, crawlSpeed);
        return MoveState.CRAWLING;
    }

    MoveState UpdateAIRBORNE(Vector2 inputDir, Vector3 desiredDir)
    {
        
        RotateWithKeys();

        
        float speed = sprintingBeforeAirborne ? runSpeed : walkSpeed;

        
        horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, speed, inputDir != Vector2.zero ? airborneAcceleration : airborneDeceleration);
        moveDir.x = desiredDir.x * horizontalSpeed;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = desiredDir.z * horizontalSpeed;

        if (EventDied())
        {
            
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            
            return MoveState.DEAD;
        }
        else if (EventLanded())
        {
            PlayLandingSound();
            return MoveState.IDLE;
        }
        else if (EventLadderEnter())
        {
            EnterLadder();
            return MoveState.CLIMBING;
        }
        else if (EventUnderWater())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
            {
                return MoveState.SWIMMING;
            }
        }

        return MoveState.AIRBORNE;
    }

    MoveState UpdateCLIMBING(Vector2 inputDir, Vector3 desiredDir)
    {
        if (EventDied())
        {
            
            
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            ladderCollider = null;
            
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            
            return MoveState.DEAD;
        }
        
        else if (EventLadderExit())
        {
            
            
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            ladderCollider = null;
            return MoveState.IDLE;
        }

        
        
        
        
        moveDir.x = inputDir.x * climbSpeed;
        moveDir.y = inputDir.y * climbSpeed;
        moveDir.z = 0;

        
        
        moveDir = ladderCollider.transform.rotation * moveDir;
        Debug.DrawLine(transform.position, transform.position + moveDir, Color.yellow, 0.1f, false);

        return MoveState.CLIMBING;
    }

    MoveState UpdateSWIMMING(Vector2 inputDir, Vector3 desiredDir)
    {
        if (EventDied())
        {
            
            controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
            
            return MoveState.DEAD;
        }
        
        else if (EventLadderEnter())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
        }
        
        else if (!EventUnderWater())
        {
            
            if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
            {
                return MoveState.IDLE;
            }
        }

        
        RotateWithKeys();

        
        horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, swimSpeed, inputDir != Vector2.zero ? swimAcceleration : swimDeceleration);
        moveDir.x = desiredDir.x * horizontalSpeed;
        moveDir.z = desiredDir.z * horizontalSpeed;

        
        if (waterCollider != null)
        {
            float surface = waterCollider.bounds.max.y;
            float surfaceDirection = surface - controller.bounds.min.y - swimSurfaceOffset;
            moveDir.y = surfaceDirection * swimSpeed;
        }
        else moveDir.y = 0;

        return MoveState.SWIMMING;
    }

    MoveState UpdateDEAD(Vector2 inputDir, Vector3 desiredDir)
    {
        
        
        moveDir.x = 0;
        moveDir.y = ApplyGravity(moveDir.y);
        moveDir.z = 0;

        
        if (health.current > 0)
        {
            
            
            
            controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
            return MoveState.IDLE;
        }
        return MoveState.DEAD;
    }

    
    void Update()
    {
        
        if (!UIUtils.AnyInputActive())
        {
            if (!jumpKeyPressed) jumpKeyPressed = Input.GetButtonDown("Jump");
            if (!crawlKeyPressed) crawlKeyPressed = Input.GetKeyDown(crawlKey);
            if (!crouchKeyPressed) crouchKeyPressed = Input.GetKeyDown(crouchKey);
        }
    }

    
    struct Move
    {
        public byte route;
        public MoveState state;
        public Vector3 position; 
        public byte yRotation; 
        public Move(byte route, MoveState state, Vector3 position, byte yRotation)
        {
            this.route = route;
            this.state = state;
            this.position = position;
            this.yRotation = yRotation;
        }
    }
    Queue<Move> pendingMoves = new Queue<Move>();
    [Command]
    void CmdFixedMove(Move move)
    {
        
        
        
        if (move.route != route)
            return;

        
        
        
        
        
        if (!isLocalPlayer && pendingMoves.Count < maxMoveBuffer)
            pendingMoves.Enqueue(move);

        
        
        RpcFixedMove(move);
    }

    [ClientRpc]
    void RpcFixedMove(Move move)
    {
        
        

        
        if (isServer) return;

        
        if (isLocalPlayer) return;

        
        
        
        
        if (pendingMoves.Count < maxMoveBuffer)
            pendingMoves.Enqueue(move);
    }

    
    
    [ClientRpc]
    void RpcForceReset(Vector3 position, byte newRoute)
    {
        
        transform.position = position;

        
        route = newRoute;

        
        
        pendingMoves.Clear();
    }

    [Server]
    public void Warp(Vector3 position)
    {
        
        transform.position = position;

        
        
        
        
        
        if (isServer)
        {
            
            ++route;

            
            pendingMoves.Clear();

            
            RpcForceReset(position, route);
            
        }
    }

    
    
    
    
    
    
    [Server]
    void RubberbandCheck(Vector3 expectedPosition)
    {
        if (Vector3.Distance(transform.position, expectedPosition) >= rubberDistance)
        {
            Warp(transform.position);
            ++rubbered; 
        }
    }

    
    
    void FixedUpdate()
    {
        
        if (isLocalPlayer)
        {
            
            Vector2 inputDir = GetInputDirection();
            Vector3 desiredDir = GetDesiredDirection(inputDir);
            Debug.DrawLine(transform.position, transform.position + desiredDir, Color.cyan);

            
            if (state == MoveState.IDLE) state = UpdateIDLE(inputDir, desiredDir);
            else if (state == MoveState.WALKING) state = UpdateWALKINGandRUNNING(inputDir, desiredDir);
            else if (state == MoveState.RUNNING) state = UpdateWALKINGandRUNNING(inputDir, desiredDir);
            else if (state == MoveState.CROUCHING) state = UpdateCROUCHING(inputDir, desiredDir);
            else if (state == MoveState.CRAWLING) state = UpdateCRAWLING(inputDir, desiredDir);
            else if (state == MoveState.AIRBORNE) state = UpdateAIRBORNE(inputDir, desiredDir);
            else if (state == MoveState.CLIMBING) state = UpdateCLIMBING(inputDir, desiredDir);
            else if (state == MoveState.SWIMMING) state = UpdateSWIMMING(inputDir, desiredDir);
            else if (state == MoveState.DEAD) state = UpdateDEAD(inputDir, desiredDir);
            else Debug.LogError("Unhandled Movement State: " + state);

            
            if (!controller.isGrounded) lastFall = controller.velocity;

            
            controller.Move(moveDir * Time.fixedDeltaTime); 
            velocity = controller.velocity; 

            
            byte rotationByte = FloatBytePacker.ScaleFloatToByte(transform.rotation.eulerAngles.y, 0, 360, byte.MinValue, byte.MaxValue); 
            CmdFixedMove(new Move(route, state, transform.position, rotationByte));

            
            
            
            float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
            jumpLeg = (runCycle < 0.5f ? 1 : -1);

            
            jumpKeyPressed = false;
            crawlKeyPressed = false;
            crouchKeyPressed = false;
        }
        
        else
        {
            
            
            
            
            
            
            
            
            
            
            if (lastState != state)
                AdjustControllerCollider();

            
            
            if (!controller.isGrounded) lastFall = velocity;

            
            
            
            
            if (pendingMoves.Count > 0 && pendingMoves.Count >= minMoveBuffer)
            {
                
                
                if (isServer && pendingMoves.Count >= maxMoveBuffer)
                {
                    
                    Warp(transform.position);
                }
                
                
                else if (pendingMoves.Count >= 2 && pendingMoves.Count >= combineMovesAfter)
                {
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    Move first = pendingMoves.Dequeue();
                    Move second = pendingMoves.Dequeue();
                    state = second.state;
                    Vector3 move = second.position - transform.position; 
                    velocity = second.position - first.position; 
                    float yRotation = FloatBytePacker.ScaleByteToFloat(second.yRotation, byte.MinValue, byte.MaxValue, 0, 360);
                    transform.rotation = Quaternion.Euler(0, yRotation, 0);
                    controller.Move(move);
                    ++combinedMoves; 

                    
                    if (isServer)
                        RubberbandCheck(second.position);
                }
                
                else
                {
                    
                    Move next = pendingMoves.Dequeue();
                    state = next.state;
                    Vector3 move = next.position - transform.position; 
                    float yRotation = FloatBytePacker.ScaleByteToFloat(next.yRotation, byte.MinValue, byte.MaxValue, 0, 360);
                    transform.rotation = Quaternion.Euler(0, yRotation, 0);
                    controller.Move(move);
                    velocity = controller.velocity; 

                    
                    if (isServer)
                        RubberbandCheck(next.position);
                }
            }
            
        }

        
        if (isServer)
        {
            
            
            
            
            
            if (lastState == MoveState.AIRBORNE && state != MoveState.AIRBORNE)
            {
                ApplyFallDamage();
            }
        }

        
        lastState = state;
    }

    void OnGUI()
    {
        
        
        
        if (Debug.isDebugBuild &&
            ((NetworkManagerSurvival)NetworkManager.singleton).showDebugGUI)
        {
            
            Vector3 center = controllerCollider.bounds.center;
            Vector3 point = camera.WorldToScreenPoint(center);

            
            
            float distance = Vector3.Distance(camera.transform.position, transform.position);
            float alpha = debugVisibilityCurve.Evaluate(distance);

            
            if (alpha > 0 && point.z >= 0 && Utils.IsPointInScreen(point))
            {
                GUI.color = new Color(0, 0, 0, alpha);
                GUILayout.BeginArea(new Rect(point.x, Screen.height - point.y, 160, 100));
                GUILayout.Label("grounded=" + controller.isGrounded);
                GUILayout.Label("groundedT=" + isGroundedWithinTolerance);
                GUILayout.Label("lastFall=" + lastFall.y);
                GUILayout.Label("sliding=" + controller.slidingState);
                if (!isLocalPlayer)
                {
                    GUILayout.Label("health=" + health.current + "/" + health.max);
                    GUILayout.Label("pending=" + pendingMoves.Count);
                    GUILayout.Label("route=" + route);
                    GUILayout.Label("combined=" + combinedMoves);
                }
                if (isServer) GUILayout.Label("rubbered=" + rubbered);
                GUILayout.EndArea();
                GUI.color = Color.white;
            }
        }
    }

    void PlayLandingSound()
    {
        feetAudio.clip = landSound;
        feetAudio.Play();
        nextStep = stepCycle + .5f;
    }

    void PlayJumpSound()
    {
        feetAudio.clip = jumpSound;
        feetAudio.Play();
    }

    void ProgressStepCycle(Vector3 inputDir, float speed)
    {
        if (controller.velocity.sqrMagnitude > 0 && (inputDir.x != 0 || inputDir.y != 0))
        {
            stepCycle += (controller.velocity.magnitude + (speed * (state == MoveState.WALKING ? 1 : runStepLength))) *
                         Time.fixedDeltaTime;
        }

        if (stepCycle > nextStep)
        {
            nextStep = stepCycle + runStepInterval;
            PlayFootStepAudio();
        }
    }

    void PlayFootStepAudio()
    {
        if (!controller.isGrounded) return;

        
        
        int n = Random.Range(1, footstepSounds.Length);
        feetAudio.clip = footstepSounds[n];
        feetAudio.PlayOneShot(feetAudio.clip);

        
        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = feetAudio.clip;
    }

    [ClientCallback] 
    
    void OnTriggerEnter(Collider co)
    {
        
        
        if (co.CompareTag("Ladder"))
            ladderCollider = co;
        
        
        else if (co.CompareTag("Water"))
            waterCollider = co;
    }

    [ClientCallback] 
    void OnTriggerExit(Collider co)
    {
        
        if (co.CompareTag("Water"))
            waterCollider = null;
    }
}
