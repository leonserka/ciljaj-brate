using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float speed = 12f;
    [SerializeField] private bool textMode;
    public float hoverVolume = 1f;
    // Extra gain on the UI hover/click sounds so they sit a bit louder over the music.
    private const float SfxBoost = 1.3f;

    private Vector3 _baseScale;
    private Vector3 _targetScale;
    private TMP_Text _label;
    private Color _originalColor;
    private float _baseX;
    private static readonly Color HoverColor = new Color(0.55f, 0.05f, 0.20f, 1f);

    private static AudioClip _hoverClip;
    private static AudioClip _clickClip;
    private static AudioSource _sharedSource;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _targetScale = _baseScale;

        _label = GetComponentInChildren<TMP_Text>();
        if (_label != null)
            _originalColor = _label.color;

        if (_hoverClip == null)
            _hoverClip = Resources.Load<AudioClip>("UI/hover");
        if (_clickClip == null)
            _clickClip = Resources.Load<AudioClip>("UI/click");

        if (_sharedSource == null)
        {
            var go = GameObject.Find("UIAudio");
            if (go == null)
            {
                go = new GameObject("UIAudio");
                DontDestroyOnLoad(go);
                _sharedSource = go.AddComponent<AudioSource>();
                _sharedSource.playOnAwake = false;
            }
            else
                _sharedSource = go.GetComponent<AudioSource>();
        }

        // Auto-detect text mode
        var img = GetComponent<Image>();
        if (img != null && img.color.a < 0.01f)
            textMode = true;

        if (textMode)
            _baseX = transform.localPosition.x;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (textMode)
        {
            if (_label != null) _label.color = HoverColor;
            var pos = transform.localPosition;
            pos.x = _baseX + 8f;
            transform.localPosition = pos;
        }
        else
            _targetScale = _baseScale * hoverScale;

        PlayClip(_hoverClip, hoverVolume * SfxBoost);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (textMode)
        {
            if (_label != null) _label.color = _originalColor;
            var pos = transform.localPosition;
            pos.x = _baseX;
            transform.localPosition = pos;
        }
        else
            _targetScale = _baseScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!textMode)
            _targetScale = _baseScale * clickScale;
        PlayClip(_clickClip, SfxBoost);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!textMode)
            _targetScale = _baseScale * hoverScale;
    }

    private static void PlayClip(AudioClip clip, float volume)
    {
        if (clip != null && _sharedSource != null)
            _sharedSource.PlayOneShot(clip, volume);
    }
}
