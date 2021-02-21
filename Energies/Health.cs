using UnityEngine;
using Mirror;
public interface IHealthBonus
{
    int GetHealthBonus(int baseHealth);
    int GetHealthRecoveryBonus();
}
[DisallowMultipleComponent]
public class Health : Energy
{
    public int baseRecoveryPerTick = 0;
    public int baseHealth = 100;
    IHealthBonus[] _bonusComponents;
    IHealthBonus[] bonusComponents =>
        _bonusComponents ?? (_bonusComponents = GetComponents<IHealthBonus>());
    public override int max
    {
        get
        {
            int bonus = 0;
            foreach (IHealthBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetHealthBonus(baseHealth);
            return baseHealth + bonus;
        }
    }
    public override int recoveryPerTick
    {
        get
        {
            int bonus = 0;
            foreach (IHealthBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetHealthRecoveryBonus();
            return baseRecoveryPerTick + bonus;
        }
    }
}