// Implements: approved UI item U6 (settings half) — quality tier (11-camera-performance.md Part B Step 9: Standard / Performance),
// volumes, sensitivity, subtitles, tell flashes, damage numbers. Stepper buttons instead of sliders (simpler to build in code).
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class SettingsPanel : UIElement
    {
        public static SettingsPanel Instance { get; private set; }
        public bool IsOpen { get; private set; }

        RectTransform panel;
        Text quality, master, music, mouse, stick, subs, tells, numbers;

        protected override void OnBuild()
        {
            Instance = this;
            panel = UIFactory.Panel(Root, "Panel", new Color(0.03f, 0.03f, 0.05f, 0.95f)).rectTransform;
            panel.GetComponent<Image>().raycastTarget = true;
            UIFactory.Stretch(panel);

            var col = UIFactory.Node(panel, "Column");
            UIFactory.Place(col, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 620f));
            UIFactory.VerticalList(col, 10f, 10);

            var title = UIFactory.Label(col, "Title", "SETTINGS", 32, TextAnchor.MiddleCenter, Color.white);
            title.rectTransform.sizeDelta = new Vector2(700f, 50f);

            quality = Row(col, () => S.qualityTier = S.qualityTier == GameSettings.QualityStandard ? GameSettings.QualityPerformance : GameSettings.QualityStandard, null);
            master = Row(col, () => S.masterVolume += 0.1f, () => S.masterVolume -= 0.1f);
            music = Row(col, () => S.musicVolume += 0.1f, () => S.musicVolume -= 0.1f);
            mouse = Row(col, () => S.mouseSensitivity += 0.02f, () => S.mouseSensitivity -= 0.02f);
            stick = Row(col, () => S.stickSensitivity += 20f, () => S.stickSensitivity -= 20f);
            subs = Row(col, () => S.subtitles = !S.subtitles, null);
            tells = Row(col, () => S.tellFlashes = !S.tellFlashes, null);
            numbers = Row(col, () => S.damageNumbers = !S.damageNumbers, null);

            UIFactory.Button(col, "Back", () => Open(false), new Vector2(700f, 46f));
            panel.gameObject.SetActive(false);
        }

        static GameSettings S => SaveSystem.Instance != null ? SaveSystem.Instance.Settings : Fallback;
        static readonly GameSettings Fallback = new GameSettings();

        Text Row(RectTransform parent, System.Action plus, System.Action minus)
        {
            var row = UIFactory.Node(parent, "Row");
            row.sizeDelta = new Vector2(700f, 44f);
            var label = UIFactory.Label(row, "Label", "", 20, TextAnchor.MiddleLeft, Color.white);
            UIFactory.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(460f, 40f));
            if (minus != null)
            {
                var m = UIFactory.Button(row, "−", () => { minus(); Changed(); }, new Vector2(100f, 40f));
                UIFactory.Place((RectTransform)m.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(100f, 40f));
            }
            var p = UIFactory.Button(row, minus != null ? "+" : "Toggle", () => { plus(); Changed(); }, new Vector2(100f, 40f));
            UIFactory.Place((RectTransform)p.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-10f, 0f), new Vector2(100f, 40f));
            return label;
        }

        static void Changed() => SaveSystem.Instance?.SaveSettings();

        public void Open(bool value)
        {
            if (IsOpen == value) return;
            IsOpen = value;
            panel.gameObject.SetActive(value);
            if (value) GameInput.Instance?.PushMenu();
            else GameInput.Instance?.PopMenu();
        }

        void Update()
        {
            if (!IsOpen) return;
            var input = GameInput.Instance;
            if (UIEscape.TryConsume(input)) { Open(false); return; }
            var s = S;
            quality.text = "Quality: " + (s.qualityTier == GameSettings.QualityPerformance ? "Performance" : "Standard");
            master.text = $"Master volume: {Mathf.RoundToInt(s.masterVolume * 100f)}%";
            music.text = $"Music volume: {Mathf.RoundToInt(s.musicVolume * 100f)}%";
            mouse.text = $"Mouse sensitivity: {s.mouseSensitivity:0.00}";
            stick.text = $"Stick sensitivity: {s.stickSensitivity:0}";
            subs.text = "Subtitles: " + (s.subtitles ? "On" : "Off");
            tells.text = "Tell flashes: " + (s.tellFlashes ? "On" : "Off");
            numbers.text = "Damage numbers: " + (s.damageNumbers ? "On" : "Off");
        }
    }
}
