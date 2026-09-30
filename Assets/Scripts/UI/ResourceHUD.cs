// Implements: approved UI plan item 1 — HP / Stamina / Focus bars + character portrait tile (bottom-left).
// Focus is the shared party meter (03 Section 0), so it doesn't change on swap.
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class ResourceHUD : UIElement
    {
        UIBar hp, sta, foc;
        Image portrait;
        Text nameText, hpText, focText, statusText;

        protected override void OnBuild()
        {
            var panel = UIFactory.Panel(Root, "Panel", UIFactory.PanelColor);
            UIFactory.Place(panel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(430f, 132f));

            portrait = UIFactory.Panel(panel.transform, "Portrait", Color.gray);
            UIFactory.Place(portrait.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -10f), new Vector2(84f, 84f));
            nameText = UIFactory.Label(panel.transform, "Name", "", 16, TextAnchor.UpperCenter, Color.white);
            UIFactory.Place(nameText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(4f, 8f), new Vector2(96f, 24f));

            hp = UIFactory.Bar(panel.transform, "HP", UIFactory.HpColor, new Vector2(310f, 22f));
            UIFactory.Place(hp.Rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(108f, -12f), new Vector2(310f, 22f));
            hpText = UIFactory.Label(hp.Rect, "Text", "", 14, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(hpText.rectTransform);

            sta = UIFactory.Bar(panel.transform, "STA", UIFactory.StaColor, new Vector2(310f, 14f));
            UIFactory.Place(sta.Rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(108f, -42f), new Vector2(310f, 14f));

            foc = UIFactory.Bar(panel.transform, "FOC", UIFactory.FocColor, new Vector2(310f, 18f));
            UIFactory.Place(foc.Rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(108f, -64f), new Vector2(310f, 18f));
            focText = UIFactory.Label(foc.Rect, "Text", "", 13, TextAnchor.MiddleCenter, Color.black);
            UIFactory.Stretch(focText.rectTransform);

            statusText = UIFactory.Label(panel.transform, "Status", "", 13, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.6f));
            UIFactory.Place(statusText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(108f, -88f), new Vector2(315f, 40f));
        }

        void Update()
        {
            var c = Controlled;
            bool show = c != null;
            Root.gameObject.transform.GetChild(0).gameObject.SetActive(show);
            if (!show) return;

            portrait.color = c.color;
            nameText.text = c.displayName;
            hp.Set(c.Health.Normalized);
            hpText.text = $"HP {Mathf.CeilToInt(c.Health.Current)} / {Mathf.CeilToInt(c.Health.max)}";
            sta.Set(c.Stamina.Normalized);
            var f = c.Focus;
            if (f != null)
            {
                foc.Set(f.Normalized);
                focText.text = $"FOCUS {Mathf.FloorToInt(f.Current)}";
            }

            var s = c.Status;
            string st = c.Kit != null && c.Kit.IsBusy ? c.Kit.CurrentActionName : "";
            if (s.HasHyperArmor) st += "  [Unyielding]";
            if (s.IsStaggered) st += "  [Staggered]";
            if (c.Target.stealthed) st += "  [Hidden]";
            if (c.Target.invulnerable) st += "  [Invulnerable]";
            if (c.Kit is PumaKit pk && pk.IsBlocking) st += "  [Stone Stance]";
            if (c.Kit is AmaruKit ak) st += $"  Snares {ak.SnaresPlaced}/{ak.SnareCap}";
            if (TimeDilation.IsSlowActive) st += "  [Time slowed]";
            statusText.text = st;
        }
    }
}
