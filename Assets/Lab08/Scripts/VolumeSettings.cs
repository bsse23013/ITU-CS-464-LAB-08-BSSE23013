using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Holds the four mixer volumes. The settings sliders give 0 to 1, the mixer wants decibels,
// so every value goes through ToDecibels (20 * log10). Values are saved in PlayerPrefs.
public class VolumeSettings : MonoBehaviour
{
    public static VolumeSettings Instance;

    public const string Master = "MasterVolume";   // the names exposed in the Audio Mixer
    public const string Music = "MusicVolume";
    public const string Sfx = "SfxVolume";
    public const string Ui = "UiVolume";

    public AudioMixer mixer;
    public Toggle muteToggle;

    bool muted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        muted = PlayerPrefs.GetInt("Lab08.Muted", 0) == 1;
    }

    void Start()
    {
        // SetFloat is reliable from Start on
        foreach (string p in new[] { Master, Music, Sfx, Ui }) mixer.SetFloat(p, ToDecibels(GetSaved(p)));
        ApplyMute();
        if (muteToggle != null) muteToggle.SetIsOnWithoutNotify(muted);
    }

    public static float DefaultFor(string parameter)
    {
        if (parameter == Music) return 0.7f;
        if (parameter == Ui) return 0.8f;
        return 1f;
    }

    // saved slider value (0.0001 to 1), also used by the sliders before the mixer is ready
    public static float GetSaved(string parameter)
    {
        return PlayerPrefs.GetFloat("Lab08." + parameter, DefaultFor(parameter));
    }

    // slider 0..1 -> decibels. The slider minimum is 0.0001 because Log10(0) is minus infinity;
    // 0.0001 gives exactly -80 dB, which is silence in the mixer.
    public static float ToDecibels(float slider)
    {
        return Mathf.Log10(Mathf.Clamp(slider, 0.0001f, 1f)) * 20f;
    }

    public void SetVolume(string parameter, float slider)
    {
        slider = Mathf.Clamp(slider, 0.0001f, 1f);
        PlayerPrefs.SetFloat("Lab08." + parameter, slider);
        PlayerPrefs.Save();
        if (parameter == Master && muted) return;          // stay silent while muted
        mixer.SetFloat(parameter, ToDecibels(slider));
    }

    // wired to the "Mute all" tick box. The saved Master slider value is kept, so unmuting restores it.
    public void SetMuted(bool on)
    {
        muted = on;
        PlayerPrefs.SetInt("Lab08.Muted", on ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
    }

    void ApplyMute()
    {
        mixer.SetFloat(Master, muted ? -80f : ToDecibels(GetSaved(Master)));
    }
}
