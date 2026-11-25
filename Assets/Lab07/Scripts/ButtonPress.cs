using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Feedback on every button: it shrinks a little while pressed (and buzzes on a phone).
// Lab 08: it also plays the UI sounds, through the one UIAudio player (no AudioSource on the button).
public class ButtonPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
    public UISound pressSound = UISound.Click;     // Back for the back buttons, None when the code plays its own

    public void OnPointerEnter(PointerEventData eventData)
    {
        Selectable s = GetComponent<Selectable>();
        if (s != null && !s.interactable) return;
        if (!Application.isMobilePlatform) UIAudio.Play(UISound.Hover);    // no hover on a touch screen
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Selectable s = GetComponent<Selectable>();
        if (s != null && !s.interactable)
        {
            UIAudio.Play(UISound.Error);       // a locked level
            return;
        }
        transform.localScale = Vector3.one * 0.94f;
        UIAudio.Play(pressSound);
#if UNITY_ANDROID || UNITY_IOS
        if (SettingsMenu.vibrate) Handheld.Vibrate();
#endif
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }
}
