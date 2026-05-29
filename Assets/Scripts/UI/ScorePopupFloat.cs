using TMPro;
using UnityEngine;

public class ScorePopupFloat : MonoBehaviour
{
    private float _riseSpeed;   // world units / sec
    private float _lifetime;
    private float _fadeDelay;
    private float _elapsed;
    private Vector3 _worldPos;
    private Camera _cam;
    private RectTransform _parentRect;
    private RectTransform _rect;
    private TMP_Text _text;

    public void Init(Vector3 worldPos, Camera cam, RectTransform parentRect, float riseSpeed, float lifetime, float fadeDelay)
    {
        _worldPos = worldPos;
        _cam = cam;
        _parentRect = parentRect;
        _riseSpeed = riseSpeed;
        _lifetime = lifetime;
        _fadeDelay = fadeDelay;
    }

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        // Rise in world space so the popup stays anchored to the spot it appeared
        _worldPos += Vector3.up * _riseSpeed * Time.deltaTime;

        if (_cam != null && _parentRect != null)
        {
            Vector3 sp = _cam.WorldToScreenPoint(_worldPos);
            if (sp.z <= 0f)
            {
                // behind camera - hide
                if (_text != null) { var cc = _text.color; cc.a = 0f; _text.color = cc; }
            }
            else
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, sp, null, out var local))
                    _rect.anchoredPosition = local;
            }
        }

        if (_elapsed > _fadeDelay && _text != null)
        {
            float fadeProgress = (_elapsed - _fadeDelay) / (_lifetime - _fadeDelay);
            var c = _text.color;
            c.a = Mathf.Lerp(1f, 0f, fadeProgress);
            _text.color = c;
        }

        if (_elapsed >= _lifetime)
            Destroy(gameObject);
    }
}
