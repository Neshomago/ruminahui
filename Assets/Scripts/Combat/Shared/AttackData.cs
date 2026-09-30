// Implements: 03-combat-design.md, Sections 1-3 movesets — one data record per attack so balance lives in data, not code paths.
using UnityEngine;

namespace Ruminahui
{
    [System.Serializable]
    public class AttackData
    {
        public string name = "Attack";
        public float damage = 10f;
        public float range = 2f;
        public float halfAngle = 60f;
        public float windup = 0.15f;
        public float active = 0.08f;
        public float recovery = 0.25f;
        public float staminaCost;
        public float staggerDuration;
        public float knockback;
        public float lunge = 0.6f;
        public HitFlags flags;
        public int guardBreakPower;

        public AttackData() { }

        public AttackData(string name, float damage, float range, float windup, float recovery, HitFlags flags = HitFlags.None,
            float stagger = 0f, float staminaCost = 0f, int guardBreakPower = 0, float halfAngle = 60f, float knockback = 0f, float lunge = 0.6f)
        {
            this.name = name;
            this.damage = damage;
            this.range = range;
            this.windup = windup;
            this.recovery = recovery;
            this.flags = flags;
            staggerDuration = stagger;
            this.staminaCost = staminaCost;
            this.guardBreakPower = guardBreakPower;
            this.halfAngle = halfAngle;
            this.knockback = knockback;
            this.lunge = lunge;
        }

        public AttackData Clone() => (AttackData)MemberwiseClone();

        public float TotalDuration => windup + active + recovery;
    }
}
