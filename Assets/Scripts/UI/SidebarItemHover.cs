using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SidebarItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _bg;
    private bool _active;
    private Color _normalColor;
    private Color _hoverColor;
    private Color _activeColor;

    public void Init(Image bg, bool active, Color normal, Color hover, Color activeCol)
    {
        _bg = bg;
        _active = active;
        _normalColor = normal;
        _hoverColor = hover;
        _activeColor = activeCol;
    }

    public void SetActive(bool active)
    {
        _active = active;
        if (_bg != null)
            _bg.color = _active ? _activeColor : _normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_bg != null && !_active)
            _bg.color = _hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_bg != null && !_active)
            _bg.color = _normalColor;
    }
}
