
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public abstract class Energy : NetworkBehaviourNonAlloc
{
    [SyncVar] int _current = 0;
    public int current
    {
        get { return Mathf.Min(_current, max); }
        set
        {
            bool emptyBefore = _current == 0;
            _current = Mathf.Clamp(value, 0, max);
            if (_current == 0 && !emptyBefore) onEmpty.Invoke();
        }
    }
    public abstract int max { get; }
    public int recoveryTickRate = 1;
    public abstract int recoveryPerTick { get; }
    public Health health;
    public bool spawnFull = true;
    [Header("Events")]
    public UnityEvent onEmpty;
    public override void OnStartServer()
    {
        if (spawnFull) current = max;
        InvokeRepeating(nameof(Recover), recoveryTickRate, recoveryTickRate);
    }
    public float Percent() =>
        (current != 0 && max != 0) ? (float)current / (float)max : 0;
    [Server]
    public void Recover()
    {
        if (enabled && health.current > 0)
        {
            int next = current + recoveryPerTick;
            current = next;
        }
    }
}
