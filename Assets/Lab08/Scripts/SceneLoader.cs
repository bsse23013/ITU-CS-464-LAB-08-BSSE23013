using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Loads the Gameplay scene in the background behind the loading screen (Lecture 10).
// op.progress only goes from 0 to 0.9 while loading, so the bar shows progress / 0.9.
// allowSceneActivation stays false until the bar is full, so the player cannot get into the game early.
// The scene is loaded additively on top of the menu scene, so the menu, the HUD and the audio stay alive.
public class SceneLoader : MonoBehaviour
{
    public string sceneName = "Gameplay";
    public UIManager ui;
    public Image barFill;
    public TMP_Text percentText;
    public TMP_Text tipText;
    public string[] tips;
    public float minSeconds = 1f;          // so the bar is readable even when loading is instant

    bool loading;

    public void Load()
    {
        if (loading) return;
        StartCoroutine(LoadRoutine());
    }

    public void Unload()
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded) SceneManager.UnloadSceneAsync(sceneName);
    }

    IEnumerator LoadRoutine()
    {
        loading = true;
        ui.ShowLoading();
        SetBar(0f);
        if (tips != null && tips.Length > 0) tipText.text = tips[Random.Range(0, tips.Length)];

        // replaying: get rid of the old copy first
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(sceneName);
            while (!unload.isDone) yield return null;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        float shown = 0f;
        float time = 0f;
        while (true)
        {
            float real = Mathf.Clamp01(op.progress / 0.9f);
            shown = Mathf.MoveTowards(shown, real, Time.unscaledDeltaTime * 1.5f);   // never ahead of the real progress
            time += Time.unscaledDeltaTime;
            SetBar(shown);
            if (shown >= 1f && time >= minSeconds) break;
            yield return null;
        }

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        loading = false;
        ui.ShowGame();
        if (GameManager.Instance != null) GameManager.Instance.Begin();
        else Debug.LogError("[SceneLoader] No GameManager in the Gameplay scene");
    }

    void SetBar(float value)
    {
        barFill.fillAmount = value;
        percentText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
