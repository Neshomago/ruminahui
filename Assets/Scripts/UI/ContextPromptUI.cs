// Implements: approved UI plan item 6 — "Grapple", "Heave", "Spare"… prompts (center-low). The Spare prompt has its own style
// and colour so it reads as a different kind of moment (08-atoc-boss-fight.md Section 2).
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class ContextPromptUI : UIElement
    {
        Image bg;
        Text text;
        UIBar hold;

        protected override void OnBuild()
        {
            bg = UIFactory.Panel(Root, "Prompt", UIFactory.PanelColor);
            UIFactory.Place(bg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 190f), new Vector2(460f, 52f));
            text = UIFactory.Label(bg.transform, "Text", "", 22, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(text.rectTransform);
            hold = UIFactory.Bar(bg.transform, "Hold", new Color(1f, 0.85f, 0.4f), new Vector2(440f, 6f));
            UIFactory.Place(hold.Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 3f), new Vector2(440f, 6f));
        }

        void Update()
        {
            var c = Controlled;
            string prompt = null;
            var style = PromptStyle.Normal;

            if (c != null && !c.ScriptLocked)
            {
                var sensor = c.Sensor;
                var current = sensor != null ? sensor.Current : null;
                if (current != null && current.OverridesKitContext)
                {
                    prompt = current.PromptFor(c);
                    style = current.Style;
                }
                if (prompt == null && c.Kit != null && c.Kit.GetContextPrompt(out var kitText, out var kitStyle))
                {
                    prompt = kitText;
                    style = kitStyle;
                }
                if (prompt == null && current != null)
                {
                    prompt = current.PromptFor(c);
                    style = current.Style;
                }
                hold.Background.gameObject.SetActive(sensor != null && sensor.IsHolding);
                if (sensor != null) hold.Set(sensor.HoldProgress01);
            }

            bg.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
            if (string.IsNullOrEmpty(prompt)) return;

            text.text = $"[E / RT]  {prompt}";
            switch (style)
            {
                case PromptStyle.Spare:
                    bg.color = new Color(0.9f, 0.9f, 0.85f, 0.9f);   // pale, quiet — not a red "execute" prompt
                    text.color = new Color(0.15f, 0.2f, 0.3f);
                    text.fontStyle = FontStyle.Italic;
                    text.text = prompt; // no button-mash framing
                    break;
                case PromptStyle.Grapple:
                    bg.color = new Color(0.35f, 0.2f, 0.05f, 0.8f);
                    text.color = Color.white;
                    text.fontStyle = FontStyle.Bold;
                    break;
                default:
                    bg.color = UIFactory.PanelColor;
                    text.color = Color.white;
                    text.fontStyle = FontStyle.Normal;
                    break;
            }
        }
    }
}
