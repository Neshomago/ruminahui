// Implements: approved UI plan item 2 — specials + ultimate with Focus cost and cooldown; locked ones greyed (bottom-right).
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class AbilityStripUI : UIElement
    {
        static readonly string[] Keys = { "R", "F", "G", "V" };
        readonly Image[] bgs = new Image[4];
        readonly Text[] labels = new Text[4];
        readonly UIBar[] cds = new UIBar[4];

        protected override void OnBuild()
        {
            for (int i = 0; i < 4; i++)
            {
                var bg = UIFactory.Panel(Root, "Slot" + i, UIFactory.PanelColor);
                UIFactory.Place(bg.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f - (3 - i) * 150f, 24f), new Vector2(142f, 76f));
                bgs[i] = bg;
                labels[i] = UIFactory.Label(bg.transform, "Label", "", 14, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Place(labels[i].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(136f, 56f));
                cds[i] = UIFactory.Bar(bg.transform, "Cooldown", new Color(0.6f, 0.6f, 0.9f), new Vector2(130f, 8f));
                UIFactory.Place(cds[i].Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(130f, 8f));
            }
        }

        void Update()
        {
            var c = Controlled;
            var kit = c != null ? c.Kit : null;
            for (int i = 0; i < 4; i++)
            {
                AbilitySlot slot = null;
                if (kit != null) slot = i < 3 ? (i < kit.Specials.Count ? kit.Specials[i] : null) : kit.Ultimate;
                bgs[i].gameObject.SetActive(slot != null);
                if (slot == null) continue;

                bool unlocked = slot.Unlocked;
                bool affordable = kit.Focus.Has(slot.FocusCost);
                labels[i].text = unlocked
                    ? $"[{Keys[i]}] {slot.DisplayName}\n{slot.FocusCost:0} FOC"
                    : $"[{Keys[i]}] {slot.DisplayName}\nLOCKED";
                labels[i].color = !unlocked ? new Color(0.5f, 0.5f, 0.5f) : affordable && slot.OffCooldown ? Color.white : new Color(0.8f, 0.7f, 0.5f);
                bgs[i].color = slot.IsUltimate ? new Color(0.25f, 0.18f, 0.05f, 0.7f) : UIFactory.PanelColor;
                cds[i].Set(1f - slot.CooldownNormalized);
            }
        }
    }
}
