using System;

// Static events that scripts in different scenes listen to (the HUD and the menu live in the
// menu scene, the game lives in the Gameplay scene). A C# event can only be invoked from the class
// that declares it, so the Raise... methods let the publishers fire them.
// Every listener must subscribe in OnEnable and unsubscribe in OnDisable, because a static event
// outlives the scene (Lecture 09, "Static events outlive the scene").
public static class GameEvents
{
    public static event Action GameStarted;
    public static event Action GameWon;
    public static event Action GameLost;
    public static event Action MenuShown;
    public static event Action<int, int> PlayerDamaged;   // current, max
    public static event Action<int, int> PlayerHealed;    // current, max
    public static event Action<int, int> CoinCollected;   // collected, total
    public static event Action BoostStarted;

    public static void RaiseGameStarted() { GameStarted?.Invoke(); }
    public static void RaiseGameWon() { GameWon?.Invoke(); }
    public static void RaiseGameLost() { GameLost?.Invoke(); }
    public static void RaiseMenuShown() { MenuShown?.Invoke(); }
    public static void RaisePlayerDamaged(int current, int max) { PlayerDamaged?.Invoke(current, max); }
    public static void RaisePlayerHealed(int current, int max) { PlayerHealed?.Invoke(current, max); }
    public static void RaiseCoinCollected(int collected, int total) { CoinCollected?.Invoke(collected, total); }
    public static void RaiseBoostStarted() { BoostStarted?.Invoke(); }
}
