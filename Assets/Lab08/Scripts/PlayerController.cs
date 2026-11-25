using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Moves the ball with WASD / arrow keys, or towards the finger / mouse while it is pressed.
// It only moves between GameStarted and GameWon / GameLost.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Camera gameCamera;
    public float speed = 4.5f;
    public float boostMultiplier = 1.6f;
    public float boostSeconds = 3f;

    Rigidbody2D rb;
    Vector2 move;
    bool playing;
    float boostUntil;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        GameEvents.GameStarted += OnStarted;
        GameEvents.GameWon += OnEnded;
        GameEvents.GameLost += OnEnded;
        GameEvents.BoostStarted += OnBoost;
    }

    void OnDisable()
    {
        GameEvents.GameStarted -= OnStarted;
        GameEvents.GameWon -= OnEnded;
        GameEvents.GameLost -= OnEnded;
        GameEvents.BoostStarted -= OnBoost;
    }

    void OnStarted() { playing = true; }
    void OnEnded() { playing = false; move = Vector2.zero; rb.linearVelocity = Vector2.zero; }
    void OnBoost() { boostUntil = Time.time + boostSeconds; }

    void Update()
    {
        move = playing ? ReadInput() : Vector2.zero;
    }

    void FixedUpdate()
    {
        float s = Time.time < boostUntil ? speed * boostMultiplier : speed;
        rb.linearVelocity = move * s;
    }

    Vector2 ReadInput()
    {
        Vector2 d = Vector2.zero;
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) d.x -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) d.x += 1f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed) d.y -= 1f;
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) d.y += 1f;
        }
        if (d != Vector2.zero) return d.normalized;

        // touch / mouse: roll towards the pressed point (but not when pressing a UI button)
        Pointer p = Pointer.current;
        if (p != null && p.press.isPressed && gameCamera != null)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return Vector2.zero;
            Vector3 world = gameCamera.ScreenToWorldPoint(p.position.ReadValue());
            Vector2 to = (Vector2)world - rb.position;
            if (to.magnitude > 0.15f) return to.normalized;
        }
        return Vector2.zero;
    }
}
