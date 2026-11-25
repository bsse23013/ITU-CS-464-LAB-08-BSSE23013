using System.Collections;
using UnityEngine;

// Listener for GameEvents.BoostStarted: a trail behind the ball while the boost lasts.
public class BoostTrail : MonoBehaviour
{
    public TrailRenderer trail;
    public float seconds = 3f;

    void OnEnable() { GameEvents.BoostStarted += OnBoost; }
    void OnDisable() { GameEvents.BoostStarted -= OnBoost; }

    void OnBoost()
    {
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        trail.emitting = true;
        yield return new WaitForSeconds(seconds);
        trail.emitting = false;
    }
}
