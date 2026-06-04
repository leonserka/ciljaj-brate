using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float crouchHeight = 0.55f;
    [SerializeField] private float crouchSpeed = 10f;
    [SerializeField] private CharacterController controller;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip jumpLaunchClip;
    [SerializeField] private AudioClip jumpLandClip;
    [SerializeField] private AudioClip[] jumpLandClips;
    [SerializeField] private float footstepInterval = 0.35f;
    [SerializeField] private float footstepVolume = 0.2f;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _crouchAction;
    private float _verticalVelocity;
    private float _standHeight;
    private Vector3 _standCenter;
    private float _standCamY;
    private Transform _cameraTransform;
    private AudioSource _audioSource;
    private float _footstepTimer;
    private bool _wasGrounded;
    private Vector3 _airVelocity;

    public float CameraHeight => _cameraTransform != null ? _cameraTransform.localPosition.y : _standCamY;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        _standHeight = controller.height;
        _standCenter = controller.center;
        var cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            _cameraTransform = cam.transform;
            _standCamY = _cameraTransform.localPosition.y;
        }
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        _moveAction = InputSystem.actions?.FindAction("Move");
        _moveAction?.Enable();
        _jumpAction = InputSystem.actions?.FindAction("Jump");
        _jumpAction?.Enable();
        _crouchAction = InputSystem.actions?.FindAction("Crouch");
        _crouchAction?.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.Disable();
        _jumpAction?.Disable();
        _crouchAction?.Disable();
    }

    private void Update()
    {
        if (controller == null || !controller.enabled || _moveAction == null) return;

        bool grounded = controller.isGrounded;
        if (grounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        bool crouching = _crouchAction != null && _crouchAction.IsPressed();
        float targetHeight = crouching ? crouchHeight : _standHeight;
        float targetCamY = crouching ? _standCamY * (crouchHeight / _standHeight) : _standCamY;

        controller.height = Mathf.MoveTowards(controller.height, targetHeight, crouchSpeed * Time.deltaTime);
        controller.center = new Vector3(_standCenter.x, controller.height * 0.5f, _standCenter.z);

        if (_cameraTransform != null)
        {
            Vector3 camPos = _cameraTransform.localPosition;
            camPos.y = Mathf.MoveTowards(camPos.y, targetCamY, crouchSpeed * Time.deltaTime);
            _cameraTransform.localPosition = camPos;
        }

        Vector2 move = _moveAction.ReadValue<Vector2>();
        Vector3 dir = transform.forward * move.y + transform.right * move.x;
        float speed = crouching ? moveSpeed * 0.45f : moveSpeed;

        bool wantsJump = _jumpAction != null && _jumpAction.WasPressedThisFrame();

        if (grounded && wantsJump && !crouching)
        {
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _airVelocity = dir * speed;
            if (jumpLaunchClip != null)
                _audioSource.PlayOneShot(jumpLaunchClip, footstepVolume);
        }

        if (grounded && !_wasGrounded)
        {
            if (jumpLandClips != null && jumpLandClips.Length > 0)
                _audioSource.PlayOneShot(jumpLandClips[Random.Range(0, jumpLandClips.Length)], footstepVolume);
            else if (jumpLandClip != null)
                _audioSource.PlayOneShot(jumpLandClip, footstepVolume);
        }
        _wasGrounded = grounded;

        _verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontal;
        if (grounded)
        {
            horizontal = dir * speed;
        }
        else
        {
            horizontal = _airVelocity;
            if (move.sqrMagnitude > 0.01f)
                horizontal += dir * speed * 0.3f;
        }

        Vector3 movement = horizontal * Time.deltaTime;
        movement.y = _verticalVelocity * Time.deltaTime;
        controller.Move(movement);

        if (grounded && !crouching && move.sqrMagnitude > 0.01f && footstepClips != null && footstepClips.Length > 0)
        {
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                _footstepTimer = footstepInterval;
                _audioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)], footstepVolume);
            }
        }
        else
        {
            _footstepTimer = 0f;
        }
    }
}
