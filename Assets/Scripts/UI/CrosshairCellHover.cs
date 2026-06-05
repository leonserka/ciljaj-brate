using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrosshairCellHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _bg;
    private Transform _icon;
    private bool _selected;
    private bool _hovered;
    private static readonly Color HoverColor = new Color(1f, 1f, 1f, 0.18f);
    private static readonly Color SelectedColor = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color NormalColor = new Color(1f, 1f, 1f, 0.03f);

    private static AudioClip _selectClip;
    private static AudioSource _uiAudio;
    private const float HoverVolume = 0.5f;
    private const float HoverScale = 1.15f;
    private const float ScaleSpeed = 10f;

    private Vector3 _targetScale = Vector3.one;

    public bool IsHovered => _hovered;

    public void Init(Image bg, bool selected)
    {
        _bg = bg;
        _selected = selected;
        if (transform.childCount > 0)
            _icon = transform.GetChild(0);

        if (_selectClip == null) _selectClip = Resources.Load<AudioClip>("UI/click");
        if (_uiAudio == null)
        {
            var existing = GameObject.Find("UIAudio");
            if (existing != null) _uiAudio = existing.GetComponent<AudioSource>();
        }
    }

    public void SetSelected(bool selected) => _selected = selected;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        if (_bg != null) _bg.color = HoverColor;
        _targetScale = Vector3.one * HoverScale;
        if (_selectClip != null && _uiAudio != null)
            _uiAudio.PlayOneShot(_selectClip, HoverVolume);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        if (_bg != null) _bg.color = _selected ? SelectedColor : NormalColor;
        _targetScale = Vector3.one;
    }

    private void Update()
    {
        if (_icon != null)
            _icon.localScale = Vector3.Lerp(_icon.localScale, _targetScale, Time.unscaledDeltaTime * ScaleSpeed);
    }
}
