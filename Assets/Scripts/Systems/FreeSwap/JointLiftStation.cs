// Implements: 06-m5-3-flow-and-enemy-roster.md, M5.3 Obstacle 4 "The Joint Lift" — one station per character; all three required.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class JointLiftStation : SwapPuzzleInteractable
    {
        public string roleText = "Brace";
        public System.Func<bool> availability;
        public Transform holdPoint;

        public override bool IsAvailable => availability == null || availability();

        protected override void OnSolved(PlayerCharacter c)
        {
            c.allyMode = AllyMode.Hold;
            if (c.AllyBrain != null) c.AllyBrain.HoldAt(holdPoint != null ? holdPoint.position : c.transform.position);
            ObjectiveTracker.Say(c.displayName, $"({roleText} — holding position)");
        }
    }
}
