using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using UnityEngine.Serialization;
public interface ICombatBonus
{
    int GetDamageBonus();
    int GetDefenseBonus();
}
public class Combat : NetworkBehaviourNonAlloc
{
    [Header("Components")]
    public Entity entity;
    [Header("Stats")]
    public int baseDamage;
    public int baseDefense;
    public GameObject onDamageEffect;
    [SyncVar] public double lastCombatTime;
    public UnityEventEntityInt onServerReceivedDamage;
    public UnityEventInt onClientReceivedDamage;
    ICombatBonus[] bonusComponents;
    void Awake()
    {
        bonusComponents = GetComponentsInChildren<ICombatBonus>();
    }
    public int damage
    {
        get
        {
            int bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetDamageBonus();
            return baseDamage + bonus;
        }
    }
    public int defense
    {
        get
        {
            int bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetDefenseBonus();
            return baseDefense + bonus;
        }
    }
    public void DealDamageAt(Entity victim, int amount, Vector3 hitPoint, Vector3 hitNormal, Collider hitCollider)
    {
        Combat victimCombat = victim.combat;
        if (victim.health.current > 0)
        {
            DamageArea damageArea = hitCollider.GetComponent<DamageArea>();
            float multiplier = damageArea != null ? damageArea.multiplier : 1;
            int amountMultiplied = Mathf.RoundToInt(amount * multiplier);
            int damageDealt = Mathf.Max(amountMultiplied - victimCombat.defense, 1);
            victim.health.current -= damageDealt;
            victimCombat.onServerReceivedDamage.Invoke(entity, damageDealt);
            victimCombat.RpcOnReceivedDamage(damageDealt, hitPoint, hitNormal);
            lastCombatTime = NetworkTime.time;
            victimCombat.lastCombatTime = NetworkTime.time;
        }
    }
    [ClientRpc]
    public void RpcOnReceivedDamage(int amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (onDamageEffect)
            Instantiate(onDamageEffect, hitPoint, Quaternion.LookRotation(-hitNormal));
        onClientReceivedDamage.Invoke(amount);
    }
}