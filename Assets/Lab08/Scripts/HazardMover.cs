using UnityEngine;

// Slides a hazard back and forth between two points, once the level has started.
public class HazardMover : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 1.5f;

    bool playing;
    float t;

    void OnEnable()
    {
        GameEvents.GameStarted += OnStarted;
        GameEvents.GameWon += OnEnded;
        GameEvents.GameLost += OnEnded;
    }

    void OnDisable()
    {
        GameEvents.GameStarted -= OnStarted;
        GameEvents.GameWon -= OnEnded;
        GameEvents.GameLost -= OnEnded;
    }

    void OnStarted() { playing = true; }
    void OnEnded() { playing = false; }

    void Update()
    {
        if (!playing) return;
        t += Time.deltaTime * speed / Mathf.Max(0.1f, Vector2.Distance(pointA, pointB));
        transform.position = Vector2.Lerp(pointA, pointB, Mathf.PingPong(t, 1f));
    }
}
