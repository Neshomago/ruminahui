// Implements: 06 M5.3 flow, Section 3 — free character switching "available at all times except during stage F, where the game
// auto-locks the player to whichever character is mid-action". Shoulder buttons cycle; 1/2/3 pick directly.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class FreeSwapController : Singleton<FreeSwapController>
    {
        public bool Active { get; private set; }
        public bool IsLocked => lockOwners.Count > 0;
        public float swapCooldown = 0.35f;

        readonly HashSet<object> lockOwners = new HashSet<object>();
        static readonly CharacterId[] Order = { CharacterId.Ruminahui, CharacterId.Chaska, CharacterId.Atoc };
        float nextSwapAt;

        public void SetActive(bool on)
        {
            Active = on;
            if (!on) lockOwners.Clear();
        }

        /// <summary>Any system can lock swapping while something synchronized is happening (stage F).</summary>
        public void Lock(object owner) => lockOwners.Add(owner);
        public void Unlock(object owner) => lockOwners.Remove(owner);

        void Update()
        {
            if (!Active || IsLocked) return;
            var input = GameInput.Instance;
            var pm = PartyManager.Instance;
            if (input == null || pm == null || pm.Controlled == null) return;
            if (Time.time < nextSwapAt) return;

            if (input.Pressed(input.Swap1)) TrySwapTo(CharacterId.Ruminahui);
            else if (input.Pressed(input.Swap2)) TrySwapTo(CharacterId.Chaska);
            else if (input.Pressed(input.Swap3)) TrySwapTo(CharacterId.Atoc);
            else if (input.Pressed(input.SwapNext)) Cycle(1);
            else if (input.Pressed(input.SwapPrev)) Cycle(-1);
        }

        void Cycle(int dir)
        {
            var pm = PartyManager.Instance;
            int i = System.Array.IndexOf(Order, pm.Controlled.id);
            for (int step = 1; step <= Order.Length; step++)
            {
                var id = Order[((i + dir * step) % Order.Length + Order.Length) % Order.Length];
                if (TrySwapTo(id)) return;
            }
        }

        public bool TrySwapTo(CharacterId id)
        {
            if (!Active || IsLocked) return false;
            var pm = PartyManager.Instance;
            var c = pm.Get(id);
            if (c == null || c == pm.Controlled || c.ScriptLocked) return false;
            var prev = pm.Controlled;
            pm.SetControlled(c, false);
            if (prev != null && prev.allyMode == AllyMode.Combat) prev.allyMode = AllyMode.Follow;
            nextSwapAt = Time.time + swapCooldown;
            return true;
        }
    }
}
