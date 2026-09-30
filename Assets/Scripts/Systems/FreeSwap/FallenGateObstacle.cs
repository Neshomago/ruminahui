// Implements: 06-m5-3-flow-and-enemy-roster.md, M5.3 Obstacle 1 "The Fallen Gate" — Rumiñahui (Puma strength check) heaves the stone aside.
// then drops a line), Obstacle 3 The Narrow Dark (rigged deadfalls — Amaru trap-sense reads/disarms them), Obstacle 4 The Joint
// Lift (all three stations), plus the no-fail "recoverable stumble" volume and optional collectibles.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class FallenGateObstacle : SwapPuzzleInteractable
    {
        public Transform stone;

        void Reset() { requiredCharacter = CharacterId.Ruminahui; holdDuration = 1.5f; actionText = "Heave the stone"; }

        protected override void OnSolved(PlayerCharacter c)
        {
            AudioPool.Instance?.PlayCue(PlaceholderCue.Heave, transform.position, 0.7f);
            ObjectiveTracker.Say("Rumiñahui", "(heaves the stone aside)");
            if (stone != null) StartCoroutine(Slide());
        }

        IEnumerator Slide()
        {
            var start = stone.position;
            var end = start + stone.right * 3.5f;
            float t = 0f;
            while (t < 1.2f) { t += Time.deltaTime; stone.position = Vector3.Lerp(start, end, t / 1.2f); yield return null; }
            Physics.SyncTransforms();
        }
    }
}
