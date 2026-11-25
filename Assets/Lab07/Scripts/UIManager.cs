using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Shows one screen at a time. Every button in the scene calls one of these public methods
// (set in the Button's OnClick list in the Inspector).
// Lab 08: the Game screen now starts a real level, loaded by the SceneLoader.
public class UIManager : MonoBehaviour
{
    // 0 Title, 1 Main Menu, 2 Levels, 3 Loadout, 4 Game, 5 Results, 6 Shop, 7 Settings, 8 Credits, 9 Loading
    public GameObject[] panels;
    public GameObject pausePanel;
    public GameObject backdrop;       // the dark background; hidden while playing so the level shows through
    public SceneLoader loader;

    // the ease curves: edit them in the Inspector to change how the screens move
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve popCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.65f, 1.15f), new Keyframe(1f, 1f));
    public float duration = 0.3f;

    public TMP_Text[] levelLabels;
    public TMP_Text resultTitle;
    public Image resultBanner;
    public GameObject starsRow;
    public GameObject nextButton;
    public Shop shop;

    const int Game = 4;
    const int Loading = 9;
    int level = 1;

    void OnEnable()
    {
        GameEvents.GameWon += OnGameWon;
        GameEvents.GameLost += OnGameLost;
    }

    void OnDisable()
    {
        GameEvents.GameWon -= OnGameWon;
        GameEvents.GameLost -= OnGameLost;
    }

    void Start()
    {
        Show(0);
    }

    void Update()
    {
        // Escape pauses / resumes (handy on a PC)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && panels[Game].activeSelf)
        {
            if (pausePanel.activeSelf) Resume(); else Pause();
        }
    }

    public void Show(int index)
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (MusicPlayer.Instance != null) MusicPlayer.Instance.Duck(false);
        pausePanel.SetActive(false);
        for (int i = 0; i < panels.Length; i++) panels[i].SetActive(false);
        panels[index].SetActive(true);
        backdrop.SetActive(index != Game);

        // panels that open on top of what you were doing get the open sound
        if (index == 5 || index == 6 || index == 7 || index == 8) UIAudio.Play(UISound.Open);
        // the menus (not the game, results or loading) bring the menu music back
        if (index != Game && index != 5 && index != Loading) GameEvents.RaiseMenuShown();

        bool pop = index == Game || index == 5;     // the game and results pop up, the menus slide in
        StartCoroutine(Animate(panels[index], pop));
    }

    public void ShowLoading() { Show(Loading); }
    public void ShowGame() { Show(Game); }

    public void SelectLevel(int number)
    {
        level = number;
        GameSession.level = number;
        foreach (TMP_Text label in levelLabels) label.text = "LEVEL " + number;
        Show(3);
    }

    public void NextLevel()
    {
        SelectLevel(Mathf.Min(level + 1, 2));      // only two levels are unlocked
    }

    // the START button on the Loadout screen
    public void StartGame()
    {
        loader.Load();
    }

    public void Retry()
    {
        loader.Load();
    }

    public void QuitToMenu()
    {
        loader.Unload();
        UIAudio.Play(UISound.Close);
        Show(1);
    }

    public void Pause()
    {
        Time.timeScale = 0f;                     // the game stops, the menu animation uses unscaled time
        AudioListener.pause = true;              // sound effects stop; music and UI sounds ignore this
        MusicPlayer.Instance.Duck(true);         // the music keeps playing, quieter
        UIAudio.Play(UISound.Pause);
        pausePanel.SetActive(true);
        StartCoroutine(Animate(pausePanel, true));
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        MusicPlayer.Instance.Duck(false);
        UIAudio.Play(UISound.Resume);
        pausePanel.SetActive(false);
    }

    void OnGameWon() { StartCoroutine(ShowResultsLater(true)); }
    void OnGameLost() { StartCoroutine(ShowResultsLater(false)); }

    // wait a moment so the last sound and sparks can be seen and heard
    IEnumerator ShowResultsLater(bool won)
    {
        yield return new WaitForSecondsRealtime(1f);
        if (won) Win(); else Lose();
    }

    public void Win()
    {
        resultTitle.text = "LEVEL CLEARED!";
        resultBanner.color = new Color(1f, 0.9f, 0.5f);
        starsRow.SetActive(true);
        nextButton.SetActive(level < 2);
        shop.AddCoins(50);
        Show(5);
    }

    public void Lose()
    {
        resultTitle.text = "LEVEL FAILED";
        resultBanner.color = new Color(0.94f, 0.27f, 0.35f);
        starsRow.SetActive(false);
        nextButton.SetActive(false);
        Show(5);
    }

    IEnumerator Animate(GameObject panel, bool pop)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        RectTransform rt = panel.GetComponent<RectTransform>();
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            if (pop) rt.localScale = Vector3.one * (0.85f + 0.15f * popCurve.Evaluate(k));
            else rt.anchoredPosition = new Vector2(150f * (1f - slideCurve.Evaluate(k)), 0f);
            group.alpha = Mathf.Clamp01(k * 2f);
            yield return null;
        }
        group.alpha = 1f;
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
    }
}
