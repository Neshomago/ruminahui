// Testing aid (approved: "each test scene shows its controls on screen") — controls for the active kit + current objective +
// last dialogue/VO placeholder line. Toggle with H.
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class TestHintUI : UIElement
    {
        RectTransform panel;
        Text text;
        bool visible = true;

        const string Common =
            "WASD/L-stick move · Mouse/R-stick look · Space/A jump\n" +
            "LMB/X light · RMB/Y heavy (Puma: hold to charge) · Shift/B dodge\n" +
            "Q/LT block (Puma) or counter (Amaru) · E/RT interact/grapple\n" +
            "Tab/MMB/R3 lock-on (+ Condor's Eye mark) · R F G / d-pad specials · V/L3 ultimate\n" +
            "T/d-pad↓ Snare (Amaru) · Esc pause · F1 debug · H hide this\n" +
            "Ally call-ins: Z Mark · X Trap · C Cover  (gamepad: hold LB + Y / X / B)";

        protected override void OnBuild()
        {
            panel = UIFactory.Panel(Root, "Panel", new Color(0f, 0f, 0f, 0.45f)).rectTransform;
            UIFactory.Place(panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(600f, 210f));
            text = UIFactory.Label(panel, "Text", "", 14, TextAnchor.UpperLeft, Color.white);
            UIFactory.Stretch(text.rectTransform).offsetMin = new Vector2(10f, 8f);
            text.rectTransform.offsetMax = new Vector2(-10f, -8f);
        }

        void Update()
        {
            var input = GameInput.Instance;
            if (input != null && input.HintToggle.WasPressedThisFrame()) visible = !visible;
            panel.gameObject.SetActive(visible);
            if (!visible) return;

            var c = Controlled;
            string kit = c != null ? $"{c.displayName} — {(c.Kit != null ? c.Kit.Kit.ToString() : "")} kit" : "(no controlled character)";
            string mission = MissionManager.Instance != null ? MissionManager.Instance.CurrentContentId : "";
            text.text = $"<b>{mission}</b>  ·  {kit}\n" +
                        $"<color=#ffd27a>{ObjectiveTracker.Current}</color>\n" +
                        $"<i>{ObjectiveTracker.LastLine}</i>\n" + Common;
            text.supportRichText = true;
        }
    }
}
