using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// HUD listener: flashes the screen red when the player is hit.
// It listens to a static event, so the -= in OnDisable matters (Lecture 09).
public class HitFlash : MonoBehaviour
{
    public Image flash;
    public float seconds = 0.3f;
    public float strength = 0.4f;

    void OnEnable()
    {
        GameEvents.PlayerDamaged += OnDamaged;
        SetAlpha(0f);
    }

    void OnDisable()
    {
        GameEvents.PlayerDamaged -= OnDamaged;
    }

    void OnDamaged(int current, int max)
    {
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            SetAlpha(strength * (1f - t / seconds));
            yield return null;
        }
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        Color c = flash.color;
        c.a = a;
        flash.color = c;
    }
}
