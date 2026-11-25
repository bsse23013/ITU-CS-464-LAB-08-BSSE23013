using System;
using UnityEngine;

public enum PickupType { Coin, Heart, Boost }

// A coin, a heart or a speed boost. When the player touches it, it announces Collected and switches off.
// The sound and the sparkle are NOT played from here (a sound on the pickup would be cut off when it
// switches off, "Gone with the coin"): PickupFeedback listens to Collected and plays them.
public class Pickup : MonoBehaviour
{
    public static event Action<Pickup> Collected;

    public PickupType type;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (type == PickupType.Heart)
        {
            Health h = other.GetComponent<Health>();
            if (h == null || h.Current >= h.maxHealth) return;       // full health: leave it for later
            h.Heal(1);
        }

        Collected?.Invoke(this);
        gameObject.SetActive(false);

        if (type == PickupType.Coin && GameManager.Instance != null) GameManager.Instance.CollectCoin();
        if (type == PickupType.Boost) GameEvents.RaiseBoostStarted();
    }
}
