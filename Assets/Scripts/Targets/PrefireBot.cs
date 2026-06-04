using TMPro;
using UnityEngine;

[RequireComponent(typeof(Target), typeof(AudioSource))]
public class PrefireBot : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float firstShotDelay = 0.3f;
    [SerializeField] private int healOnDeath = 20;
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private Material tracerMaterial;

    private const float MuzzleForward = 0.55f;
    private const float MuzzleHeight = 0.95f;
    private const float MuzzleRight = 0.05f;
    private const float BotEyeHeight = 1.05f;
    private const float PlayerHeadHeightFallback = 0.75f;
    private const float BulletSpread = 0.15f;

    private Transform _playerTransform;
    private PlayerHealth _playerHealth;
    private PlayerController _playerController;
    private Target _target;
    private AudioSource _audioSource;
    private float _nextFireTime;
    private bool _hasSeenPlayer;
    private int _losMask;
    private TextMeshProUGUI _healthText;
    private Transform _healthCanvas;
    private int _prevHealth;
    private float _hitLabelTimer;

    private void Awake()
    {
        _target = GetComponent<Target>();
        _target.Died += OnDied;
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            _playerTransform = pc.transform;
            _playerHealth = pc.GetComponent<PlayerHealth>();
            _playerController = pc;
        }

        _losMask = ~((1 << gameObject.layer) | (1 << LayerMask.NameToLayer("Player")));
        _nextFireTime = Time.time + Random.Range(0f, fireInterval);

        _prevHealth = _target.Health;
        CreateHealthDisplay();
    }

    private void Update()
    {
        if (_playerTransform == null || _playerHealth == null) return;
        if (_playerHealth.IsDead) return;

        FacePlayer();

        if (Time.time < _nextFireTime) return;

        if (HasLineOfSight())
        {
            if (!_hasSeenPlayer)
            {
                _hasSeenPlayer = true;
                _nextFireTime = Time.time + firstShotDelay;
                return;
            }

            _nextFireTime = Time.time + fireInterval;
            Fire();
        }
        else
        {
            _hasSeenPlayer = false;
        }
    }

    private void FacePlayer()
    {
        Vector3 lookDir = _playerTransform.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    private void Fire()
    {
        _playerHealth.TakeDamage(damage);

        if (fireClip != null && _audioSource != null && _audioSource.enabled && _audioSource.gameObject.activeInHierarchy)
            _audioSource.PlayOneShot(fireClip);

        Vector3 muzzle = transform.position
            + transform.forward * MuzzleForward
            + Vector3.up * MuzzleHeight
            + transform.right * MuzzleRight;

        Vector3 target = _playerTransform.position + Vector3.up * GetPlayerHeadHeight();
        target.x += Random.Range(-BulletSpread, BulletSpread);
        target.y += Random.Range(-BulletSpread * 0.66f, BulletSpread * 0.66f);
        target.z += Random.Range(-BulletSpread, BulletSpread);

        SpawnTracer(muzzle, target);


    }

    private void SpawnTracer(Vector3 from, Vector3 to)
    {
        var go = new GameObject("Tracer");
        var cam = Camera.main;
        if (cam != null)
            go.transform.SetParent(cam.transform, true);
        go.transform.position = from;
        go.transform.LookAt(to);

        var lr = go.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.0012f;
        lr.endWidth = 0.0012f;
        lr.numCapVertices = 2;
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, go.transform.InverseTransformPoint(to));
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.startColor = new Color(1f, 0.7f, 0.3f, 0.5f);
        lr.endColor = new Color(1f, 0.7f, 0.3f, 0.1f);
        Destroy(go, 0.05f);
    }

    private float GetPlayerHeadHeight()
    {
        return _playerController != null ? _playerController.CameraHeight : PlayerHeadHeightFallback;
    }

    private bool HasLineOfSight()
    {
        Vector3 botEye = transform.position + Vector3.up * BotEyeHeight;
        Vector3 playerCenter = _playerTransform.position + Vector3.up * GetPlayerHeadHeight();
        Vector3 toBot = botEye - playerCenter;
        return !Physics.Raycast(playerCenter, toBot.normalized, toBot.magnitude, _losMask, QueryTriggerInteraction.Ignore);
    }

    private void LateUpdate()
    {
        if (_healthCanvas == null) return;
        var cam = Camera.main;
        if (cam == null) return;
        _healthCanvas.LookAt(_healthCanvas.position + cam.transform.forward);

        int hp = _target.Health;
        if (hp != _prevHealth)
        {
            _prevHealth = hp;
            _hitLabelTimer = 1.5f;
        }

        if (_hitLabelTimer > 0f)
        {
            _hitLabelTimer -= Time.deltaTime;
            string label = _target.LastHitLabel ?? "";
            bool isHs = label == "HS";
            _healthText.color = isHs ? Color.red : Color.yellow;
            _healthText.text = $"{hp} {label}";
        }
        else
        {
            _healthText.color = Color.white;
            _healthText.text = hp.ToString();
        }
    }

    private void CreateHealthDisplay()
    {
        var canvasGo = new GameObject("HealthCanvas");
        canvasGo.transform.SetParent(transform, false);
        canvasGo.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.01f;

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        var rt = canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 40f);

        var textGo = new GameObject("HealthText");
        textGo.transform.SetParent(canvasGo.transform, false);
        _healthText = textGo.AddComponent<TextMeshProUGUI>();
        _healthText.text = _target.Health.ToString();
        _healthText.fontSize = 36;
        _healthText.color = Color.white;
        _healthText.alignment = TextAlignmentOptions.Center;
        _healthText.enableWordWrapping = false;
        _healthText.outlineWidth = 0.3f;
        _healthText.outlineColor = Color.black;

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(100f, 40f);
        textRt.anchoredPosition = Vector2.zero;

        _healthCanvas = canvasGo.transform;
    }

    private void OnDied(Target t)
    {
        t.Died -= OnDied;
        if (_playerHealth != null && !_playerHealth.IsDead)
            _playerHealth.Heal(healOnDeath);
    }

    private void OnDestroy()
    {
        if (_target != null) _target.Died -= OnDied;
    }
}
