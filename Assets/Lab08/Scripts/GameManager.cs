using UnityEngine;

// Lives in the Gameplay scene. Counts the coins and decides when the level is won or lost,
// then announces it with GameEvents. Everything that reacts (music, results screen, sounds,
// the player stopping) listens for those events instead of being called from here.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Health playerHealth;

    int totalCoins;
    int coins;
    bool playing;

    void Awake()
    {
        Instance = this;
        foreach (Pickup p in FindObjectsByType<Pickup>(FindObjectsSortMode.None))
            if (p.type == PickupType.Coin) totalCoins++;
    }

    void OnEnable()
    {
        playerHealth.Died += OnPlayerDied;
    }

    void OnDisable()
    {
        playerHealth.Died -= OnPlayerDied;
        if (Instance == this) Instance = null;
    }

    // called by the SceneLoader once the scene is loaded and the Game screen is showing
    public void Begin()
    {
        coins = 0;
        playing = true;
        GameEvents.RaiseCoinCollected(0, totalCoins);
        GameEvents.RaiseGameStarted();
    }

    public void CollectCoin()
    {
        if (!playing) return;
        coins++;
        GameEvents.RaiseCoinCollected(coins, totalCoins);
        if (coins >= totalCoins) Finish(true);
    }

    void OnPlayerDied()
    {
        Finish(false);
    }

    void Finish(bool won)
    {
        if (!playing) return;
        playing = false;
        if (won) GameEvents.RaiseGameWon();
        else GameEvents.RaiseGameLost();
    }
}
