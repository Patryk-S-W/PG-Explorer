using UnityEngine;
using UnityEngine.AI;
using Mirror;
public class Monster : Entity
{
    [Header("Components")]
    public NavMeshAgent agent;
    public AudioSource audioSource;
#pragma warning disable CS0109 
    new Camera camera;
#pragma warning restore CS0109 
    [Header("Movement")]
    public float walkSpeed = 1;
    public float runSpeed = 5;
    [Range(0, 1)] public float moveProbability = 0.1f; 
    public float moveDistance = 10;
    public float followDistance = 20;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.5f; 
    [Header("Attack")]
    public float attackRange = 2;
    public float attackInterval = 0.5f; 
    double attackEndTime;  
    public AudioClip attackSound;
    [Header("Difficulty")]
    [Tooltip("If a player runs out of attack range while an attack is in progress and kiting is enabled, then the attack will be canceled. If kiting is disabled, then the attack will always finish once it started, even if the player ran out of range.")]
    public bool allowKiting = false;
    [Header("Debug")]
    [Tooltip("Debug GUI visibility curve. X axis = distance, Y axis = alpha. Nothing will be displayed if Y = 0.")]
    public AnimationCurve debugVisibilityCurve = new AnimationCurve(new Keyframe(0, 0.3f), new Keyframe(15, 0.3f), new Keyframe(20, 0f));
    Vector3 startPosition;
    [SyncVar] public string state = "IDLE";
    [Header("Target")]
    [SyncVar] GameObject _target;
    public Entity target
    {
        get { return _target != null  ? _target.GetComponent<Entity>() : null; }
        set { _target = value != null ? value.gameObject : null; }
    }
    void Awake()
    {
        camera = Camera.main;
    }
    void Start()
    {
        startPosition = transform.position;
    }
    public override void OnStartServer()
    {
        if (health.current == 0) state = "DEAD";
    }
    public bool IsHidden() => proximityChecker.forceHidden;
    
    public bool IsWorthUpdating() =>
        netIdentity.observers == null ||
        netIdentity.observers.Count > 0 ||
        IsHidden();
    void Update()
    {
        if (IsWorthUpdating())
        {
            if (isClient) UpdateClient();
            if (isServer)
            {
                if (target != null && target.proximityChecker.forceHidden) target = null;
                state = UpdateServer();
            }
        }
    }
    public void LookAtY(Vector3 position)
    {
        transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
    }
    public bool IsMoving() =>
        agent.pathPending ||
        agent.remainingDistance > agent.stoppingDistance ||
        agent.velocity != Vector3.zero;
    bool EventTargetDisappeared() =>
        target == null;
    bool EventTargetDied() =>
        target != null && target.health.current == 0;
    bool EventTargetTooFarToAttack() =>
        target != null &&
        Utils.ClosestDistance(collider, target.collider) > attackRange;
    bool EventTargetTooFarToFollow() =>
        target != null &&
        Vector3.Distance(startPosition, target.collider.ClosestPoint(transform.position)) > followDistance;
    bool EventAggro() =>
        target != null && target.health.current > 0;
    bool EventAttackFinished() =>
        NetworkTime.time >= attackEndTime;
    bool EventMoveEnd() =>
        state == "MOVING" && !IsMoving();
    bool EventMoveRandomly() =>
        Random.value <= moveProbability * Time.deltaTime;
    public bool IsTargetInRangeButNotReachable()
    {
        if (target != null)
        {
            Collider targetCollider = target.collider;
            return Utils.ClosestDistance(collider, targetCollider) <= attackRange &&
                   !Utils.IsReachableVertically(collider, targetCollider, attackRange);
        }
        return false;
    }
    [Server]
    string UpdateServer_IDLE()
    {
        if (agent.pathPending) return "IDLE";
        if (agent.hasPath) return "MOVING";
        if (EventTargetDied())
        {
            target = null;
            return "IDLE";
        }
        if (EventTargetTooFarToFollow())
        {
            target = null;
            agent.speed = walkSpeed;
            agent.stoppingDistance = 0;
            agent.destination = startPosition;
            
            return "IDLE";
        }
        if (EventTargetTooFarToAttack() || IsTargetInRangeButNotReachable())
        {
            agent.speed = runSpeed;
            
            agent.stoppingDistance = 0;
            agent.destination = target.collider.ClosestPoint(transform.position);
            
            return "IDLE";
        }
        if (EventAggro())
        {
            attackEndTime = NetworkTime.time + attackInterval;
            RpcOnAttackStarted();
            return "ATTACKING";
        }
        if (EventMoveRandomly())
        {
            Vector2 circle2D = Random.insideUnitCircle * moveDistance;
            agent.speed = walkSpeed;
            agent.stoppingDistance = 0;
            agent.destination = startPosition + new Vector3(circle2D.x, 0, circle2D.y);
            return "MOVING";
        }
        if (EventMoveEnd()) {} 
        if (EventTargetDisappeared()) {} 
        return "IDLE"; 
    }
    [Server]
    string UpdateServer_MOVING()
    {
        if (EventMoveEnd())
        {
            return "IDLE";
        }
        if (EventTargetDied())
        {
            target = null;
            agent.ResetMovement();
            return "IDLE";
        }
        if (EventTargetTooFarToFollow())
        {
            target = null;
            agent.speed = walkSpeed;
            agent.stoppingDistance = 0;
            agent.destination = startPosition;
            return "MOVING";
        }
        if (EventTargetTooFarToAttack() || IsTargetInRangeButNotReachable())
        {
            agent.speed = runSpeed;
            
            agent.stoppingDistance = 0;
            agent.destination = target.collider.ClosestPoint(transform.position);
            return "MOVING";
        }
        if (EventAggro())
        {
            
            
            Collider targetCollider = target.collider;
            if (Vector3.Distance(transform.position, targetCollider.ClosestPoint(transform.position)) <= attackRange * attackToMoveRangeRatio &&
                Utils.IsReachableVertically(collider, targetCollider, attackRange))
            {
                attackEndTime = NetworkTime.time + attackInterval;
                agent.ResetMovement();
                RpcOnAttackStarted();
                return "ATTACKING";
            }
        }
        if (EventAttackFinished()) {} 
        if (EventTargetDisappeared()) {} 
        if (EventMoveRandomly()) {} 
        return "MOVING"; 
    }
    [Server]
    string UpdateServer_ATTACKING()
    {
        if (agent.pathPending) return "ATTACKING";
        if (agent.hasPath) return "MOVING";
        if (target) LookAtY(target.transform.position);
        if (EventTargetDisappeared())
        {
            target = null;
            return "IDLE";
        }
        if (EventTargetDied())
        {
            target = null;
            return "IDLE";
        }
        if (EventAttackFinished())
        {
            combat.DealDamageAt(target, combat.damage, target.transform.position, -transform.forward, target.collider);
            if (target.health.current == 0)
                target = null;
            return "IDLE";
        }
        if (EventMoveEnd()) {} 
        if (EventTargetTooFarToAttack())
        {
            if (allowKiting)
            {
                
                agent.speed = runSpeed;
                agent.stoppingDistance = attackRange * attackToMoveRangeRatio;
                agent.destination = target.collider.ClosestPoint(transform.position);
                
                return "ATTACKING";
            }
        }
        if (EventTargetTooFarToFollow())
        {
            if (allowKiting)
            {
                
                target = null;
                agent.speed = walkSpeed;
                agent.stoppingDistance = 0;
                agent.destination = startPosition;
                
                return "ATTACKING";
            }
        }
        if (EventAggro()) {} 
        if (EventMoveRandomly()) {} 
        return "ATTACKING"; 
    }
    [Server]
    string UpdateServer_DEAD()
    {
        if (EventAttackFinished()) {} 
        if (EventMoveEnd()) {} 
        if (EventTargetDisappeared()) {} 
        if (EventTargetDied()) {} 
        if (EventTargetTooFarToFollow()) {} 
        if (EventTargetTooFarToAttack()) {} 
        if (EventAggro()) {} 
        if (EventMoveRandomly()) {} 
        return "DEAD"; 
    }
    [Server]
    string UpdateServer()
    {
        if (state == "IDLE")    return UpdateServer_IDLE();
        if (state == "MOVING")  return UpdateServer_MOVING();
        if (state == "ATTACKING") return UpdateServer_ATTACKING();
        if (state == "DEAD")    return UpdateServer_DEAD();
        Debug.LogError("invalid state: " + state);
        return "IDLE";
    }
    [Client]
    void UpdateClient()
    {
        if (state == "ATTACKING")
        {
            if (target) LookAtY(target.transform.position);
        }
    }
    [ClientCallback] 
    void LateUpdate()
    {
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            anim.SetBool("MOVING", state == "MOVING" && agent.velocity != Vector3.zero);
            anim.SetFloat("Speed", agent.speed);
            anim.SetBool("ATTACKING", state == "ATTACKING");
            anim.SetBool("DEAD", state == "DEAD");
        }
    }
    [Server]
    public void OnDeath()
    {
        state = "DEAD";
        agent.ResetMovement();
        target = null;
    }
    [Server]
    public void OnRespawn()
    {
        agent.Warp(startPosition);
        state = "IDLE";
    }
    public bool CanAttack(Entity entity)
    {
        return entity is Player &&
               entity.health.current > 0 &&
               health.current > 0;
    }
    void OnDrawGizmos()
    {
        Vector3 startHelp = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startHelp, moveDistance);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(startHelp, followDistance);
    }
    void OnGUI()
    {
        if (Debug.isDebugBuild &&
            ((NetworkManagerSurvival)NetworkManager.singleton).showDebugGUI)
        {
            Vector3 center = collider.bounds.center;
            Vector3 point = camera.WorldToScreenPoint(center);
            float distance = Vector3.Distance(camera.transform.position, transform.position);
            float alpha = debugVisibilityCurve.Evaluate(distance);
            if (alpha > 0 && point.z >= 0 && Utils.IsPointInScreen(point))
            {
                GUI.color = new Color(0, 0, 0, alpha);
                GUILayout.BeginArea(new Rect(point.x, Screen.height - point.y, 150, 120));
                GUILayout.Label("state=" + state);
                GUILayout.Label("moving=" + IsMoving());
                GUILayout.Label("pathPending=" + agent.pathPending);
                GUILayout.Label("hasPath=" + agent.hasPath);
                GUILayout.Label("health=" + health.current + "/" + health.max);
                GUILayout.EndArea();
                GUI.color = Color.white;
            }
        }
    }
    [ServerCallback]
    public void OnAggro(Entity entity)
    {
        if (entity != null && CanAttack(entity))
        {
            
            
            if (target == null)
            {
                target = entity;
            }
            else if (target != entity) 
            {
                float oldDistance = Vector3.Distance(transform.position, target.transform.position);
                float newDistance = Vector3.Distance(transform.position, entity.transform.position);
                if ((newDistance < oldDistance * 0.8) ||
                    (!Utils.IsReachableVertically(collider, target.collider, attackRange) &&
                     Utils.IsReachableVertically(collider, entity.collider, attackRange)))
                {
                    target = entity;
                }
            }
        }
    }
    [Server]
    public void OnReceivedDamage(Entity attacker, int damage)
    {
        OnAggro(attacker);
    }
    [ClientRpc]
    public void RpcOnAttackStarted()
    {
        if (attackSound) audioSource.PlayOneShot(attackSound);
    }
}
