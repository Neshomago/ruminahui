// Implements: 06 Enemy Roster, Spanish Infantry — "tighter formation fighting": keeps infantry shoulder-to-shoulder facing their target.
using UnityEngine;

namespace Ruminahui
{
    /// <summary>Keeps infantry shoulder-to-shoulder in a line facing their target ("tighter formation fighting").</summary>
    public class FormationGroup : MonoBehaviour
    {
        public float spacing = 1.4f;
        public float frontDistance = 1.9f;
        readonly System.Collections.Generic.List<SpanishInfantry> members = new System.Collections.Generic.List<SpanishInfantry>();

        public void Add(SpanishInfantry s)
        {
            if (!members.Contains(s)) members.Add(s);
            s.formation = this;
        }

        public Vector3 SlotFor(SpanishInfantry s, Vector3 targetPos)
        {
            members.RemoveAll(m => m == null || m.IsDead || !m.isActiveAndEnabled);
            int i = members.IndexOf(s);
            if (i < 0) return targetPos;
            var center = Vector3.zero;
            foreach (var m in members) center += m.transform.position;
            center /= members.Count;
            var toTarget = targetPos - center;
            toTarget.y = 0f;
            var fwd = toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : Vector3.forward;
            var right = Vector3.Cross(Vector3.up, fwd);
            float offset = (i - (members.Count - 1) * 0.5f) * spacing;
            return targetPos - fwd * frontDistance + right * offset;
        }
    }
}
