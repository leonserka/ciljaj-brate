using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RouteCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _bg;
    private bool _active;
    private Vector3 _targetScale = Vector3.one;

    private static readonly Color HoverColor = new Color(0.24f, 0.08f, 0.14f, 1f);
    private static readonly Color ActiveColor = new Color(0.50f, 0.06f, 0.20f, 1f);
    private static readonly Color NormalColor = new Color(0.12f, 0.12f, 0.14f, 1f);
    private const float HoverScale = 1.03f;
    private const float ScaleSpeed = 8f;

    public void Init(Image bg, bool active)
    {
        _bg = bg;
        _active = active;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = Vector3.one * HoverScale;
        if (_bg != null && !_active)
            _bg.color = HoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = Vector3.one;
        if (_bg != null && !_active)
            _bg.color = NormalColor;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * ScaleSpeed);
    }
}
