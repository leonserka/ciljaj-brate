using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private CharacterController controller;

    private InputAction _moveAction;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _moveAction = InputSystem.actions?.FindAction("Move");
        _moveAction?.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.Disable();
    }

    private void Update()
    {
        if (controller == null || !controller.enabled || _moveAction == null) return;
        Vector2 move = _moveAction.ReadValue<Vector2>();
        Vector3 dir = transform.forward * move.y + transform.right * move.x;
        controller.SimpleMove(dir * moveSpeed);
    }
}
