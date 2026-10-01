// Implements: 04 M0.1 "light interactions with 2–3 NPCs, establishing the community" — talkable NPC. The doc doesn't write these
// lines, so content is clearly-marked placeholder until the writers add them to 04 (then they come through the importer).
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public class TalkNpc : MonoBehaviour, IInteractable
    {
        public string npcName = "Villager";
        public List<string> lines = new List<string>();
        public float range = 2.4f;
        public bool Talked { get; private set; }
        public event System.Action<TalkNpc> TalkedTo;

        public Vector3 Position => transform.position;
        public float Range => range;
        public PromptStyle Style => PromptStyle.Normal;
        public float HoldDuration => 0f;
        public bool OverridesKitContext => true;

        void OnEnable() => InteractableRegistry.All.Add(this);
        void OnDisable() => InteractableRegistry.All.Remove(this);

        public string PromptFor(PlayerCharacter c) => DialogueRunner.Shared != null && DialogueRunner.Shared.IsPlaying ? null : $"Talk to {npcName}";
        public bool CanInteract(PlayerCharacter c) => DialogueRunner.Shared != null && !DialogueRunner.Shared.IsPlaying;

        public void Interact(PlayerCharacter c)
        {
            var runner = DialogueRunner.Shared;
            if (runner == null) return;
            var beats = new List<DialogueBeat>();
            for (int i = 0; i < lines.Count; i++)
                beats.Add(new DialogueBeat { id = $"npc.{npcName}.{i}", kind = "line", speaker = npcName, direction = "", text = lines[i], prompts = new string[0] });
            runner.StartCoroutine(runner.PlayScene(new DialogueScene { heading = "", beats = beats.ToArray() }));
            if (!Talked) { Talked = true; TalkedTo?.Invoke(this); }
        }
    }
}
