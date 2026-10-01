// Implements: 02 M0.1 "move from cover to cover" — inside a hiding spot the player can't be seen (walls, crates, tall grass).
using UnityEngine;

namespace Ruminahui
{
    public class HidingSpot : MonoBehaviour
    {
        public Vector3 size = new Vector3(2f, 2f, 2f);

        public bool Contains(Vector3 p) => new Bounds(transform.position, size).Contains(p);

        void OnEnable() { if (StealthSystem.Instance != null) StealthSystem.Instance.Spots.Add(this); }
        void OnDisable() { if (StealthSystem.Instance != null) StealthSystem.Instance.Spots.Remove(this); }
    }
}
