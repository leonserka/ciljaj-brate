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

    public event Action<int, int> RouteStarted;
    public event Action<int, float> RouteCleared;
    public event Action AllRoutesCleared;

    private readonly List<Target> _activeBots = new();
    private Transform _playerTransform;
    private CharacterController _playerController;
    private PlayerHealth _playerHealth;
    private InputAction _moveAction;
    private bool _waitingForMove;

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

        if (_playerHealth != null)
            _playerHealth.Died -= OnPlayerDied;
    }

    public void SwitchToRoute(int index)
    {
        if (routes == null || index < 0 || index >= routes.Length) return;
        CurrentRouteIndex = index;
        LoadRoute(index);
    }

    private void OnPlayerDied()
    {
        RouteActive = false;
        LoadRoute(CurrentRouteIndex);
    }

    private void LoadRoute(int index)
    {
        ClearBots();

        var route = routes[index];
        route.HideMarkers();

        _playerController.enabled = false;
        _playerTransform.SetPositionAndRotation(route.PlayerSpawn.position, route.PlayerSpawn.rotation);
        _playerController.enabled = true;

        _playerHealth?.ResetHealth();

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
        RouteCleared?.Invoke(CurrentRouteIndex, RouteTime);

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
