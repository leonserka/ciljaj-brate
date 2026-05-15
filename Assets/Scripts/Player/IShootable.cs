using UnityEngine;

public interface IShootable
{
    void OnShot(RaycastHit hit, Vector3 fromDirection);
}
