using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TrackingTarget : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float boundsX = 4f;
    [SerializeField] private float boundsY = 1.5f;
    [SerializeField] private float directionChangeInterval = 1.5f;

    public bool IsOnTarget { get; private set; }

    private Vector3 _origin;
    private Vector2 _velocity;
    private float _changeTimer;
    private Camera _cam;

    public void Init(float moveSpeed, float horizontal, float vertical, float changeInterval = 1.5f)
    {
        speed = moveSpeed;
        boundsX = horizontal;
        boundsY = vertical;
        directionChangeInterval = changeInterval;
    }

    private void Start()
    {
        _origin = transform.position;
        _cam = Camera.main;
        PickDirection();
    }

    private void Update()
    {
        Move();
        CheckCrosshair();
    }

    private void Move()
    {
        _changeTimer -= Time.deltaTime;
        if (_changeTimer <= 0f)
            PickDirection();

        Vector3 pos = transform.position;
        pos.x += _velocity.x * Time.deltaTime;
        pos.y += _velocity.y * Time.deltaTime;

        float dx = pos.x - _origin.x;
        float dy = pos.y - _origin.y;

        if (Mathf.Abs(dx) > boundsX)
        {
            dx = Mathf.Clamp(dx, -boundsX, boundsX);
            _velocity.x = -_velocity.x;
        }
        if (Mathf.Abs(dy) > boundsY)
        {
            dy = Mathf.Clamp(dy, -boundsY, boundsY);
            _velocity.y = -_velocity.y;
        }

        pos.x = _origin.x + dx;
        pos.y = _origin.y + dy;
        transform.position = pos;
    }

    private void PickDirection()
    {
        _changeTimer = directionChangeInterval + Random.Range(-0.4f, 0.4f);
        float angle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        _velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.4f) * speed;
        if (Random.value > 0.5f) _velocity.x = -_velocity.x;
    }

    private void CheckCrosshair()
    {
        if (_cam == null) return;
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        IsOnTarget = Physics.Raycast(ray, out RaycastHit hit, 200f) && hit.transform == transform;
    }
}
