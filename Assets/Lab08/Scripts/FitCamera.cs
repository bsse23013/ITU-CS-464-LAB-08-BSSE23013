using UnityEngine;

// Keeps the whole arena on screen, whatever the shape of the screen.
[RequireComponent(typeof(Camera))]
public class FitCamera : MonoBehaviour
{
    public Vector2 arenaSize = new Vector2(6f, 10f);
    public float margin = 0.8f;

    Camera cam;

    void Awake() { cam = GetComponent<Camera>(); }

    void LateUpdate()
    {
        float byHeight = arenaSize.y / 2f + margin;
        float byWidth = (arenaSize.x / 2f + margin) / cam.aspect;
        cam.orthographicSize = Mathf.Max(byHeight, byWidth);
    }
}
