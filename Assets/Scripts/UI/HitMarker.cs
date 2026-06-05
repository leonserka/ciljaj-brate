using UnityEngine;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour
{
    [SerializeField] private float displayTime = 0.15f;
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float expandScale = 1.5f;
    [SerializeField] private float randomOffset = 25f;
    [SerializeField] private AudioClip hitSound;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 0.65f;

    private CanvasGroup _group;
    private RectTransform _rect;
    private Vector3 _baseScale;
    private Vector2 _centerPos;
    private float _timer;
    private bool _showing;
    private AudioSource _audio;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _baseScale = _rect.localScale;
        _centerPos = _rect.anchoredPosition;

        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;

        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
    }

    private StatsManager _stats;

    private void OnEnable()
    {
        _stats = StatsManager.Instance != null ? StatsManager.Instance : FindAnyObjectByType<StatsManager>();
        if (_stats != null)
            _stats.StatsChanged += OnStats;
    }

    private void Start()
    {
        if (_stats == null)
        {
            _stats = StatsManager.Instance != null ? StatsManager.Instance : FindAnyObjectByType<StatsManager>();
            if (_stats != null)
                _stats.StatsChanged += OnStats;
        }
    }

    private void OnDisable()
    {
        if (_stats != null)
            _stats.StatsChanged -= OnStats;
    }

    private void OnStats(SessionStats stats)
    {
        if (stats.LastShotHit && ModeManager.Current?.ActiveMode?.ShowHitFeedback != false)
        {
            _timer = displayTime;
            _showing = true;
            _group.alpha = 1f;
            _rect.localScale = _baseScale * expandScale;

            _rect.anchoredPosition = _centerPos + new Vector2(
                Random.Range(-randomOffset, randomOffset),
                Random.Range(-randomOffset, randomOffset));

            if (hitSound != null)
                _audio.PlayOneShot(hitSound, hitVolume);
        }
        else { }
    }

    private void Update()
    {
        if (!_showing) return;

        _timer -= Time.deltaTime;
        _rect.localScale = Vector3.Lerp(_rect.localScale, _baseScale, fadeSpeed * Time.deltaTime);

        if (_timer <= 0f)
        {
            _group.alpha = Mathf.MoveTowards(_group.alpha, 0f, fadeSpeed * Time.deltaTime);
            if (_group.alpha <= 0.01f)
            {
                _group.alpha = 0f;
                _showing = false;
            }
        }
    }
}
