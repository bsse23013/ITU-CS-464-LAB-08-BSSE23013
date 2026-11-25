using UnityEngine;

// Listener 1 for Health.Damaged: the hurt sound.
public class HurtSound : MonoBehaviour
{
    public Health health;
    public AudioClip hurt;

    void OnEnable() { health.Damaged += OnDamaged; }
    void OnDisable() { health.Damaged -= OnDamaged; }

    void OnDamaged(int current, int max)
    {
        if (SfxPlayer.Instance != null) SfxPlayer.Instance.Play(hurt);
    }
}
