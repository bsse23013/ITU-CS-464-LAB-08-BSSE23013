using UnityEngine;

// Picks the music for each state. Listens to the game events and crossfades:
// menu music -> game music when a level starts -> results music when it is won or lost
// -> menu music again when the menu comes back.
public class MusicController : MonoBehaviour
{
    public AudioClip menuMusic, gameMusic, resultsMusic;

    void OnEnable()
    {
        GameEvents.MenuShown += PlayMenu;
        GameEvents.GameStarted += PlayGame;
        GameEvents.GameWon += PlayResults;
        GameEvents.GameLost += PlayResults;
    }

    void OnDisable()
    {
        GameEvents.MenuShown -= PlayMenu;
        GameEvents.GameStarted -= PlayGame;
        GameEvents.GameWon -= PlayResults;
        GameEvents.GameLost -= PlayResults;
    }

    void PlayMenu() { MusicPlayer.Instance.Play(menuMusic); }
    void PlayGame() { MusicPlayer.Instance.Play(gameMusic); }
    void PlayResults() { MusicPlayer.Instance.Play(resultsMusic); }
}
