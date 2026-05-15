using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour, IShootable
{
    private static AudioClip _fallbackHitClip;

    [SerializeField] private float deathDuration = 0.1f;
    [SerializeField] private float spawnEaseDuration = 0.08f;
    [SerializeField] private float punchScale = 1.15f;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private AudioClip hitClip;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 0.55f;

    public event Action<Target> Died;

    private Renderer _renderer;
    private Collider[] _colliders;
    private Color _baseColor;
    private Vector3 _baseScale;
    private int _hitCount;
    private bool _dead;

    public int HitCount => _hitCount;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _colliders = GetComponentsInChildren<Collider>();
        _baseScale = transform.localScale;

        if (_renderer != null && _renderer.material.HasProperty("_BaseColor"))
            _baseColor = _renderer.material.GetColor("_BaseColor");

        if (hitClip == null)
            hitClip = GetFallbackHitClip();
    }

    private void OnEnable()
    {
        StartCoroutine(SpawnEase());
    }

    public void OnShot(RaycastHit hit, Vector3 fromDirection)
    {
        if (_dead)
            return;

        _hitCount++;
        _dead = true;

        foreach (Collider targetCollider in _colliders)
            targetCollider.enabled = false;

        if (hitClip != null)
            AudioSource.PlayClipAtPoint(hitClip, hit.point, hitVolume);

        Died?.Invoke(this);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator SpawnEase()
    {
        float elapsed = 0f;
        transform.localScale = Vector3.zero;

        while (elapsed < spawnEaseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spawnEaseDuration);
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, _baseScale, EaseOutBack(t));
            yield return null;
        }

        transform.localScale = _baseScale;
    }

    private IEnumerator DeathRoutine()
    {
        if (_renderer != null && _renderer.material.HasProperty("_BaseColor"))
            _renderer.material.SetColor("_BaseColor", hitColor);

        Vector3 punchedScale = _baseScale * punchScale;
        transform.localScale = punchedScale;

        float elapsed = 0f;
        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deathDuration);
            transform.localScale = Vector3.Lerp(punchedScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private static AudioClip GetFallbackHitClip()
    {
        if (_fallbackHitClip != null)
            return _fallbackHitClip;

        const int sampleRate = 44100;
        const int sampleCount = 3308;
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 28f);
            float tone = Mathf.Sin(2f * Mathf.PI * 980f * t) * 0.55f;
            float click = Mathf.Sin(2f * Mathf.PI * 2200f * t) * 0.18f;
            samples[i] = Mathf.Clamp((tone + click) * envelope, -1f, 1f);
        }

        _fallbackHitClip = AudioClip.Create("SFX_Hit_Target_Fallback", sampleCount, 1, sampleRate, false);
        _fallbackHitClip.SetData(samples, 0);
        return _fallbackHitClip;
    }
}
