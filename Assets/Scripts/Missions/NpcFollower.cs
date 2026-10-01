// Visual-only companion that keeps near a target (M2.3 Willka, walk-and-talk missions). No combat, no CharacterController.
using UnityEngine;

namespace Ruminahui
{
    public class NpcFollower : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(1.6f, 0f, -1.2f);
        public float speed = 5f;
        public bool following = true;

        void Update()
        {
            if (!following || target == null) return;
            var goal = target.position + target.rotation * offset;
            var to = goal - transform.position;
            to.y = 0f;
            if (to.magnitude < 0.3f) return;
            var step = to.normalized * Mathf.Min(to.magnitude, speed * Time.deltaTime);
            transform.position += step;
            transform.rotation = Quaternion.LookRotation(to.normalized);
        }
    }
}
