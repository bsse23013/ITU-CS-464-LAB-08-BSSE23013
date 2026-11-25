using TMPro;
using UnityEngine;

// The coin balance and the three shop items.
public class Shop : MonoBehaviour
{
    public int coins = 300;
    public TMP_Text[] coinTexts;      // every place that shows the balance
    public int[] prices;
    public TMP_Text[] buyLabels;      // the price on each Buy button, which becomes OWNED
    public TMP_Text message;

    bool[] owned;

    void Start()
    {
        owned = new bool[prices.Length];
        ShowCoins();
    }

    public void Buy(int i)
    {
        if (owned[i]) { message.text = "You already own this"; UIAudio.Play(UISound.Error); return; }
        if (coins < prices[i]) { message.text = "Not enough coins"; UIAudio.Play(UISound.Error); return; }
        coins -= prices[i];
        owned[i] = true;
        buyLabels[i].text = "OWNED";
        message.text = "Bought!";
        UIAudio.Play(UISound.Success);
        ShowCoins();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        ShowCoins();
    }

    void ShowCoins()
    {
        foreach (TMP_Text t in coinTexts) t.text = coins.ToString();
    }
}
