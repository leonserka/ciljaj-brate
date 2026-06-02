using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _bg;
    private bool _active;
    private Color _inactiveColor;
    private static readonly Color HoverTint = new Color(0.22f, 0.08f, 0.14f, 1f);
    private static readonly Color ActiveColor = new Color(0.55f, 0.05f, 0.20f);

    public void Init(Image bg, Color inactiveColor)
    {
        _bg = bg;
        _inactiveColor = inactiveColor;
        _active = bg.color == ActiveColor;
    }

    public void SetActive(bool active)
    {
        _active = active;
        if (_bg != null)
            _bg.color = _active ? ActiveColor : _inactiveColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_bg != null && !_active)
            _bg.color = HoverTint;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_bg != null && !_active)
            _bg.color = _inactiveColor;
    }
}
