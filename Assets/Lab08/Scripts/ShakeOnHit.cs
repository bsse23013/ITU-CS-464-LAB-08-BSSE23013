using UnityEngine;

// Listener 3 for Health.Damaged: shakes the camera.
public class ShakeOnHit : MonoBehaviour
{
    public Health health;
    public CameraShake shake;

    void OnEnable() { health.Damaged += OnDamaged; }
    void OnDisable() { health.Damaged -= OnDamaged; }

    void OnDamaged(int current, int max)
    {
        shake.Shake(0.25f, 0.3f);
    }
}
