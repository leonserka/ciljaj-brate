using UnityEngine;

[RequireComponent(typeof(Target), typeof(AudioSource))]
public class PrefireBot : MonoBehaviour
{
    [SerializeField] private int damage = 5;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float firstShotDelay = 0.3f;
    [SerializeField] private int healOnDeath = 20;
    [SerializeField] private AudioClip fireClip;

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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var head = transform.Find("Head");
        if (head != null)
        {
            var sc = head.GetComponent<SphereCollider>();
            if (sc != null)
            {
                UnityEditor.Handles.color = new Color(1f, 0.15f, 0.15f, 0.85f);
                var wc = head.TransformPoint(sc.center);
                float wr = sc.radius * head.lossyScale.x;
                UnityEditor.Handles.DrawWireDisc(wc, Vector3.up, wr);
                UnityEditor.Handles.DrawWireDisc(wc, Vector3.forward, wr);
                UnityEditor.Handles.DrawWireDisc(wc, Vector3.right, wr);
            }
        }
        var cc = GetComponent<CapsuleCollider>();
        if (cc != null)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.4f);
            Gizmos.matrix = transform.localToWorldMatrix;
            float halfHeight = Mathf.Max(0f, cc.height * 0.5f - cc.radius);
            Gizmos.DrawWireSphere(cc.center + Vector3.up * halfHeight, cc.radius);
            Gizmos.DrawWireSphere(cc.center - Vector3.up * halfHeight, cc.radius);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
#endif
}
