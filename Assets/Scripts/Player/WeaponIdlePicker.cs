using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WeaponIdlePicker : MonoBehaviour
{
    [SerializeField] private float minIdleDelay = 5f;
    [SerializeField] private float maxIdleDelay = 12f;

    private Animator _anim;
    private float _timer;
    private static readonly string[] _triggers = { "IdleVar2", "IdleVar3", "IdleVar4" };

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        PickNextTimer();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _anim.SetTrigger(_triggers[Random.Range(0, _triggers.Length)]);
            PickNextTimer();
        }
    }

    private void PickNextTimer() => _timer = Random.Range(minIdleDelay, maxIdleDelay);
}
