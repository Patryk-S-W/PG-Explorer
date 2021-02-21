
using UnityEngine;
[RequireComponent(typeof(Collider))] 
public class AggroArea : MonoBehaviour
{
    public Monster owner; 
    void OnTriggerEnter(Collider co)
    {
        Entity entity = co.GetComponentInParent<Entity>();
        if (entity) owner.OnAggro(entity);
    }
    void OnTriggerStay(Collider co)
    {
        Entity entity = co.GetComponentInParent<Entity>();
        if (entity) owner.OnAggro(entity);
    }
}
