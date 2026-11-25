using TMPro;
using UnityEngine;

// HUD listener: the "3/8" coin counter.
public class CoinCounter : MonoBehaviour
{
    public TMP_Text label;

    void OnEnable() { GameEvents.CoinCollected += Show; }
    void OnDisable() { GameEvents.CoinCollected -= Show; }

    void Show(int collected, int total)
    {
        label.text = collected + "/" + total;
    }
}
