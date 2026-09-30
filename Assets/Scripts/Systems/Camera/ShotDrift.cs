// Implements: 11-camera-performance.md, Part A Step 4.2/5.1 — slow cinematic drift for placeholder cutscene shots (stand-in for Noise profiles).
using UnityEngine;

namespace Ruminahui
{
    /// <summary>Slow cinematic drift for placeholder cutscene shots (stand-in for Cinemachine Noise profiles, Step 5.1).</summary>
    public class ShotDrift : MonoBehaviour
    {
        public float speed = 0.15f;
        Vector3 origin;
        void Awake() => origin = transform.position;
        void Update() => transform.position = origin + transform.right * Mathf.Sin(Time.time * 0.4f) * speed;
    }
}
