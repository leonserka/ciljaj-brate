using UnityEngine;

public class LobbyMusic : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip startupClip;
    [SerializeField] private float maxVolume = 0.3f;
    [SerializeField] private float fadeInDuration = 5f;
    [SerializeField] private float fadeOutDuration = 3f;

    public static LobbyMusic Instance { get; private set; }

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private float _fadeTarget;
    private float _fadeSpeed;
    private bool _musicStarted;
    private float _musicDelay;

    private void Awake()
    {
        Instance = this;
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.clip = musicClip;
        _musicSource.loop = false;
        _musicSource.playOnAwake = false;
        _musicSource.volume = 0f;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
    }

    private void Start()
    {
        if (startupClip != null)
        {
            _sfxSource.PlayOneShot(startupClip);
            _musicDelay = startupClip.length;
        }
    }

    private void Update()
    {
        if (!_musicStarted)
        {
            _musicDelay -= Time.unscaledDeltaTime;
            if (_musicDelay <= 0f)
            {
                _musicStarted = true;
                _musicSource.Play();
                FadeTo(maxVolume, fadeInDuration);
            }
            return;
        }

        _musicSource.volume = Mathf.MoveTowards(_musicSource.volume, _fadeTarget, _fadeSpeed * Time.unscaledDeltaTime);

        if (!_musicSource.isPlaying) return;

        float timeLeft = _musicSource.clip.length - _musicSource.time;
        if (timeLeft <= fadeOutDuration && _fadeTarget > 0f)
            FadeTo(0f, fadeOutDuration);

        if (timeLeft <= 0.05f)
        {
            _musicSource.Stop();
            _musicSource.time = 0f;
            _musicSource.Play();
            FadeTo(maxVolume, fadeInDuration);
        }
    }

    public void FadeOutAndStop(float duration)
    {
        _musicStarted = true;
        FadeTo(0f, duration);
    }

    public bool IsSilent => _musicSource == null || _musicSource.volume <= 0.01f;

    private void FadeTo(float target, float duration)
    {
        _fadeTarget = target;
        _fadeSpeed = duration > 0f ? maxVolume / duration : maxVolume;
    }
}
