// Implements: approved call-in UI plan (2026-09-30) items 1-2 — a 3-slot strip above the ability strip (bottom-right): key, call-in
// name, ally colour, cooldown bar; greyed with a short reason when unavailable; hidden with no allies; while LB is held it
// brightens and shows the Y / X / B face-button glyphs.
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class CallInStripUI : UIElement
    {
        static readonly AllyCommand[] Order = { AllyCommand.Mark, AllyCommand.Trap, AllyCommand.Cover };
        static readonly string[] Keys = { "Z", "X", "C" };
        static readonly string[] PadKeys = { "Y", "X", "B" };

        RectTransform row;
        readonly Image[] bgs = new Image[3];
        readonly Image[] swatches = new Image[3];
        readonly Text[] labels = new Text[3];
        readonly UIBar[] cds = new UIBar[3];

        protected override void OnBuild()
        {
            row = UIFactory.Node(Root, "Row");
            UIFactory.Stretch(row);
            for (int i = 0; i < 3; i++)
            {
                // Sits over the three right-most ability slots (AbilityStripUI: 142 wide, 150 pitch, y 24..100).
                var bg = UIFactory.Panel(row, "CallIn" + i, UIFactory.PanelColor);
                UIFactory.Place(bg.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f - (2 - i) * 150f, 108f), new Vector2(142f, 54f));
                bgs[i] = bg;
                swatches[i] = UIFactory.Panel(bg.transform, "Ally", Color.gray);
                UIFactory.Place(swatches[i].rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, 2f), new Vector2(8f, 40f));
                labels[i] = UIFactory.Label(bg.transform, "Label", "", 13, TextAnchor.MiddleCenter, Color.white);
                UIFactory.Place(labels[i].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(4f, -2f), new Vector2(128f, 40f));
                cds[i] = UIFactory.Bar(bg.transform, "Cooldown", new Color(0.9f, 0.85f, 0.6f), new Vector2(124f, 6f));
                UIFactory.Place(cds[i].Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(4f, 5f), new Vector2(124f, 6f));
            }
        }

        void Update()
        {
            var sys = AllyCommandSystem.Instance;
            var swap = FreeSwapController.Instance;
            var leader = Controlled;
            bool show = sys != null && sys.HasAllies && (swap == null || !swap.Active) && (leader == null || !leader.CombatDisabled);
            row.gameObject.SetActive(show);
            if (!show) return;

            var input = GameInput.Instance;
            bool padHint = input != null && input.CommandModifierHeld;

            for (int i = 0; i < 3; i++)
            {
                var c = Order[i];
                var check = sys.Check(c, out var ally);
                string key = padHint ? "LB+" + PadKeys[i] : Keys[i];
                string who = ally != null ? ally.displayName : AllyCommandRules.ExpectedAlly(c);
                string status = check.Available ? who : check.Reason;
                labels[i].text = $"[{key}] {AllyCommandRules.Label(c)}\n<size=11>{status}</size>";
                labels[i].supportRichText = true;

                var allyColor = ally != null ? ally.color : Color.gray;
                bool usable = check.Available;
                swatches[i].color = usable ? allyColor : Color.Lerp(allyColor, Color.gray, 0.7f);
                labels[i].color = usable ? Color.white : new Color(0.6f, 0.6f, 0.6f);
                bgs[i].color = padHint
                    ? new Color(0.25f, 0.2f, 0.08f, 0.85f)       // LB held: brightened
                    : UIFactory.PanelColor;
                cds[i].Set(1f - sys.CooldownNormalized(c));
            }
        }
    }
}
