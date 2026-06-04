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

    public Vector3 GetBotPosition(int index) => MarkerOf(ResolvedBots[index]).position;
    public Quaternion GetBotRotation(int index) => MarkerOf(ResolvedBots[index]).rotation;

    // The visible preview model ("Placeholder") is what the level designer drags
    // onto the map; the "Bot" parent transform may sit elsewhere. Spawn the real
    // bot where the marker actually is, not at the parent's position.
    private static Transform MarkerOf(Transform bot)
    {
        var marker = bot.Find("Placeholder");
        return marker != null ? marker : bot;
    }

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
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = false;
        // Markers are design-time preview bots with their own Target + Collider.
        // Their colliders must be disabled during play, otherwise the player's
        // raycast can hit an invisible marker instead of the spawned bot,
        // playing hit/kill sounds while no visible bot takes damage.
        foreach (var c in GetComponentsInChildren<Collider>(true))
            c.enabled = false;
    }

    public void ShowMarkers()
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.enabled = true;
        foreach (var c in GetComponentsInChildren<Collider>(true))
            c.enabled = true;
    }
}
