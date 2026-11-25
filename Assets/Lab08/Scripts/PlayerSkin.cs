using UnityEngine;

// Uses the skin picked on the Loadout screen.
public class PlayerSkin : MonoBehaviour
{
    public SpriteRenderer ball;
    public SpriteRenderer pattern;

    void Start()
    {
        ball.color = GameSession.skinColor;
        if (string.IsNullOrEmpty(GameSession.skinPattern))
        {
            pattern.gameObject.SetActive(false);
        }
        else
        {
            pattern.sprite = Resources.Load<Sprite>("Lab07/Sprites/" + GameSession.skinPattern);
            pattern.gameObject.SetActive(true);
        }
    }
}
