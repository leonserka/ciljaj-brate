using System.Collections;
using UnityEngine;

public class RainAmbience : MonoBehaviour
{
    [Header("Rain")]
    [SerializeField] private AudioClip rainLoop;
    [SerializeField, Range(0f, 1f)] private float rainVolume = 0.3f;

    [Header("Thunder")]
    [SerializeField] private AudioClip[] thunderClips;
    [SerializeField, Range(0f, 1f)] private float thunderVolume = 0.5f;
    [SerializeField] private float thunderMinInterval = 8f;
    [SerializeField] private float thunderMaxInterval = 20f;

    [Header("Lightning Flash")]
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private float flashExposure = 2.5f;
    [SerializeField] private float flashDuration = 0.35f;

    private AudioSource _rainSource;
    private AudioSource _thunderSource;
    private float _baseExposure;
    private bool _hasSkyboxExposure;

    private void Start()
    {
        _rainSource = gameObject.AddComponent<AudioSource>();
        _rainSource.clip = rainLoop;
        _rainSource.loop = true;
        _rainSource.volume = rainVolume;
        _rainSource.spatialBlend = 0f;
        _rainSource.playOnAwake = false;
        if (rainLoop != null) _rainSource.Play();

        _thunderSource = gameObject.AddComponent<AudioSource>();
        _thunderSource.spatialBlend = 0f;
        _thunderSource.playOnAwake = false;

        if (skyboxMaterial == null)
            skyboxMaterial = RenderSettings.skybox;

        if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Exposure"))
        {
            _baseExposure = skyboxMaterial.GetFloat("_Exposure");
            _hasSkyboxExposure = true;
        }

        if (thunderClips != null && thunderClips.Length > 0)
            StartCoroutine(ThunderLoop());
    }

    private IEnumerator ThunderLoop()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));

        while (true)
        {

            var clip = thunderClips[Random.Range(0, thunderClips.Length)];
            _thunderSource.PlayOneShot(clip, thunderVolume);

            float preDelay = Random.Range(0f, 0.3f);
            yield return new WaitForSeconds(preDelay);

            int flashes = Random.Range(1, 4);
            for (int i = 0; i < flashes; i++)
            {
                yield return StartCoroutine(Flash());
                if (i < flashes - 1)
                    yield return new WaitForSeconds(Random.Range(0.15f, 0.5f));
            }

            yield return new WaitForSeconds(Random.Range(thunderMinInterval, thunderMaxInterval));
        }
    }

    private IEnumerator Flash()
    {
        if (!_hasSkyboxExposure) yield break;

        float peak = flashExposure * Random.Range(0.7f, 1f);
        float halfDur = flashDuration * 0.5f;

        float t = 0f;
        while (t < halfDur)
        {
            t += Time.deltaTime;
            float lerp = t / halfDur;
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(_baseExposure, peak, lerp));
            yield return null;
        }

        t = 0f;
        while (t < halfDur)
        {
            t += Time.deltaTime;
            float lerp = t / halfDur;
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(peak, _baseExposure, lerp));
            yield return null;
        }

        skyboxMaterial.SetFloat("_Exposure", _baseExposure);
    }

    private void OnDestroy()
    {
        if (_hasSkyboxExposure && skyboxMaterial != null)
            skyboxMaterial.SetFloat("_Exposure", _baseExposure);
    }
}
