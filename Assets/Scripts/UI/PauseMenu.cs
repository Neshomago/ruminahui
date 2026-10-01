// Implements: approved UI plan item 11 (Esc) — resume, restart from checkpoint, mission list (all 26; unbuilt ones load
// placeholders) and test scenes.
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class PauseMenu : UIElement
    {
        public static PauseMenu Instance { get; private set; }
        RectTransform panel;
        RectTransform missions;
        bool open;

        protected override void OnBuild()
        {
            Instance = this;
            panel = UIFactory.Panel(Root, "Panel", new Color(0f, 0f, 0f, 0.8f)).rectTransform;
            panel.GetComponent<Image>().raycastTarget = true;
            UIFactory.Stretch(panel);

            var col = UIFactory.Node(panel, "Main");
            UIFactory.Place(col, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(320f, 520f));
            UIFactory.VerticalList(col, 10f, 10);
            var title = UIFactory.Label(col, "Title", "PAUSED", 34, TextAnchor.MiddleCenter, Color.white);
            title.rectTransform.sizeDelta = new Vector2(300f, 50f);
            UIFactory.Button(col, "Resume", () => Open(false), new Vector2(300f, 44f));
            UIFactory.Button(col, "Upgrades", () => { Open(false); UpgradeMenuUI.Instance?.Open(true); }, new Vector2(300f, 44f));
            UIFactory.Button(col, "Settings", () => SettingsPanel.Instance?.Open(true), new Vector2(300f, 44f));
            UIFactory.Button(col, "Restart from checkpoint", () => { Open(false); CheckpointService.Instance?.RestartFromCheckpoint(); }, new Vector2(300f, 44f));
            UIFactory.Button(col, "Restart scene", () => { Open(false); MissionManager.Instance?.RestartCurrent(); }, new Vector2(300f, 44f));
            UIFactory.Button(col, "Main menu", () => { Open(false); MissionManager.Instance?.StartMission(MissionDatabase.BootScene); }, new Vector2(300f, 44f));

            missions = UIFactory.Node(panel, "Missions");
            UIFactory.Place(missions, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(820f, 900f));
            var grid = missions.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(400f, 28f);
            grid.spacing = new Vector2(8f, 4f);
            foreach (var m in MissionDatabase.Missions)
            {
                var id = m.Id;
                string label = $"{m.Id}  {m.Title}" + (m.HasBuiltContent ? "" : "  (placeholder)");
                UIFactory.Button(missions, label, () => { Open(false); MissionManager.Instance?.StartFromChapterSelect(id); }, new Vector2(400f, 28f));
            }
            foreach (var t in MissionDatabase.TestScenes)
            {
                var id = t;
                UIFactory.Button(missions, "TEST: " + t, () => { Open(false); MissionManager.Instance?.StartMission(id); }, new Vector2(400f, 28f));
            }

            panel.gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(bool value)
        {
            if (open == value) return;
            open = value;
            panel.gameObject.SetActive(open);
            Time.timeScale = open ? 0f : 1f;
            if (open) GameInput.Instance?.PushMenu();
            else GameInput.Instance?.PopMenu();
        }

        void Update()
        {
            var input = GameInput.Instance;
            // Menus stacked on top (upgrades, settings) and the main menu own Esc while open.
            if ((UpgradeMenuUI.Instance != null && UpgradeMenuUI.Instance.IsOpen) || (SettingsPanel.Instance != null && SettingsPanel.Instance.IsOpen)) return;
            if (MainMenuUI.Instance != null && MainMenuUI.Instance.IsOpen) return;
            if (UIEscape.TryConsume(input)) Open(!open);
        }
    }
}
