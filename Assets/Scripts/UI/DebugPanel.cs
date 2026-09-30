// Implements: approved UI plan item 10 (F1) — unlock all, refill, god mode, spawn enemy, jump to mission, tell-flash toggle,
// upgrade tiers per kit, and a live feed of Focus gains (03 Section 0 sources).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class DebugPanel : UIElement
    {
        RectTransform panel;
        Text info;
        Text focusFeed;
        readonly Queue<string> focusLines = new Queue<string>();
        bool open;
        FocusPool subscribed;

        protected override void OnBuild()
        {
            panel = UIFactory.Panel(Root, "Panel", new Color(0.05f, 0.05f, 0.08f, 0.92f)).rectTransform;
            panel.GetComponent<Image>().raycastTarget = true;
            UIFactory.Place(panel, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(420f, 900f));

            var list = UIFactory.Node(panel, "List");
            UIFactory.Stretch(list);
            UIFactory.VerticalList(list, 6f, 12);

            Header(list, "DEBUG (F1)");
            Btn(list, "Unlock all abilities: toggle", () => Progression.SetUnlockAll(!Progression.UnlockAll));
            Btn(list, "Refill HP / STA / Focus", () => PartyManager.Instance?.RefillAll());
            Btn(list, "God mode: toggle", () => PartyManager.Instance?.SetGodMode(!PartyManager.Instance.GodMode));
            Btn(list, "Tell flashes: toggle", () => TellFlashSettings.Enabled = !TellFlashSettings.Enabled);
            Btn(list, "Damage numbers: toggle", () => DamageNumbers.Enabled = !DamageNumbers.Enabled);

            Header(list, "Upgrade tiers (0-3)");
            foreach (KitId k in System.Enum.GetValues(typeof(KitId)))
            {
                var kit = k;
                Btn(list, $"{kit}: tier +1 (wraps)", () => Progression.SetTier(kit, (Progression.GetTier(kit) + 1) % 4));
            }

            Header(list, "Spawn in front of you");
            var row = UIFactory.Node(list, "SpawnGrid");
            row.sizeDelta = new Vector2(396f, 170f);
            var grid = row.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(128f, 30f);
            grid.spacing = new Vector2(6f, 6f);
            foreach (EnemyType t in System.Enum.GetValues(typeof(EnemyType)))
            {
                var type = t;
                UIFactory.Button(row, type.ToString(), () => SpawnInFront(type), new Vector2(128f, 30f));
            }

            Btn(list, "Clear all enemies", () => PoolManager.Instance?.DespawnAll());
            Btn(list, "Missions… (opens pause menu)", () => { Toggle(); PauseMenu.Instance?.Open(true); });

            info = UIFactory.Label(list, "Info", "", 13, TextAnchor.UpperLeft, new Color(0.8f, 0.9f, 1f));
            info.rectTransform.sizeDelta = new Vector2(396f, 90f);
            Header(list, "Focus gains");
            focusFeed = UIFactory.Label(list, "Feed", "", 13, TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.4f));
            focusFeed.rectTransform.sizeDelta = new Vector2(396f, 110f);

            panel.gameObject.SetActive(false);
        }

        void Header(RectTransform parent, string text)
        {
            var t = UIFactory.Label(parent, "Header", text, 16, TextAnchor.MiddleLeft, new Color(1f, 0.8f, 0.4f));
            t.fontStyle = FontStyle.Bold;
            t.rectTransform.sizeDelta = new Vector2(396f, 24f);
        }

        void Btn(RectTransform parent, string label, System.Action a) => UIFactory.Button(parent, label, a, new Vector2(396f, 30f));

        void SpawnInFront(EnemyType type)
        {
            var c = Controlled;
            if (c == null) return;
            var pos = c.transform.position + c.transform.forward * 6f + Vector3.up * 0.2f;
            EnemyFactory.Spawn(type, pos, Quaternion.LookRotation(-c.transform.forward));
        }

        void Toggle()
        {
            open = !open;
            panel.gameObject.SetActive(open);
            if (open) GameInput.Instance?.PushMenu();
            else GameInput.Instance?.PopMenu();
        }

        void Update()
        {
            var input = GameInput.Instance;
            if (input != null && input.DebugToggle.WasPressedThisFrame()) Toggle();

            var focus = PartyManager.Instance != null ? PartyManager.Instance.Focus : null;
            if (focus != null && subscribed != focus)
            {
                focus.Gained += OnFocusGained;
                subscribed = focus;
            }

            if (!open) return;
            info.text = $"Unlock all: {Progression.UnlockAll}   God: {(PartyManager.Instance != null && PartyManager.Instance.GodMode)}\n" +
                        $"Tell flash: {TellFlashSettings.Enabled}   Tiers P/K/A: {Progression.GetTier(KitId.Puma)}/{Progression.GetTier(KitId.Kuntur)}/{Progression.GetTier(KitId.Amaru)}\n" +
                        $"Tell reading (post-M3.5): {Progression.IsUnlocked(Unlocks.TellReading)}   Enemies: {EnemyBrain.Active.Count}\n" +
                        $"Objective: {ObjectiveTracker.Current}";
            focusFeed.text = string.Join("\n", focusLines.ToArray());
        }

        void OnFocusGained(string reason, float amount)
        {
            focusLines.Enqueue($"+{amount:0.#}  {reason}");
            while (focusLines.Count > 6) focusLines.Dequeue();
        }
    }
}
