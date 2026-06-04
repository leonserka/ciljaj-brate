using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrefireHUD : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image healthBarFill;

    [Header("Route Info")]
    [SerializeField] private TMP_Text routeText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text botsText;

    [Header("Damage Flash")]
    [SerializeField] private CanvasGroup damageFlash;

    private PlayerHealth _playerHealth;
    private PrefireManager _prefireManager;
    private float _displayHealth;
    private float _flashAlpha;

    private static readonly Color HealthGreen = new Color(0.35f, 0.85f, 0.35f);
    private static readonly Color HealthYellow = new Color(0.95f, 0.85f, 0.2f);
    private static readonly Color HealthRed = new Color(0.9f, 0.2f, 0.15f);

    private void Start()
    {
        _playerHealth = PlayerHealth.Instance;
        if (_playerHealth == null)
            _playerHealth = FindAnyObjectByType<PlayerHealth>();

        _prefireManager = PrefireManager.Instance;
        if (_prefireManager == null)
            _prefireManager = FindAnyObjectByType<PrefireManager>();

        if (_playerHealth != null)
        {
            _playerHealth.HealthChanged += OnHealthChanged;
            _displayHealth = _playerHealth.CurrentHealth;
            UpdateHealthDisplay(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }

        if (_prefireManager != null)
        {
            _prefireManager.RouteStarted += OnRouteStarted;
            _prefireManager.RouteCleared += OnRouteCleared;
            UpdateRouteDisplay();
        }
    }

    private void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.HealthChanged -= OnHealthChanged;
        if (_prefireManager != null)
        {
            _prefireManager.RouteStarted -= OnRouteStarted;
            _prefireManager.RouteCleared -= OnRouteCleared;
        }
    }

    private void Update()
    {
        if (_playerHealth != null)
        {
            _displayHealth = Mathf.MoveTowards(_displayHealth, _playerHealth.CurrentHealth, 120f * Time.deltaTime);
            float t = _displayHealth / _playerHealth.MaxHealth;
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = t;
                healthBarFill.color = GetHealthColor(t);
            }
        }

        if (_flashAlpha > 0f)
        {
            _flashAlpha = Mathf.MoveTowards(_flashAlpha, 0f, 2f * Time.deltaTime);
            if (damageFlash != null) damageFlash.alpha = _flashAlpha;
        }

        if (_prefireManager != null)
        {
            UpdateTimerDisplay();
            UpdateBotsDisplay();
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (current < _displayHealth)
            _flashAlpha = 0.35f;
        UpdateHealthDisplay(current, max);
    }

    private void UpdateHealthDisplay(int current, int max)
    {
        healthText.text = current.ToString();
        healthText.color = GetHealthColor((float)current / max);
    }

    private void OnRouteStarted(int index, int total) => UpdateRouteDisplay();
    private void OnRouteCleared(int index, float time) => UpdateRouteDisplay();

    private void UpdateRouteDisplay()
    {
        if (_prefireManager == null) return;
        string name = _prefireManager.CurrentRouteName;
        int idx = _prefireManager.CurrentRouteIndex + 1;
        int total = _prefireManager.TotalRoutes;
        routeText.text = name + "  " + idx + "/" + total;
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        float t = _prefireManager.RouteTime;
        int sec = Mathf.FloorToInt(t);
        int ms = Mathf.FloorToInt((t - sec) * 100f);
        timerText.text = sec.ToString("00") + "." + ms.ToString("00");
    }

    private void UpdateBotsDisplay()
    {
        if (botsText == null) return;
        botsText.text = _prefireManager.BotsRemaining.ToString();
    }

    private Color GetHealthColor(float t)
    {
        return t > 0.3f ? Color.white : HealthRed;
    }
}
