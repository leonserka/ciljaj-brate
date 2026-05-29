using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _bg;
    private Outline _outline;
    private Vector3 _targetScale = Vector3.one;

    private static readonly Color Normal = new Color(0.08f, 0.08f, 0.10f, 0.95f);
    private static readonly Color Hover = new Color(0.12f, 0.12f, 0.15f, 0.95f);
    private static readonly Color OutlineNormal = new Color(1f, 1f, 1f, 0.04f);
    private static readonly Color OutlineHover = new Color(0.85f, 0.12f, 0.20f, 0.25f);

    private static AudioClip _hoverClip;
    private static AudioSource _uiAudio;

    public void Init(Image bg, Outline outline)
    {
        _bg = bg;
        _outline = outline;
        if (_hoverClip == null) _hoverClip = Resources.Load<AudioClip>("UI/hover");
        if (_uiAudio == null)
        {
            var existing = GameObject.Find("UIAudio");
            if (existing != null) _uiAudio = existing.GetComponent<AudioSource>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_bg != null) _bg.color = Hover;
        if (_outline != null) _outline.effectColor = OutlineHover;
        _targetScale = Vector3.one * 1.02f;
        if (_hoverClip != null && _uiAudio != null)
            _uiAudio.PlayOneShot(_hoverClip, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_bg != null) _bg.color = Normal;
        if (_outline != null) _outline.effectColor = OutlineNormal;
        _targetScale = Vector3.one;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * 10f);
    }
}
