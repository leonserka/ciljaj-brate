using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    private const float InchesPerCentimeter = 1f / 2.54f;

    [SerializeField] private Transform body;
    [SerializeField] private Camera playerCamera;
    [SerializeField, Min(1f)] private float sensCm360 = 50f;
    [SerializeField, Min(100)] private int dpi = 800;
    [SerializeField, Range(60f, 120f)] private float fov = 90f;
    [SerializeField] private bool smoothingEnabled = false;
    [SerializeField, Range(1, 5)] private int smoothingFrames = 2;
    [SerializeField] private bool lockCursorOnEnable = true;

    private InputAction _lookAction;
    private float _pitch;
    private Vector2 _smoothedDelta;

    private void Awake()
    {
        if (body == null) body = transform;
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        ApplyCameraSettings();
    }

    private void OnEnable()
    {
        _lookAction = InputSystem.actions?.FindAction("Look");
        _lookAction?.Enable();

        if (lockCursorOnEnable)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDisable()
    {
        _lookAction?.Disable();
    }

    private void OnValidate()
    {
        sensCm360 = Mathf.Max(1f, sensCm360);
        dpi = Mathf.Max(100, dpi);
        ApplyCameraSettings();
    }

    private void Update()
    {
        Vector2 delta = _lookAction != null ? _lookAction.ReadValue<Vector2>() : Vector2.zero;

        if (smoothingEnabled && smoothingFrames > 1)
        {
            float alpha = 1f / smoothingFrames;
            _smoothedDelta = Vector2.Lerp(_smoothedDelta, delta, alpha);
            delta = _smoothedDelta;
        }
        else
        {
            _smoothedDelta = delta;
        }

        float degPerCount = CalculateDegreesPerCount(sensCm360, dpi);
        float yaw = delta.x * degPerCount;
        float pitchDelta = delta.y * degPerCount;

        body.Rotate(0f, yaw, 0f, Space.World);
        _pitch = Mathf.Clamp(_pitch - pitchDelta, -89f, 89f);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    public void SetSensitivity(float cm360)
    {
        sensCm360 = Mathf.Max(1f, cm360);
    }

    public void SetFov(float verticalFov)
    {
        fov = Mathf.Clamp(verticalFov, 60f, 120f);
        ApplyCameraSettings();
    }

    public void SetSmoothing(bool enabled, int frames)
    {
        smoothingEnabled = enabled;
        smoothingFrames = Mathf.Clamp(frames, 1, 5);
        _smoothedDelta = Vector2.zero;
    }

    public static float CalculateDegreesPerCount(float cm360, int mouseDpi)
    {
        float countsPer360 = Mathf.Max(1f, cm360) * InchesPerCentimeter * Mathf.Max(100, mouseDpi);
        return 360f / countsPer360;
    }

    private void ApplyCameraSettings()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = fov;
    }
}
