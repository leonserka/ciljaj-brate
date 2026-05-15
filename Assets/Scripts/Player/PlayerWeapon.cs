using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField, Min(60f)] private float fireRateRpm = 600f;
    [SerializeField] private float maxRange = 200f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip shotClip;

    private AudioSource _audio;
    private InputAction _fireAction;
    private float _nextShotTime;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (shotClip == null) shotClip = GenerateShotClip();

        muzzleFlash = GetComponentInChildren<ParticleSystem>(true);
        if (muzzleFlash != null)
        {
            muzzleFlash.gameObject.SetActive(true);
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
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
        if (_fireAction != null && _fireAction.WasPressedThisFrame()) TryFire();
    }

    private void TryFire()
    {
        if (Time.time < _nextShotTime) return;
        _nextShotTime = Time.time + 60f / fireRateRpm;

        if (muzzleFlash != null) muzzleFlash.Play();
        if (_audio != null && shotClip != null) _audio.PlayOneShot(shotClip);

        if (playerCamera == null) return;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            IShootable shootable = hit.collider.GetComponentInParent<IShootable>();
            if (shootable != null) shootable.OnShot(hit, ray.direction);
        }
    }

    private static AudioClip GenerateShotClip()
    {
        const int sampleRate = 44100;
        const int sampleCount = 2205;
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Exp(-t * 14f);
            float noise = (Random.value * 2f - 1f) * 0.6f;
            float pulse = Mathf.Sin(t * 60f) * 0.35f;
            samples[i] = Mathf.Clamp((noise + pulse) * envelope, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("ProceduralShot", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
