// Implements: approved UI item U6 (2026-10-01) — main menu in _Boot: New Game · Continue · Missions · Settings · Quit.
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class MainMenuUI : UIElement
    {
        public static MainMenuUI Instance { get; private set; }
        public bool IsOpen { get; private set; }

        RectTransform panel;
        Button continueButton;
        Text continueLabel;
        Text confirmText;
        bool confirmNewGame;

        protected override void OnBuild()
        {
            Instance = this;
            panel = UIFactory.Panel(Root, "Panel", new Color(0.02f, 0.02f, 0.03f, 0.92f)).rectTransform;
            panel.GetComponent<Image>().raycastTarget = true;
            UIFactory.Stretch(panel);

            var title = UIFactory.Label(panel, "Title", "RUMIÑAHUI", 72, TextAnchor.MiddleCenter, new Color(0.85f, 0.78f, 0.65f));
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(1000f, 100f));

            var col = UIFactory.Node(panel, "Buttons");
            UIFactory.Place(col, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(380f, 360f));
            UIFactory.VerticalList(col, 12f, 0);
            UIFactory.Button(col, "New Game", NewGame, new Vector2(380f, 52f));
            continueButton = UIFactory.Button(col, "Continue", Continue, new Vector2(380f, 52f));
            continueLabel = continueButton.GetComponentInChildren<Text>();
            UIFactory.Button(col, "Missions", () => { Open(false); PauseMenu.Instance?.Open(true); }, new Vector2(380f, 52f));
            UIFactory.Button(col, "Settings", () => SettingsPanel.Instance?.Open(true), new Vector2(380f, 52f));
            UIFactory.Button(col, "Quit", Quit, new Vector2(380f, 52f));

            confirmText = UIFactory.Label(panel, "Confirm", "", 18, TextAnchor.MiddleCenter, new Color(1f, 0.8f, 0.5f));
            UIFactory.Place(confirmText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(900f, 30f));
            panel.gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(bool value)
        {
            if (IsOpen == value) return;
            IsOpen = value;
            panel.gameObject.SetActive(value);
            confirmNewGame = false;
            confirmText.text = "";
            if (value) GameInput.Instance?.PushMenu();
            else GameInput.Instance?.PopMenu();
        }

        void NewGame()
        {
            // Two clicks when a save exists — starting over overwrites it.
            if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave && !confirmNewGame)
            {
                confirmNewGame = true;
                confirmText.text = "This replaces your saved game. Click New Game again to confirm.";
                return;
            }
            Open(false);
            MissionManager.Instance?.NewGame();
        }

        void Continue()
        {
            if (SaveSystem.Instance == null || !SaveSystem.Instance.HasSave) return;
            Open(false);
            MissionManager.Instance?.ContinueFromSave();
        }

        static void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        void Update()
        {
            if (!IsOpen) return;
            var save = SaveSystem.Instance != null ? SaveSystem.Instance.LastSave : null;
            continueButton.interactable = save != null;
            var m = save != null ? MissionDatabase.Get(save.currentMission) : null;
            continueLabel.text = m != null ? $"Continue — {m.Id} {m.Title}" : "Continue (no save)";
        }
    }
}
