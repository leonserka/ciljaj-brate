using System.Collections;
using UnityEngine;

public class TestShootableTarget : MonoBehaviour, IShootable
{
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private float punchScale = 1.15f;

    private Renderer _renderer;
    private Color _baseColor;
    private Vector3 _baseScale;
    private Coroutine _flashRoutine;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _baseScale = transform.localScale;

        if (_renderer != null && _renderer.material.HasProperty("_BaseColor"))
            _baseColor = _renderer.material.GetColor("_BaseColor");
    }

    public void OnShot(RaycastHit hit, Vector3 fromDirection)
    {
        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);

        _flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        transform.localScale = _baseScale * punchScale;

        if (_renderer != null && _renderer.material.HasProperty("_BaseColor"))
            _renderer.material.SetColor("_BaseColor", hitColor);

        yield return new WaitForSeconds(flashDuration);

        transform.localScale = _baseScale;

        if (_renderer != null && _renderer.material.HasProperty("_BaseColor"))
            _renderer.material.SetColor("_BaseColor", _baseColor);

        _flashRoutine = null;
    }
}
