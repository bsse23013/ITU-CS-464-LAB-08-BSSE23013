using UnityEngine;

// Listener: when any pickup is collected, play its sound and spawn a sparkle where it was.
public class PickupFeedback : MonoBehaviour
{
    public AudioClip coinSound, heartSound, boostSound;
    public GameObject sparklePrefab;

    void OnEnable() { Pickup.Collected += OnCollected; }
    void OnDisable() { Pickup.Collected -= OnCollected; }

    void OnCollected(Pickup p)
    {
        AudioClip clip = coinSound;
        if (p.type == PickupType.Heart) clip = heartSound;
        if (p.type == PickupType.Boost) clip = boostSound;
        if (SfxPlayer.Instance != null) SfxPlayer.Instance.Play(clip);

        // the prefab destroys itself when it finishes (Stop Action: Destroy)
        if (sparklePrefab != null) Instantiate(sparklePrefab, p.transform.position, Quaternion.identity);
    }
}
