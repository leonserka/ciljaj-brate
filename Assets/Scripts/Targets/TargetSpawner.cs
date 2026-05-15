using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Vector3 spawnBoxSize = new Vector3(5f, 3f, 0f);
    [SerializeField] private float minimumDistanceFromLast = 1.25f;
    [SerializeField] private int maxSpawnAttempts = 20;

    private readonly List<Target> _liveTargets = new List<Target>();
    private Vector3? _lastSpawnPosition;
    private Coroutine _pendingSpawn;

    public IReadOnlyList<Target> LiveTargets => _liveTargets;

    public Target Spawn()
    {
        if (targetPrefab == null)
        {
            Debug.LogWarning("TargetSpawner has no target prefab assigned.", this);
            return null;
        }

        Vector3 position = PickSpawnPosition();
        GameObject targetObject = Instantiate(targetPrefab, position, Quaternion.identity, transform);
        Target target = targetObject.GetComponent<Target>();
        if (target == null)
        {
            Debug.LogError("Spawned target prefab does not contain a Target component.", targetObject);
            Destroy(targetObject);
            return null;
        }

        _liveTargets.Add(target);
        _lastSpawnPosition = position;
        return target;
    }

    public void SpawnAfter(float delay, System.Action<Target> spawned)
    {
        if (_pendingSpawn != null)
            StopCoroutine(_pendingSpawn);

        _pendingSpawn = StartCoroutine(SpawnDelayed(delay, spawned));
    }

    public void DespawnAll()
    {
        if (_pendingSpawn != null)
        {
            StopCoroutine(_pendingSpawn);
            _pendingSpawn = null;
        }

        for (int i = _liveTargets.Count - 1; i >= 0; i--)
        {
            if (_liveTargets[i] != null)
                Destroy(_liveTargets[i].gameObject);
        }

        _liveTargets.Clear();
        _lastSpawnPosition = null;
    }

    public void Forget(Target target)
    {
        _liveTargets.Remove(target);
    }

    private IEnumerator SpawnDelayed(float delay, System.Action<Target> spawned)
    {
        yield return new WaitForSeconds(delay);
        _pendingSpawn = null;
        spawned?.Invoke(Spawn());
    }

    private Vector3 PickSpawnPosition()
    {
        Vector3 bestPosition = transform.position;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 candidate = transform.position + new Vector3(
                WeightedOffset(spawnBoxSize.x),
                WeightedOffset(spawnBoxSize.y),
                Random.Range(-spawnBoxSize.z * 0.5f, spawnBoxSize.z * 0.5f));

            bestPosition = candidate;

            if (!_lastSpawnPosition.HasValue || Vector3.Distance(candidate, _lastSpawnPosition.Value) >= minimumDistanceFromLast)
                return candidate;
        }

        return bestPosition;
    }

    private static float WeightedOffset(float size)
    {
        float centered = ((Random.value + Random.value) * 0.5f) - 0.5f;
        return centered * size;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.913f, 0.118f, 0.549f, 0.35f);
        Gizmos.DrawWireCube(transform.position, spawnBoxSize);
    }
}
