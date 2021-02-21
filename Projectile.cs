
using UnityEngine;
using Mirror;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : NetworkBehaviourNonAlloc
{
    public Rigidbody rigidBody;
#pragma warning disable CS0109 
    new public Collider collider;
#pragma warning restore CS0109 
    public float speed = 1;
    public float destroyAfter = 10; 
    [SyncVar, HideInInspector] public GameObject owner;
    [SyncVar, HideInInspector] public Vector3 direction;
    [HideInInspector] public int damage = 1; 
    void Start()
    {
        foreach (Collider co in owner.GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(collider, co);
        Invoke(nameof(DestroySelf), destroyAfter);
    }
    void FixedUpdate()
    {
        rigidBody.MovePosition(Vector3.MoveTowards(transform.position, transform.position + direction, speed * Time.fixedDeltaTime));
        transform.LookAt(transform.position + direction);
    }
    void OnTriggerEnter(Collider co)
    {
        if (isServer)
        {
            Entity entity = co.GetComponentInParent<Entity>();
            if (entity != null && entity.health.current > 0)
            {
                Combat casterCombat = owner.GetComponent<Combat>();
                casterCombat.DealDamageAt(entity,
                                          casterCombat.damage + damage, 
                                          transform.position, 
                                          -direction, 
                                          co); 
            }
        }
        Destroy(gameObject);
    }
    void DestroySelf()
    {
        Destroy(gameObject);
    }
}