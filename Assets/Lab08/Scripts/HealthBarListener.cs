using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD listener: the health bar (a filled Image) follows the player's health events.
public class HealthBarListener : MonoBehaviour
{
    public Image fill;
    public TMP_Text label;
    public int startHealth = 3;
    public Color full = new Color(0.56f, 0.9f, 0.49f);
    public Color low = new Color(0.94f, 0.27f, 0.35f);

    void OnEnable()
    {
        GameEvents.PlayerDamaged += Show;
        GameEvents.PlayerHealed += Show;
        GameEvents.GameStarted += ResetBar;
        Show(startHealth, startHealth);
    }

    void OnDisable()
    {
        GameEvents.PlayerDamaged -= Show;
        GameEvents.PlayerHealed -= Show;
        GameEvents.GameStarted -= ResetBar;
    }

    void ResetBar() { Show(startHealth, startHealth); }

    void Show(int current, int max)
    {
        float f = (float)current / max;
        fill.fillAmount = f;
        fill.color = Color.Lerp(low, full, f);
        label.text = current + "/" + max;
    }
}
