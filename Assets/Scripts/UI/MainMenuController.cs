using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private CanvasGroup modePanel;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private CanvasGroup gameModePanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button aimTrainingButton;
    [SerializeField] private Button prefireButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button gameModeBackButton;

    private const float TransitionTime = 0.3f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowPanel(mainPanel, false);
        HidePanel(modePanel, false);
        HidePanel(settingsPanel, false);
        HidePanel(gameModePanel, false);

        playButton?.onClick.AddListener(OnPlay);
        settingsButton?.onClick.AddListener(OnSettings);
        quitButton?.onClick.AddListener(OnQuit);
        aimTrainingButton?.onClick.AddListener(OnAimTraining);
        prefireButton?.onClick.AddListener(OnPrefire);
        backButton?.onClick.AddListener(OnBack);
        settingsBackButton?.onClick.AddListener(OnSettingsBack);
        gameModeBackButton?.onClick.AddListener(OnGameModeBack);
    }

    private void OnPlay()
    {
        StartCoroutine(CrossFade(mainPanel, modePanel));
    }

    private void OnBack()
    {
        StartCoroutine(CrossFade(modePanel, mainPanel));
    }

    private void OnSettings()
    {
        StartCoroutine(CrossFade(mainPanel, settingsPanel));
    }

    private void OnSettingsBack()
    {
        StartCoroutine(CrossFade(settingsPanel, mainPanel));
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnAimTraining()
    {
        StartCoroutine(CrossFade(modePanel, gameModePanel));
    }

    private void OnGameModeBack()
    {
        StartCoroutine(CrossFade(gameModePanel, modePanel));
    }

    private void OnPrefire()
    {
        StartCoroutine(LoadScene("Prefire"));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        if (LobbyMusic.Instance != null)
            LobbyMusic.Instance.FadeOutAndStop(0.8f);

        yield return FadeOut(modePanel);

        while (LobbyMusic.Instance != null && !LobbyMusic.Instance.IsSilent)
            yield return null;

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator CrossFade(CanvasGroup from, CanvasGroup to)
    {
        yield return FadeOut(from);
        yield return FadeIn(to);
    }

    private IEnumerator FadeIn(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.interactable = false;
        group.blocksRaycasts = false;
        float t = 0;
        while (t < TransitionTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = t / TransitionTime;
            yield return null;
        }
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private IEnumerator FadeOut(CanvasGroup group)
    {
        group.interactable = false;
        float t = 0;
        while (t < TransitionTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = 1f - t / TransitionTime;
            yield return null;
        }
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);
    }

    private void ShowPanel(CanvasGroup group, bool animate)
    {
        group.gameObject.SetActive(true);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void HidePanel(CanvasGroup group, bool animate)
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);
    }
}
