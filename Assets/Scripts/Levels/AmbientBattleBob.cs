// Implements: 08-atoc-boss-fight.md Section 1 "Ambient chaos" — distant, non-interactive background fighters (placeholder motion).
// NOTE: these sit under the Dynamic root (not static-batched) because they move.
using UnityEngine;

namespace Ruminahui
{
    public class AmbientBattleBob : MonoBehaviour
    {
        Vector3 origin;
        float seed;

        void Awake()
        {
            origin = transform.position;
            seed = Random.value * 10f;
        }

        void Update()
        {
            float t = Time.time * 3f + seed;
            transform.position = origin + new Vector3(Mathf.Sin(t) * 0.4f, Mathf.Abs(Mathf.Sin(t * 1.7f)) * 0.3f, 0f);
        }
    }
}
