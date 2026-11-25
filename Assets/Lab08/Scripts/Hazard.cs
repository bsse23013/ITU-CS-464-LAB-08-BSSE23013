using UnityEngine;

// A red crusher. It hurts the player while they touch it (Health has the cool-down).
public class Hazard : MonoBehaviour
{
    public int damage = 1;
    public float spinSpeed = 180f;

    void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Health h = other.GetComponent<Health>();
        if (h != null) h.TakeDamage(damage);
    }
}
