// Implements: approved UI plan item 5 — boss name, HP, tick marks at the phase thresholds (Atoc: 65% / 30%, 08 Section 2).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class BossBarUI : UIElement
    {
        RectTransform panel;
        UIBar bar;
        Text nameText;
        readonly List<Image> ticks = new List<Image>();

        protected override void OnBuild()
        {
            panel = UIFactory.Panel(Root, "Panel", UIFactory.PanelColor).rectTransform;
            UIFactory.Place(panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(760f, 64f));
            nameText = UIFactory.Label(panel, "Name", "", 20, TextAnchor.UpperCenter, Color.white);
            UIFactory.Place(nameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(740f, 26f));
            bar = UIFactory.Bar(panel, "HP", new Color(0.75f, 0.15f, 0.2f), new Vector2(730f, 20f));
            UIFactory.Place(bar.Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(730f, 20f));
        }

        void Update()
        {
            var boss = BossBarTarget.Current;
            panel.gameObject.SetActive(boss != null);
            if (boss == null || boss.Health == null) return;

            string phase = "";
            var atoc = boss.GetComponent<AtocBoss>();
            if (atoc != null) phase = $"   —   Phase {(int)Mathf.Min((int)atoc.Phase, 3)}: {AtocBoss.PhaseLabel(atoc.Phase)}";
            nameText.text = boss.displayName + phase;
            bar.Set(boss.Health.Normalized);

            while (ticks.Count < boss.phaseTicks.Count)
            {
                var t = UIFactory.Panel(bar.Rect, "Tick", Color.white);
                ticks.Add(t);
            }
            for (int i = 0; i < ticks.Count; i++)
            {
                bool on = i < boss.phaseTicks.Count;
                ticks[i].gameObject.SetActive(on);
                if (!on) continue;
                var rt = ticks[i].rectTransform;
                rt.anchorMin = new Vector2(boss.phaseTicks[i], 0f);
                rt.anchorMax = new Vector2(boss.phaseTicks[i], 1f);
                rt.sizeDelta = new Vector2(3f, 6f);
                rt.anchoredPosition = Vector2.zero;
            }
        }
    }
}
