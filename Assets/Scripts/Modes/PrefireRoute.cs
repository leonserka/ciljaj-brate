using System.Collections.Generic;
using UnityEngine;

public class PrefireRoute : MonoBehaviour
{
    [SerializeField] private string routeName;
    [SerializeField] private Sprite thumbnail;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform[] botSpawns;

    private Transform[] _resolvedBots;

    public string RouteName => string.IsNullOrEmpty(routeName) ? gameObject.name : routeName;
    public Sprite Thumbnail => thumbnail;
    public Transform PlayerSpawn => playerSpawn;
    public int BotCount => ResolvedBots.Length;

    public Vector3 GetBotPosition(int index) => ResolvedBots[index].position;
    public Quaternion GetBotRotation(int index) => ResolvedBots[index].rotation;

    private Transform[] ResolvedBots
    {
        get
        {
            if (_resolvedBots != null) return _resolvedBots;
            _resolvedBots = FindBotSpawns();
            return _resolvedBots;
        }
    }

    private Transform[] FindBotSpawns()
    {
        if (botSpawns != null && botSpawns.Length > 0)
            return botSpawns;

        var bots = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child == playerSpawn) continue;
            if (child.name.StartsWith("Bot", System.StringComparison.OrdinalIgnoreCase))
                bots.Add(child);
        }
        bots.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
        return bots.ToArray();
    }

    public void HideMarkers()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    public void ShowMarkers()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
    }
}
