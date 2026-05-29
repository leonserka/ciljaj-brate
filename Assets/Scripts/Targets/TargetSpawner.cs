using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Vector3 spawnBoxSize = new Vector3(5f, 3f, 0f);
    [SerializeField] private float minimumDistanceFromLast = 1.25f;
    [SerializeField] private int maxSpawnAttempts = 100;

    private readonly List<Target> _liveTargets = new List<Target>();
    private Coroutine _pendingSpawn;

    private bool _useGrid;
    private int _gridCols = 4;
    private int _gridRows = 4;
    private Vector3[] _gridSlots;
    private readonly HashSet<int> _occupiedSlots = new HashSet<int>();
    private int _lastKilledSlot = -1;

    public IReadOnlyList<Target> LiveTargets => _liveTargets;

    public void SetSpawnBox(Vector3 size, float minDist = -1f)
    {
        spawnBoxSize = size;
        if (minDist >= 0) minimumDistanceFromLast = minDist;
        _useGrid = false;
    }

    public void SetupGrid(int cols, int rows, Vector3 areaSize)
    {
        _gridCols = cols;
        _gridRows = rows;
        spawnBoxSize = areaSize;
        _useGrid = true;

        _gridSlots = new Vector3[cols * rows];
        _occupiedSlots.Clear();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = Mathf.Lerp(-areaSize.x * 0.5f, areaSize.x * 0.5f, (c + 0.5f) / cols);
                float y = Mathf.Lerp(-areaSize.y * 0.5f, areaSize.y * 0.5f, (r + 0.5f) / rows);
                _gridSlots[r * cols + c] = transform.position + new Vector3(x, y, 0f);
            }
        }
    }

    public Target Spawn()
    {
        if (targetPrefab == null)
        {
            Debug.LogWarning("TargetSpawner has no target prefab assigned.", this);
            return null;
        }

        Vector3 position = _useGrid ? PickGridPosition() : PickRandomPosition();
        GameObject targetObject = Instantiate(targetPrefab, position, Quaternion.identity, transform);
        Target target = targetObject.GetComponent<Target>();
        if (target == null)
        {
            Debug.LogError("Spawned target prefab does not contain a Target component.", targetObject);
            Destroy(targetObject);
            return null;
        }

        _liveTargets.Add(target);
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
        _occupiedSlots.Clear();
    }

    public void Forget(Target target)
    {
        _liveTargets.Remove(target);

        if (_useGrid && _gridSlots != null)
        {
            for (int i = 0; i < _gridSlots.Length; i++)
            {
                if (_occupiedSlots.Contains(i) && Vector3.Distance(_gridSlots[i], target.transform.position) < 0.1f)
                {
                    _occupiedSlots.Remove(i);
                    _lastKilledSlot = i;
                    break;
                }
            }
        }
    }

    private IEnumerator SpawnDelayed(float delay, System.Action<Target> spawned)
    {
        yield return new WaitForSeconds(delay);
        _pendingSpawn = null;
        spawned?.Invoke(Spawn());
    }

    private Vector3 PickGridPosition()
    {
        _liveTargets.RemoveAll(t => t == null);

        var freeSlots = new List<int>();
        for (int i = 0; i < _gridSlots.Length; i++)
        {
            if (!_occupiedSlots.Contains(i) && i != _lastKilledSlot)
                freeSlots.Add(i);
        }

        if (freeSlots.Count == 0)
        {
            _occupiedSlots.Clear();
            for (int i = 0; i < _gridSlots.Length; i++)
                freeSlots.Add(i);
        }

        int picked = freeSlots[Random.Range(0, freeSlots.Count)];
        _occupiedSlots.Add(picked);
        _lastKilledSlot = -1;
        return _gridSlots[picked];
    }

    private Vector3 PickRandomPosition()
    {
        float targetRadius = targetPrefab != null ? targetPrefab.transform.localScale.x * 0.5f : 0.275f;
        float noOverlapDist = targetRadius * 2f + 0.15f;

        _liveTargets.RemoveAll(t => t == null);

        Vector3 bestPosition = transform.position;
        float bestMinDist = -1f;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 candidate = transform.position + new Vector3(
                Random.Range(-spawnBoxSize.x * 0.5f, spawnBoxSize.x * 0.5f),
                Random.Range(-spawnBoxSize.y * 0.5f, spawnBoxSize.y * 0.5f),
                Random.Range(-spawnBoxSize.z * 0.5f, spawnBoxSize.z * 0.5f));

            float closestDist = float.MaxValue;
            bool tooClose = false;

            foreach (var t in _liveTargets)
            {
                float d = Vector3.Distance(candidate, t.transform.position);
                if (d < noOverlapDist) { tooClose = true; break; }
                if (d < closestDist) closestDist = d;
            }

            if (!tooClose)
                return candidate;

            if (closestDist > bestMinDist)
            {
                bestMinDist = closestDist;
                bestPosition = candidate;
            }
        }

        return bestPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.913f, 0.118f, 0.549f, 0.35f);
        Gizmos.DrawWireCube(transform.position, spawnBoxSize);

        if (_gridSlots != null)
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.4f);
            foreach (var slot in _gridSlots)
                Gizmos.DrawWireSphere(slot, 0.25f);
        }
    }
}
