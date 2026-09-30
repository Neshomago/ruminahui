// Implements: 03-combat-design.md Section 0 (one shared Focus meter) + Section 4 (who is player-controlled vs AI ally);
// shared by the forced swap (05, M3.5) and free swap (06, M5.3) systems.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class PartyManager : Singleton<PartyManager>
    {
        public readonly FocusPool Focus = new FocusPool();
        public readonly List<PlayerCharacter> Members = new List<PlayerCharacter>();

        public PlayerCharacter Controlled { get; private set; }
        public bool GodMode { get; private set; }

        public event System.Action<PlayerCharacter> ControlledChanged;
        public event System.Action<PlayerCharacter> ControlledDied;

        public void Register(PlayerCharacter c)
        {
            if (Members.Contains(c)) return;
            Members.Add(c);
            c.Health.Died += () => OnMemberDied(c);
            c.Target.invulnerable = GodMode;
        }

        public void Unregister(PlayerCharacter c)
        {
            Members.Remove(c);
            if (Controlled == c) Controlled = null;
        }

        public PlayerCharacter Get(CharacterId id)
        {
            foreach (var m in Members) if (m != null && m.id == id) return m;
            return null;
        }

        /// <summary>Moves player control to <paramref name="c"/>. cut=true: hard camera cut (M3.5 masks it under a flash).</summary>
        public void SetControlled(PlayerCharacter c, bool cut)
        {
            if (c == null) return;
            if (Controlled == c) { CameraDirector.Instance?.FocusCharacter(c, cut); return; }
            var prev = Controlled;
            if (prev != null) prev.ApplyControlState(false);
            Controlled = c;
            c.ApplyControlState(true);
            if (CameraDirector.Instance != null) CameraDirector.Instance.FocusCharacter(c, cut);
            AudioPool.Instance?.PlayCue(PlaceholderCue.Swap, c.transform.position, 0.25f);
            ControlledChanged?.Invoke(c);
        }

        void OnMemberDied(PlayerCharacter c)
        {
            if (c == Controlled) ControlledDied?.Invoke(c);
        }

        public void SetGodMode(bool on)
        {
            GodMode = on;
            foreach (var m in Members) if (m != null) m.Target.invulnerable = on;
        }

        public void RefillAll()
        {
            Focus.Fill();
            foreach (var m in Members)
            {
                if (m == null) continue;
                m.Health.ResetFull();
                m.Stamina.ResetFull();
            }
        }

        /// <summary>Scene change: members are scene objects, they unregister themselves; this clears stale refs.</summary>
        public void ClearMembers()
        {
            Members.RemoveAll(m => m == null);
            if (Controlled == null) Controlled = null;
        }
    }
}
