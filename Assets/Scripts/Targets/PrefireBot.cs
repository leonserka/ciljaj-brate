using UnityEngine;

[RequireComponent(typeof(Target), typeof(AudioSource))]
public class PrefireBot : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float firstShotDelay = 0.3f;
    [SerializeField] private int healOnDeath = 20;
    [SerializeField] private AudioClip fireClip;

    private const float MuzzleForward = 0.55f;
    private const float MuzzleHeight = 0.95f;
    private const float MuzzleRight = 0.05f;
    private const float BotEyeHeight = 1.05f;
    private const float PlayerHeadHeight = 0.75f;
    private const float BulletSpread = 0.15f;

    private Transform _playerTransform;
    private PlayerHealth _playerHealth;
    private Target _target;
    private AudioSource _audioSource;
    private float _nextFireTime;
    private bool _hasSeenPlayer;
    private int _losMask;

    private static Material _tracerMaterial;

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
        }

        _losMask = ~((1 << gameObject.layer) | (1 << LayerMask.NameToLayer("Player")));
        _nextFireTime = Time.time + Random.Range(0f, fireInterval);

        if (_tracerMaterial == null)
        {
            _tracerMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _tracerMaterial.color = new Color(1f, 1f, 1f, 0.35f);
            _tracerMaterial.SetFloat("_Surface", 1f);
            _tracerMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _tracerMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _tracerMaterial.renderQueue = 3000;
            _tracerMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
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

        if (fireClip != null)
            _audioSource.PlayOneShot(fireClip);

        Vector3 muzzle = transform.position
            + transform.forward * MuzzleForward
            + Vector3.up * MuzzleHeight
            + transform.right * MuzzleRight;

        Vector3 target = _playerTransform.position + Vector3.up * PlayerHeadHeight;
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
        lr.sharedMaterial = _tracerMaterial;
        lr.startWidth = 0.003f;
        lr.endWidth = 0.003f;
        lr.numCapVertices = 2;
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, go.transform.InverseTransformPoint(to));
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        Destroy(go, 0.05f);
    }

    private bool HasLineOfSight()
    {
        Vector3 botEye = transform.position + Vector3.up * BotEyeHeight;
        Vector3 playerCenter = _playerTransform.position + Vector3.up * PlayerHeadHeight;
        Vector3 toBot = botEye - playerCenter;
        return !Physics.Raycast(playerCenter, toBot.normalized, toBot.magnitude, _losMask, QueryTriggerInteraction.Ignore);
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
