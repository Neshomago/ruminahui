// Implements: approved UI item U3 (2026-10-01) — escort health bar(s) + an M5.2 countdown, top-centre (below the boss bar slot).
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class EscortPanelUI : UIElement
    {
        const int MaxBars = 4;
        RectTransform panel;
        Text title, timer;
        readonly UIBar[] bars = new UIBar[MaxBars];
        readonly Text[] names = new Text[MaxBars];

        protected override void OnBuild()
        {
            panel = UIFactory.Panel(Root, "Escort", UIFactory.PanelColor).rectTransform;
            UIFactory.Place(panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(560f, 150f));
            title = UIFactory.Label(panel, "Title", "", 18, TextAnchor.UpperLeft, Color.white);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -6f), new Vector2(400f, 24f));
            timer = UIFactory.Label(panel, "Timer", "", 26, TextAnchor.UpperRight, new Color(1f, 0.85f, 0.4f));
            UIFactory.Place(timer.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-12f, -2f), new Vector2(140f, 32f));
            for (int i = 0; i < MaxBars; i++)
            {
                names[i] = UIFactory.Label(panel, "Name" + i, "", 14, TextAnchor.MiddleLeft, Color.white);
                UIFactory.Place(names[i].rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -36f - i * 27f), new Vector2(170f, 22f));
                bars[i] = UIFactory.Bar(panel, "Bar" + i, new Color(0.4f, 0.75f, 0.9f), new Vector2(350f, 14f));
                UIFactory.Place(bars[i].Rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190f, -40f - i * 27f), new Vector2(350f, 14f));
            }
            panel.gameObject.SetActive(false);
        }

        void Update()
        {
            bool show = EscortHud.Visible && (EscortTarget.All.Count > 0 || EscortHud.TimeRemaining >= 0f);
            panel.gameObject.SetActive(show);
            if (!show) return;
            title.text = EscortHud.Title;
            timer.text = EscortHud.FormatTime(EscortHud.TimeRemaining);
            timer.color = EscortHud.TimeRemaining >= 0f && EscortHud.TimeRemaining < 20f ? new Color(1f, 0.35f, 0.3f) : new Color(1f, 0.85f, 0.4f);
            for (int i = 0; i < MaxBars; i++)
            {
                bool on = i < EscortTarget.All.Count;
                names[i].gameObject.SetActive(on);
                bars[i].Background.gameObject.SetActive(on);
                if (!on) continue;
                var e = EscortTarget.All[i];
                string state = e.Lost ? " — lost" : e.Arrived ? " — safe" : e.Halted ? " — halted" : "";
                names[i].text = e.displayName + state;
                bars[i].Set(e.Health != null ? e.Health.Normalized : 0f);
                bars[i].Fill.color = e.Arrived ? new Color(0.4f, 0.85f, 0.45f) : e.Halted ? new Color(1f, 0.7f, 0.3f) : new Color(0.4f, 0.75f, 0.9f);
            }
        }
    }
}
