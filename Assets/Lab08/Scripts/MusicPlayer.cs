using System.Collections;
using UnityEngine;

// Background music on two looping sources, so one track can fade out while the next fades in.
// The fade is equal power (cos / sin), which has no dip in the middle. It runs on unscaled time,
// so it also works while the game is paused. Both sources ignore the listener pause: the music keeps
// playing under the pause menu, just quieter (the "duck").
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    public AudioSource[] sources;      // two sources, both output to the Music group
    public float fadeTime = 1.5f;
    public float duckedVolume = 0.35f;

    float[] gain = new float[2];
    int current;
    float duck = 1f;
    float duckTarget = 1f;
    Coroutine fade;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        foreach (AudioSource s in sources)
        {
            s.loop = true;
            s.playOnAwake = false;
            s.ignoreListenerPause = true;
            s.volume = 0f;
        }
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (sources[current].clip == clip && sources[current].isPlaying) return;   // already playing it

        int from = current;
        int to = 1 - current;
        sources[to].clip = clip;
        gain[to] = 0f;
        sources[to].volume = 0f;
        sources[to].Play();
        current = to;

        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Crossfade(from, to));
    }

    // the pause menu calls this: true = quieter music, false = normal
    public void Duck(bool on)
    {
        duckTarget = on ? duckedVolume : 1f;
    }

    IEnumerator Crossfade(int from, int to)
    {
        float startGain = gain[from];
        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            float k = t / fadeTime;
            gain[from] = startGain * Mathf.Cos(k * Mathf.PI / 2f);
            gain[to] = Mathf.Sin(k * Mathf.PI / 2f);
            yield return null;
        }
        gain[from] = 0f;
        gain[to] = 1f;
        sources[from].Stop();
    }

    void Update()
    {
        duck = Mathf.MoveTowards(duck, duckTarget, Time.unscaledDeltaTime * 3f);
        for (int i = 0; i < sources.Length; i++) sources[i].volume = gain[i] * duck;
    }
}
