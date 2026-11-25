using UnityEngine;

// One player for every gameplay sound effect (Lecture 09). Four sources are used in turn, because
// pitch belongs to the source and a new pitch would bend a sound that is still playing.
// The sources output to the SFX group and do NOT ignore the pause, so effects stop with the game.
public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance;

    public AudioSource[] sources;
    public float semitones = 2f;

    int next;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource s = sources[next];
        next = (next + 1) % sources.Length;
        // pitch = 2 ^ (semitones / 12), a random amount each time
        s.pitch = Mathf.Pow(2f, Random.Range(-semitones, semitones) / 12f);
        s.PlayOneShot(clip, volume * Random.Range(0.85f, 1f));
    }
}
