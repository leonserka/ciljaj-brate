using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField, Min(60f)] private float fireRateRpm = 600f;
    [SerializeField] private float maxRange = 200f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private AudioClip shotClip;
    [SerializeField, Range(0f, 1f)] private float shotVolume = 0.45f;
    [SerializeField] private Animator weaponAnimator;
    [Tooltip("Root of the arms + gun mesh. The PlayerWeapon script usually lives on a " +
             "separate object, so the viewmodel renderers are NOT its children. Leave " +
             "empty to auto-detect a sibling with renderers.")]
    [SerializeField] private GameObject viewmodelRoot;

    private AudioSource _audio;
    private InputAction _fireAction;
    private float _nextShotTime;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (playerCamera == null) playerCamera = Camera.main;

        SetViewmodelVisible(GameplaySettings.ShowViewmodel);
    }

    // Shows/hides the arms + gun viewmodel. Called from Awake and from the
    // settings toggle (PauseMenu) so the "show weapon viewmodel" option works.
    public void SetViewmodelVisible(bool show)
    {
        var root = ResolveViewmodelRoot();
        if (root == null) return;
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer) continue;
            r.enabled = show;
        }
    }

    private GameObject ResolveViewmodelRoot()
    {
        if (viewmodelRoot != null) return viewmodelRoot;

        // The viewmodel mesh is typically a sibling of this object (both parented
        // to the camera). Pick the sibling that actually carries renderers.
        Transform parent = transform.parent;
        if (parent != null)
        {
            foreach (Transform sib in parent)
            {
                if (sib == transform) continue;
                if (sib.GetComponentInChildren<Renderer>(true) != null)
                {
                    viewmodelRoot = sib.gameObject;
                    return viewmodelRoot;
                }
            }
        }
        return null;
    }

    private void OnEnable()
    {
        _fireAction = InputSystem.actions?.FindAction("Fire");
        _fireAction?.Enable();
    }

    private void OnDisable()
    {
        _fireAction?.Disable();
    }

    private void OnValidate()
    {
        fireRateRpm = Mathf.Max(60f, fireRateRpm);
        maxRange = Mathf.Max(1f, maxRange);
    }

    private void Update()
    {
        bool fire = _fireAction != null && _fireAction.WasPressedThisFrame();
        if (!fire)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.zKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame))
                fire = true;
        }
        if (fire && ModeManager.Current?.ActiveMode?.AllowShooting != false) TryFire();
    }

    private void TryFire()
    {
        if (Time.time < _nextShotTime) return;
        _nextShotTime = Time.time + 60f / fireRateRpm;

        // When the viewmodel is hidden, the weapon is meant to be "gone" — mute its fire sound too.
        if (_audio != null && shotClip != null && GameplaySettings.ShowViewmodel)
            _audio.PlayOneShot(shotClip, shotVolume);
        weaponAnimator?.SetTrigger("Fire");

        if (playerCamera == null) return;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            LastShotPoint = hit.point;
            IShootable shootable = hit.collider.GetComponentInParent<IShootable>();
            ModeManager.Current?.HandleShot(shootable is Target);
            if (shootable != null) shootable.OnShot(hit, ray.direction);
        }
        else
        {
            LastShotPoint = ray.origin + ray.direction * 20f;
            ModeManager.Current?.HandleShot(false);
        }
    }

    public static Vector3 LastShotPoint { get; private set; }
}
