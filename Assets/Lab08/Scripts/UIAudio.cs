using UnityEngine;

// One player for every UI sound, so no button needs its own AudioSource.
// The two sources output to the UI mixer group. They ignore the listener pause,
// so a click still works on the pause menu.
public class UIAudio : MonoBehaviour
{
    public static UIAudio Instance;

    public AudioSource[] sources;
    public AudioClip hover, click, back, open, close, pause, resume, success, error;

    int next;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        foreach (AudioSource s in sources) s.ignoreListenerPause = true;
    }

    // any script can call UIAudio.Play(UISound.Click)
    public static void Play(UISound sound)
    {
        if (Instance != null) Instance.PlayClip(sound);
    }

    void PlayClip(UISound sound)
    {
        AudioClip clip = GetClip(sound);
        if (clip == null) return;
        AudioSource s = sources[next];
        next = (next + 1) % sources.Length;
        // a tiny pitch change so a row of clicks does not sound like a machine
        s.pitch = Random.Range(0.97f, 1.03f);
        s.PlayOneShot(clip);
    }

    AudioClip GetClip(UISound sound)
    {
        switch (sound)
        {
            case UISound.Hover: return hover;
            case UISound.Click: return click;
            case UISound.Back: return back;
            case UISound.Open: return open;
            case UISound.Close: return close;
            case UISound.Pause: return pause;
            case UISound.Resume: return resume;
            case UISound.Success: return success;
            case UISound.Error: return error;
        }
        return null;
    }
}
