using UnityEngine;

// The win and lose jingles, played through the SFX player when the game ends.
public class ResultSounds : MonoBehaviour
{
    public AudioClip winSound, loseSound;

    void OnEnable()
    {
        GameEvents.GameWon += PlayWin;
        GameEvents.GameLost += PlayLose;
    }

    void OnDisable()
    {
        GameEvents.GameWon -= PlayWin;
        GameEvents.GameLost -= PlayLose;
    }

    void PlayWin() { SfxPlayer.Instance.Play(winSound); }
    void PlayLose() { SfxPlayer.Instance.Play(loseSound); }
}
