using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One of the four volume sliders in Settings. It shows the saved value, sends changes to
// VolumeSettings, and plays a short sample when you let go so you can hear the new level.
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour, IPointerUpHandler
{
    public string parameter;           // MasterVolume, MusicVolume, SfxVolume or UiVolume
    public TMP_Text valueLabel;
    public AudioClip sfxSample;        // only for the SFX slider

    Slider slider;

    void OnEnable()
    {
        slider = GetComponent<Slider>();
        slider.SetValueWithoutNotify(VolumeSettings.GetSaved(parameter));
        ShowValue(slider.value);
        slider.onValueChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnChanged);
    }

    void OnChanged(float value)
    {
        if (VolumeSettings.Instance != null) VolumeSettings.Instance.SetVolume(parameter, value);
        ShowValue(value);
    }

    void ShowValue(float value)
    {
        if (valueLabel != null) valueLabel.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (parameter == VolumeSettings.Sfx && SfxPlayer.Instance != null) SfxPlayer.Instance.Play(sfxSample);
        else if (parameter != VolumeSettings.Music) UIAudio.Play(UISound.Click);
    }
}
