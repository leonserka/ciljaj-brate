using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrefireManager : MonoBehaviour
{
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private PrefireRoute[] routes;

    public static PrefireManager Instance { get; private set; }

    public int CurrentRouteIndex { get; private set; }
    public int TotalRoutes => routes != null ? routes.Length : 0;
    public PrefireRoute[] Routes => routes;
    public string CurrentRouteName => routes != null && CurrentRouteIndex < routes.Length
        ? routes[CurrentRouteIndex].RouteName : "";
    public bool RouteActive { get; private set; }
    public float RouteTime { get; private set; }
    public int BotsRemaining => _activeBots.Count;
    public int CurrentShotsFired => _currentShotsFired;
    public int CurrentShotsHit => _currentShotsHit;
    public float CurrentAccuracy => _currentShotsFired > 0 ? (float)_currentShotsHit / _currentShotsFired : 0f;
    public IReadOnlyList<PrefireRouteResult> Results => _results;

    public event Action<int, int> RouteStarted;
    public event Action<PrefireRouteResult> RouteCleared;
    public event Action AllRoutesCleared;

    private readonly List<Target> _activeBots = new();
    private readonly List<PrefireRouteResult> _results = new();
    private Transform _playerTransform;
    private CharacterController _playerController;
    private PlayerHealth _playerHealth;
    private InputAction _moveAction;
    private bool _waitingForMove;
    private bool _waitingForContinue;
    private int _currentShotsFired;
    private int _currentShotsHit;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        ClearBots();
        if (_playerHealth != null)
            _playerHealth.Died -= OnPlayerDied;
        if (Instance == this) Instance = null;
    }

    public void Begin()
    {
        var pc = FindAnyObjectByType<PlayerController>();
        if (pc == null) return;

        _playerTransform = pc.transform;
        _playerController = pc.GetComponent<CharacterController>();
        _playerHealth = pc.GetComponent<PlayerHealth>();
        _moveAction = InputSystem.actions?.FindAction("Move");

        if (_playerHealth != null)
            _playerHealth.Died += OnPlayerDied;

        CurrentRouteIndex = 0;
        LoadRoute(CurrentRouteIndex);
    }

    public void Stop()
    {
        ClearBots();
        RouteActive = false;
        _waitingForMove = false;
        _waitingForContinue = false;

        if (_playerHealth != null)
            _playerHealth.Died -= OnPlayerDied;
    }

    public void SwitchToRoute(int index)
    {
        if (routes == null || index < 0 || index >= routes.Length) return;
        _waitingForContinue = false;
        CurrentRouteIndex = index;
        LoadRoute(index);
    }


    public void ContinueAfterClear()
    {
        if (!_waitingForContinue) return;
        _waitingForContinue = false;

        CurrentRouteIndex++;
        if (CurrentRouteIndex < routes.Length)
        {
            LoadRoute(CurrentRouteIndex);
        }
        else
        {
            AllRoutesCleared?.Invoke();
            CurrentRouteIndex = 0;
            LoadRoute(0);
        }
    }


    public void RegisterShot(bool hit)
    {
        if (!RouteActive) return;
        _currentShotsFired++;
        if (hit) _currentShotsHit++;
    }

    private void OnPlayerDied()
    {
        RouteActive = false;
        _waitingForContinue = false;
        LoadRoute(CurrentRouteIndex);
    }

    private void LoadRoute(int index)
    {
        ClearBots();

        var route = routes[index];
        foreach (var r in routes)
            if (r != null) r.HideMarkers();

        var pc = _playerTransform?.GetComponent<PlayerController>();
        if (pc != null)
            pc.Teleport(route.PlayerSpawn.position, route.PlayerSpawn.rotation);
        else if (_playerController != null)
        {
            _playerController.enabled = false;
            _playerTransform.SetPositionAndRotation(route.PlayerSpawn.position, route.PlayerSpawn.rotation);
            _playerController.enabled = true;
        }

        _playerHealth?.ResetHealth();

        var weapon = _playerTransform?.GetComponentInChildren<PrefireWeapon>();
        if (weapon != null) weapon.ResetAmmo();

        _currentShotsFired = 0;
        _currentShotsHit = 0;

        for (int i = 0; i < route.BotCount; i++)
        {
            var bot = Instantiate(botPrefab, route.GetBotPosition(i), route.GetBotRotation(i));
            var target = bot.GetComponent<Target>();
            if (target != null)
            {
                target.Died += OnBotKilled;
                _activeBots.Add(target);
            }
        }

        _waitingForMove = true;
        RouteActive = false;
        RouteTime = 0f;

        RouteStarted?.Invoke(CurrentRouteIndex, TotalRoutes);
    }

    private void Update()
    {
        if (_waitingForMove && _moveAction != null)
        {
            if (_moveAction.ReadValue<Vector2>().sqrMagnitude > 0.01f)
            {
                _waitingForMove = false;
                RouteActive = true;
                RouteTime = 0f;
            }
        }

        if (RouteActive)
            RouteTime += Time.deltaTime;
    }

    private void OnBotKilled(Target target)
    {
        target.Died -= OnBotKilled;
        _activeBots.Remove(target);

        if (_activeBots.Count > 0) return;

        RouteActive = false;

        var result = new PrefireRouteResult
        {
            RouteName = routes[CurrentRouteIndex].RouteName,
            Time = RouteTime,
            ShotsFired = _currentShotsFired,
            ShotsHit = _currentShotsHit,
        };


        while (_results.Count <= CurrentRouteIndex) _results.Add(null);
        if (_results[CurrentRouteIndex] == null || result.Score > _results[CurrentRouteIndex].Score)
            _results[CurrentRouteIndex] = result;

        RouteCleared?.Invoke(result);
        _waitingForContinue = true;
    }

    private void ClearBots()
    {
        foreach (var target in _activeBots)
        {
            if (target != null)
            {
                target.Died -= OnBotKilled;
                Destroy(target.gameObject);
            }
        }
        _activeBots.Clear();
    }
}
