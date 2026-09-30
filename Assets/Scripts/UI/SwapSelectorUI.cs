// Implements: approved UI plan item 7 + 06 M5.3 Section 3 ("simple radial or shoulder-button cycle") — three portraits, the active
// one highlighted; greyed while swapping is locked (stage F). Only visible while free swap is active.
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class SwapSelectorUI : UIElement
    {
        static readonly CharacterId[] Order = { CharacterId.Ruminahui, CharacterId.Chaska, CharacterId.Atoc };
        RectTransform panel;
        readonly Image[] tiles = new Image[3];
        readonly Text[] labels = new Text[3];
        Text lockText;

        protected override void OnBuild()
        {
            panel = UIFactory.Panel(Root, "Panel", UIFactory.PanelColor).rectTransform;
            UIFactory.Place(panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(330f, 120f));
            for (int i = 0; i < 3; i++)
            {
                tiles[i] = UIFactory.Panel(panel, "Tile" + i, Color.gray);
                UIFactory.Place(tiles[i].rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f + i * 106f, -10f), new Vector2(96f, 72f));
                labels[i] = UIFactory.Label(tiles[i].transform, "Label", "", 14, TextAnchor.LowerCenter, Color.white);
                UIFactory.Stretch(labels[i].rectTransform);
            }
            lockText = UIFactory.Label(panel, "Lock", "", 14, TextAnchor.LowerCenter, new Color(1f, 0.8f, 0.5f));
            UIFactory.Place(lockText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(320f, 24f));
        }

        void Update()
        {
            var swap = FreeSwapController.Instance;
            var pm = PartyManager.Instance;
            bool show = swap != null && swap.Active && pm != null;
            panel.gameObject.SetActive(show);
            if (!show) return;

            for (int i = 0; i < 3; i++)
            {
                var c = pm.Get(Order[i]);
                bool active = c != null && c == pm.Controlled;
                bool unavailable = c == null || swap.IsLocked || c.ScriptLocked;
                var col = CharacterFactory.ColorFor(Order[i]);
                if (unavailable && !active) col = Color.Lerp(col, Color.gray, 0.7f);
                tiles[i].color = col;
                tiles[i].rectTransform.localScale = active ? Vector3.one * 1.08f : Vector3.one;
                labels[i].text = $"[{i + 1}] {CharacterFactory.NameFor(Order[i])}" + (active ? "\n▲" : "");
            }
            lockText.text = swap.IsLocked ? "Swap locked — mid-action" : "LB / RB  or  1 · 2 · 3";
        }
    }
}
