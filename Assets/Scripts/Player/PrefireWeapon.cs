using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PrefireWeapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxRange = 200f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator viewmodelAnimator;
    [SerializeField] private Transform barrelTip;

    [Header("Audio")]
    [SerializeField] private AudioClip[] fireClips;
    [SerializeField] private AudioClip[] headshotClips;
    [SerializeField] private AudioClip[] bodyHitClips;
    [SerializeField] private AudioClip killDoofClip;

    [Header("Damage")]
    [SerializeField] private int bodyDamage = 28;
    [SerializeField] private int headshotDamage = 112;
    [SerializeField] private CanvasGroup killFlashOverlay;

    private const float FireRateRpm = 600f;
    private const float FireInterval = 60f / FireRateRpm;
    private const float RecoilRecoveryDelay = 0.5f;
    private const float BaseInaccuracy = 0.006f;
    private const float MoveInaccuracy = 0.08f;
    private const float JumpInaccuracy = 0.25f;
    private const float SprayInaccuracyPerShot = 0.003f;
    private const float RecoilPitchPerShot = 0.55f;

    private AudioSource _audio;
    private AudioSource _hitAudio;
    private InputAction _fireAction;
    private InputAction _moveAction;
    private InputAction _crouchAction;
    private PlayerLook _look;
    private CharacterController _cc;
    private Material _tracerMat;

    private int _sprayIndex;
    private float _nextShotTime;
    private float _lastShotTime;
    private Coroutine _flashRoutine;

    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");

    private static readonly Vector2[] RecoilPattern = new Vector2[]
    {
        new(-0.2f, -1.0f),
        new(-0.1f, -1.1f),
        new( 0.0f, -1.2f),
        new( 0.1f, -1.3f),
        new( 0.0f, -1.2f),
        new(-0.1f, -1.1f),
        new( 0.2f, -1.0f),
        new( 0.3f, -0.9f),
        new( 0.6f, -0.7f),
        new( 0.8f, -0.5f),
        new( 0.9f, -0.4f),
        new( 0.7f, -0.3f),
        new( 0.3f, -0.2f),
        new(-0.3f, -0.2f),
        new(-0.7f, -0.1f),
        new(-0.9f, -0.1f),
        new(-0.8f,  0.0f),
        new(-0.5f,  0.0f),
        new( 0.2f, -0.1f),
        new( 0.6f, -0.1f),
        new( 0.8f,  0.0f),
        new( 0.5f,  0.0f),
        new( 0.0f,  0.0f),
        new(-0.5f,  0.0f),
        new(-0.7f,  0.0f),
        new(-0.4f,  0.0f),
        new( 0.3f,  0.0f),
        new( 0.5f,  0.0f),
        new( 0.2f,  0.0f),
        new(-0.2f,  0.0f),
    };

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _cc = GetComponentInParent<CharacterController>();
        _look = GetComponentInParent<PlayerLook>();
        if (playerCamera == null) playerCamera = Camera.main;

        _hitAudio = gameObject.AddComponent<AudioSource>();
        _hitAudio.spatialBlend = 0f;
        _hitAudio.playOnAwake = false;

        _tracerMat = new Material(Shader.Find("Sprites/Default"));

        muzzleFlash = GetComponentInChildren<ParticleSystem>(true);
        if (muzzleFlash != null)
        {
            muzzleFlash.gameObject.SetActive(true);
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // Kept for PrefireManager route resets — clears recoil/spray state.
    public void ResetAmmo()
    {
        _sprayIndex = 0;
    }

    private void OnEnable()
    {
        _fireAction = InputSystem.actions?.FindAction("Fire");
        _fireAction?.Enable();
        _moveAction = InputSystem.actions?.FindAction("Move");
        _moveAction?.Enable();
        _crouchAction = InputSystem.actions?.FindAction("Crouch");
        _crouchAction?.Enable();
    }

    private void OnDisable()
    {
        _fireAction?.Disable();
        _moveAction?.Disable();
        _crouchAction?.Disable();
    }

    private void Update()
    {
        if (Time.time - _lastShotTime > RecoilRecoveryDelay && _sprayIndex > 0)
            _sprayIndex = 0;

        bool fireHeld = _fireAction != null && _fireAction.IsPressed();
        var kb = Keyboard.current;
        bool zHeld = false, xHeld = false;
        if (kb != null)
        {
            zHeld = kb.zKey.isPressed;
            xHeld = kb.xKey.isPressed;
        }

        if ((fireHeld || zHeld || xHeld) && Time.time >= _nextShotTime)
            TryFire();
    }

    private void TryFire()
    {
        _nextShotTime = Time.time + FireInterval;
        _lastShotTime = Time.time;

        if (_audio != null && fireClips != null && fireClips.Length > 0)
            _audio.PlayOneShot(fireClips[Random.Range(0, fireClips.Length)]);

        if (muzzleFlash != null) muzzleFlash.Play();
        if (viewmodelAnimator != null) viewmodelAnimator.SetTrigger(ShootTrigger);

        float inaccuracy = CalculateInaccuracy();

        if (_look != null && _sprayIndex < RecoilPattern.Length)
        {
            var recoil = RecoilPattern[_sprayIndex];
            _look.ApplyRecoilPunch(recoil.y * RecoilPitchPerShot, recoil.x * RecoilPitchPerShot);
        }

        if (playerCamera == null) return;
        var camT = playerCamera.transform;
        Vector3 forward = camT.forward;
        Vector3 right = camT.right;
        Vector3 up = camT.up;

        Vector2 spread = Random.insideUnitCircle * inaccuracy;
        Vector3 dir = (forward + right * spread.x + up * spread.y).normalized;

        Ray ray = new Ray(camT.position, dir);
        Vector3 hitPoint = ray.origin + dir * maxRange;
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;
            var target = hit.collider.GetComponentInParent<Target>();
            ModeManager.Current?.HandleShot(target != null);

            if (target != null && !target.IsDead)
            {
                bool headshot = CheckHeadshot(hit, target.transform);
                int damage = headshot ? headshotDamage : bodyDamage;
                target.LastHitLabel = headshot ? "HS" : "BODY";
                int hpBefore = target.Health;
                target.OnShot(hit, ray.direction, damage);
                Debug.Log($"[HIT] {target.name} | {(headshot ? "HEADSHOT" : "BODY")} | dmg={damage} | hp {hpBefore}->{target.Health} | dead={target.IsDead} | hitY={hit.point.y:F2} | collider={hit.collider.name}");

                if (target.IsDead)
                {
                    if (headshot && headshotClips != null && headshotClips.Length > 0)
                        _hitAudio.PlayOneShot(headshotClips[Random.Range(0, headshotClips.Length)], 1f);
                    if (killDoofClip != null)
                        _hitAudio.PlayOneShot(killDoofClip, 0.5f);
                    if (_flashRoutine != null) StopCoroutine(_flashRoutine);
                    _flashRoutine = StartCoroutine(KillFlash());
                }
                else if (bodyHitClips != null && bodyHitClips.Length > 0)
                {
                    _hitAudio.PlayOneShot(bodyHitClips[Random.Range(0, bodyHitClips.Length)]);
                }
            }
            else
            {
                Debug.Log($"[MISS] hit {hit.collider.name} (no Target) at {hit.point}");
            }
        }
        else
        {
            ModeManager.Current?.HandleShot(false);
        }

        Vector3 tracerOrigin = barrelTip != null
            ? barrelTip.position
            : camT.position + forward * 0.5f;
        SpawnTracer(tracerOrigin, hitPoint);

        _sprayIndex = Mathf.Min(_sprayIndex + 1, RecoilPattern.Length - 1);
    }

    private bool CheckHeadshot(RaycastHit hit, Transform botTransform)
    {
        // Headshot is decided by which collider the bullet actually struck.
        // The bot has a dedicated "Head" SphereCollider; everything else
        // (the body CapsuleCollider) is a body shot. No height guessing.
        return hit.collider != null && hit.collider.gameObject.name == "Head";
    }

    private Texture2D _vignetteTexture;

    private void EnsureVignetteTexture()
    {
        if (_vignetteTexture != null) return;
        const int size = 256;
        _vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01((dist - 0.3f) / 0.7f);
                a = a * a;
                _vignetteTexture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        _vignetteTexture.Apply();

        var img = killFlashOverlay.GetComponent<Image>();
        if (img != null)
        {
            var rect = new Rect(0, 0, size, size);
            img.sprite = Sprite.Create(_vignetteTexture, rect, new Vector2(0.5f, 0.5f));
            img.type = Image.Type.Simple;
        }
    }

    private IEnumerator KillFlash()
    {
        if (killFlashOverlay == null) yield break;
        EnsureVignetteTexture();
        killFlashOverlay.alpha = 0.18f;
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            killFlashOverlay.alpha = Mathf.Lerp(0.18f, 0f, t / 0.2f);
            yield return null;
        }
        killFlashOverlay.alpha = 0f;
        _flashRoutine = null;
    }

    private float CalculateInaccuracy()
    {
        bool crouching = _crouchAction != null && _crouchAction.IsPressed();
        if (crouching) return 0f;

        float acc = BaseInaccuracy;

        bool grounded = _cc != null && _cc.isGrounded;
        if (!grounded)
        {
            acc += JumpInaccuracy;
        }
        else
        {
            bool moving = _moveAction != null && _moveAction.ReadValue<Vector2>().sqrMagnitude > 0.01f;
            if (moving) acc += MoveInaccuracy;
        }

        acc += _sprayIndex * SprayInaccuracyPerShot;
        return acc;
    }

    private void SpawnTracer(Vector3 from, Vector3 to)
    {
        var go = new GameObject("Tracer");
        go.transform.position = from;
        go.transform.LookAt(to);

        var lr = go.AddComponent<LineRenderer>();
        lr.material = _tracerMat;
        // Width is in world units; a near point ~1m from the camera needs a few
        // mm to read as a thin-but-visible line instead of a sub-pixel sliver.
        lr.startWidth = 0.004f;
        lr.endWidth = 0.004f;
        lr.numCapVertices = 1;
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.startColor = new Color(1f, 0.95f, 0.8f, 0.9f);
        lr.endColor = new Color(1f, 0.9f, 0.7f, 0.35f);
        Destroy(go, 0.04f);
    }
}
