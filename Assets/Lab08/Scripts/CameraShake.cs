using System.Collections;
using UnityEngine;

// Shakes the camera for a moment. Uses unscaled time so it does not depend on Time.timeScale.
public class CameraShake : MonoBehaviour
{
    Vector3 home;
    Coroutine running;

    void Awake() { home = transform.localPosition; }

    public void Shake(float seconds, float strength)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Run(seconds, strength));
    }

    IEnumerator Run(float seconds, float strength)
    {
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            float fade = 1f - t / seconds;
            Vector2 offset = Random.insideUnitCircle * strength * fade;
            transform.localPosition = home + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }
        transform.localPosition = home;
    }
}
