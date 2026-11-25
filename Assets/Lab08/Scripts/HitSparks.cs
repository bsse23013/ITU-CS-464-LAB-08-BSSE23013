using UnityEngine;

// Listener 2 for Health.Damaged: a burst of sparks on the player.
public class HitSparks : MonoBehaviour
{
    public Health health;
    public ParticleSystem sparks;

    void OnEnable() { health.Damaged += OnDamaged; }
    void OnDisable() { health.Damaged -= OnDamaged; }

    void OnDamaged(int current, int max)
    {
        sparks.Emit(24);
    }
}
