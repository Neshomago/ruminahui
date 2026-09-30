// Implements: 06-m5-3-flow-and-enemy-roster.md, M5.3 Obstacle 2 "The Wide Break" — Chaska crosses first, then drops a line for the others.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class LineAnchorObstacle : SwapPuzzleInteractable
    {
        public GameObject bridge;

        void Reset() { requiredCharacter = CharacterId.Chaska; actionText = "Drop a line for the others"; }

        protected override void OnSolved(PlayerCharacter c)
        {
            if (bridge != null) bridge.SetActive(true);
            ObjectiveTracker.Say("Chaska", "(secures the line across the ravine)");
        }
    }
}
