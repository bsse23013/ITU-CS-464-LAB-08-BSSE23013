using UnityEngine;

// What the menu picked, so the Gameplay scene can read it after it loads.
public static class GameSession
{
    public static int level = 1;
    public static Color skinColor = new Color(0.235f, 0.81f, 0.77f);
    public static string skinPattern = null;     // sprite name in Resources/Lab07/Sprites, or null for a plain ball
}
