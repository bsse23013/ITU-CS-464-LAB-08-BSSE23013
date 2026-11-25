using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Loadout screen: tapping a skin changes the big preview and moves the orange frame.
// Lab 08: the choice is kept in GameSession so the ball in the game uses it.
public class SkinPicker : MonoBehaviour
{
    public Image preview;
    public TMP_Text skinName;
    public RectTransform marker;
    public RectTransform[] tiles;
    public Color[] colors;
    public string[] names;
    public string[] patterns;         // sprite name for each skin, empty for the plain one

    void Start()
    {
        Pick(0);
    }

    public void Pick(int i)
    {
        preview.color = colors[i];
        skinName.text = names[i];
        marker.localPosition = tiles[i].localPosition;
        GameSession.skinColor = colors[i];
        GameSession.skinPattern = patterns[i];
    }
}
