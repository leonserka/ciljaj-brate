using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoundController : MonoBehaviour
{
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private TMP_Text centerText;
    [SerializeField] private ModeManager modeManager;
    [SerializeField] private PlayerWeapon weapon;
    [SerializeField] private GameObject crosshairCanvas;

    [Header("Countdown Audio")]
    [SerializeField] private AudioClip count3;
    [SerializeField] private AudioClip count2;
    [SerializeField] private AudioClip count1;
    [SerializeField] private AudioClip countFight;
    [SerializeField, Range(0f, 1f)] private float countVolume = 0.8f;

    public static RoundController Instance { get; private set; }

    private AudioSource _audio;
    private float _timeRemaining;
    private bool _roundActive;
    private bool _waitingForClick;

    public float TimeRemaining => _roundActive ? _timeRemaining : roundDuration;
    public bool RoundActive => _roundActive;

    public event System.Action<SessionStats> RoundEnded;

    private void Awake()
    {
        Instance = this;
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        if (modeManager == null) modeManager = FindAnyObjectByType<ModeManager>();
        if (weapon == null) weapon = FindAnyObjectByType<PlayerWeapon>();
        if (crosshairCanvas == null)
        {
            var cc = GameObject.Find("Crosshair Canvas");
            if (cc != null) crosshairCanvas = cc;
        }
    }

    private void Start()
    {
        ShowClickToBegin();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (_waitingForClick)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                _waitingForClick = false;
                StartCoroutine(CountdownAndStart());
            }
            return;
        }

        if (_roundActive)
        {
            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                EndRound();
            }
        }
    }

    public void ShowClickToBegin()
    {
        _waitingForClick = true;
        _roundActive = false;
        if (weapon != null) weapon.enabled = false;
        SetCrosshair(false);

        modeManager?.SwitchMode(modeManager.ActiveMode);

        if (centerText != null)
        {
            centerText.gameObject.SetActive(true);
            centerText.text = "CLICK TO BEGIN";
            centerText.fontSize = 40;
            centerText.color = new Color(1f, 1f, 1f, 0.7f);
        }
    }

    private IEnumerator CountdownAndStart()
    {
        SetCrosshair(false);

        if (centerText != null)
        {
            centerText.fontSize = 72;
            centerText.color = Color.white;

            for (int i = 3; i >= 1; i--)
            {
                centerText.text = i.ToString();
                centerText.transform.localScale = Vector3.one * 1.3f;

                PlayCount(i == 3 ? count3 : i == 2 ? count2 : count1);

                float t = 0;
                while (t < 1f)
                {
                    t += Time.deltaTime;
                    centerText.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, t);
                    centerText.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.4f, t));
                    yield return null;
                }
            }

            centerText.gameObject.SetActive(false);
            centerText.transform.localScale = Vector3.one;
        }

        PlayCount(countFight);
        StartRound();
    }

    private void StartRound()
    {
        _timeRemaining = roundDuration;
        _roundActive = true;

        if (weapon != null) weapon.enabled = true;
        SetCrosshair(true);

        modeManager?.ResetSession();
    }

    public void RestartRound() => ShowClickToBegin();

    private void EndRound()
    {
        _roundActive = false;
        if (weapon != null) weapon.enabled = false;
        modeManager?.ClearTargets();
        SetCrosshair(false);

        var sm = ModeManager.Current?.Stats;
        var stats = sm != null ? sm.CurrentStats : default(SessionStats);
        RoundEnded?.Invoke(stats);

        if (centerText != null)
        {
            centerText.gameObject.SetActive(true);
            centerText.text = "ROUND OVER";
            centerText.fontSize = 48;
            centerText.color = new Color(1f, 1f, 1f, 0.8f);
        }

        if (RoundEnded == null)
            StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        ShowClickToBegin();
    }

    private void PlayCount(AudioClip clip)
    {
        if (clip != null && _audio != null)
            _audio.PlayOneShot(clip, countVolume);
    }

    private void SetCrosshair(bool visible)
    {
        if (crosshairCanvas != null)
            crosshairCanvas.SetActive(visible);
    }
}
