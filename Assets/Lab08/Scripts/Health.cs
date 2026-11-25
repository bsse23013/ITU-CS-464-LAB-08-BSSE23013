using System;
using UnityEngine;

// The player's health. It only says WHAT happened (damaged, healed, died). It does not know about the
// sound, the sparks, the shake or the HUD: those are separate listener components that subscribe.
public class Health : MonoBehaviour
{
    public int maxHealth = 3;
    public float immuneSeconds = 1f;       // after a hit the player cannot be hit again for a moment

    public int Current { get; private set; }
    public bool Dead { get { return Current <= 0; } }

    // events for the scripts in the Gameplay scene (sound, sparks, shake)
    public event Action<int, int> Damaged;   // current, max
    public event Action<int, int> Healed;
    public event Action Died;

    float immuneUntil;

    void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (Dead || Time.time < immuneUntil) return;
        Current = Mathf.Max(0, Current - amount);
        immuneUntil = Time.time + immuneSeconds;

        Damaged?.Invoke(Current, maxHealth);
        GameEvents.RaisePlayerDamaged(Current, maxHealth);      // for the HUD in the menu scene
        if (Current == 0) Died?.Invoke();
    }

    public void Heal(int amount)
    {
        if (Dead || Current >= maxHealth) return;
        Current = Mathf.Min(maxHealth, Current + amount);
        Healed?.Invoke(Current, maxHealth);
        GameEvents.RaisePlayerHealed(Current, maxHealth);
    }
}
