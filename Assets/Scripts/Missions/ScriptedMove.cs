// Scripted placement/movement for mission beats. Anything with a CharacterController moves via CharacterMotor.Teleport — setting
// transform.position directly gets overwritten by the controller (LESSONS_LEARNED rule #5).
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public static class ScriptedMove
    {
        public static void Place(Transform t, Vector3 position, Quaternion? rotation = null)
        {
            if (t == null) return;
            var motor = t.GetComponent<CharacterMotor>();
            if (motor != null) motor.Teleport(position, rotation);
            else
            {
                t.position = position;
                if (rotation.HasValue) t.rotation = rotation.Value;
            }
        }

        /// <summary>Linear move over <paramref name="seconds"/>, facing the direction of travel.</summary>
        public static IEnumerator To(Transform t, Vector3 target, float seconds)
        {
            if (t == null) yield break;
            var start = t.position;
            var dir = target - start;
            dir.y = 0f;
            var rot = dir.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir) : t.rotation;
            float e = 0f;
            while (e < seconds && t != null)
            {
                e += Time.deltaTime;
                Place(t, Vector3.Lerp(start, target, Mathf.Clamp01(e / seconds)), rot);
                yield return null;
            }
        }
    }
}
