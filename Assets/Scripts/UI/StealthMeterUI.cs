// Implements: approved UI item U2 (2026-10-01) — eye icon that fills as you're being spotted (white → amber → red), "Hidden" in
// cover. Bottom-centre. Only while a stealth sequence is active (attack slots are hidden by CombatDisabled).
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class StealthMeterUI : UIElement
    {
        RectTransform panel;
        Text eye, status;
        UIBar meter;

        protected override void OnBuild()
        {
            panel = UIFactory.Node(Root, "Stealth");
            UIFactory.Place(panel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(300f, 80f));
            eye = UIFactory.Label(panel, "Eye", "◉", 40, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Place(eye.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(60f, 46f));
            meter = UIFactory.Bar(panel, "Meter", Color.white, new Vector2(220f, 10f));
            UIFactory.Place(meter.Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(220f, 10f));
            status = UIFactory.Label(panel, "Status", "", 16, TextAnchor.MiddleCenter, new Color(0.8f, 0.9f, 1f));
            UIFactory.Place(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(300f, 20f));
            panel.gameObject.SetActive(false);
        }

        void Update()
        {
            var s = StealthSystem.Instance;
            bool show = s != null && s.Active;
            panel.gameObject.SetActive(show);
            if (!show) return;
            var col = StealthMath.MeterColor(s.Detection);
            eye.color = s.PlayerHidden && s.Detection < 0.05f ? new Color(1f, 1f, 1f, 0.35f) : col;
            meter.Fill.color = col;
            meter.Set(s.Detection);
            status.text = s.Spotted ? "SPOTTED" : s.PlayerHidden ? "Hidden" : s.Detection > 0.05f ? "Being seen…" : "";
        }
    }
}
