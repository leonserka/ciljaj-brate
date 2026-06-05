using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    private const float YawPerCount = 0.022f;

    [SerializeField] private Transform body;
    [SerializeField] private Camera playerCamera;
    [SerializeField, Range(0.1f, 10f)] private float sensitivity = 1f;
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
        sensitivity = GameplaySettings.Sensitivity;
        fov = GameplaySettings.Fov;
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
        sensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
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

        float degPerCount = YawPerCount * sensitivity;
        float yaw = delta.x * degPerCount;
        float pitchDelta = delta.y * degPerCount;

        body.Rotate(0f, yaw, 0f, Space.World);
        _pitch = Mathf.Clamp(_pitch - pitchDelta, -89f, 89f);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    public void SetSensitivity(float sens)
    {
        sensitivity = Mathf.Clamp(sens, 0.1f, 10f);
    }

    public void SetFov(float verticalFov)
    {
        fov = Mathf.Clamp(verticalFov, 60f, 120f);
        ApplyCameraSettings();
    }

    public void ApplyRecoilPunch(float pitchDelta, float yawDelta)
    {
        _pitch = Mathf.Clamp(_pitch + pitchDelta, -89f, 89f);
        body.Rotate(0f, yawDelta, 0f, Space.World);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void ApplyCameraSettings()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = fov;
    }
}
