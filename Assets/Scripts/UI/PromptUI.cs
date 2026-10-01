// Implements: approved UI item U1 (2026-10-01) — centred prompt: timed ("WARN HIM") with a shrinking timing ring (bar placeholder),
// a two-option choice (STAND / RUN, with an honest caption), and a quiet text-only style ("STAY SILENT").
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class PromptUI : UIElement
    {
        RectTransform panel;
        Text main, caption, optionA, optionB;
        UIBar ring;

        protected override void OnBuild()
        {
            panel = UIFactory.Node(Root, "Prompt");
            UIFactory.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(900f, 260f));
            main = UIFactory.Label(panel, "Main", "", 54, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Place(main.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 80f));
            main.gameObject.AddComponent<Outline>().effectColor = Color.black;
            ring = UIFactory.Bar(panel, "Ring", new Color(1f, 0.85f, 0.4f), new Vector2(420f, 10f));
            UIFactory.Place(ring.Rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(420f, 10f));
            optionA = UIFactory.Label(panel, "A", "", 40, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Place(optionA.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, 40f), new Vector2(380f, 70f));
            optionB = UIFactory.Label(panel, "B", "", 40, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Place(optionB.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 40f), new Vector2(380f, 70f));
            caption = UIFactory.Label(panel, "Caption", "", 20, TextAnchor.MiddleCenter, new Color(0.85f, 0.85f, 0.85f));
            UIFactory.Place(caption.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(900f, 40f));
            panel.gameObject.SetActive(false);
        }

        void Update()
        {
            var sys = PromptSystem.Instance;
            var st = sys != null ? sys.State : null;
            bool show = st != null && st.IsActive;
            panel.gameObject.SetActive(show);
            if (!show) return;

            caption.text = st.Caption;
            switch (st.Kind)
            {
                case PromptKind.Timed:
                    main.gameObject.SetActive(true);
                    optionA.gameObject.SetActive(false);
                    optionB.gameObject.SetActive(false);
                    ring.Background.gameObject.SetActive(true);
                    main.text = $"[E / RT]  {st.LabelA}";
                    main.fontSize = 54;
                    main.color = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.3f), st.Progress01);
                    ring.Set(1f - st.Progress01);
                    break;
                case PromptKind.Choice:
                    main.gameObject.SetActive(false);
                    ring.Background.gameObject.SetActive(false);
                    optionA.gameObject.SetActive(true);
                    optionB.gameObject.SetActive(true);
                    optionA.text = $"[E / RT]\n{st.LabelA}";
                    optionB.text = $"[Shift / B]\n{st.LabelB}";
                    break;
                case PromptKind.Quiet:
                    main.gameObject.SetActive(true);
                    optionA.gameObject.SetActive(false);
                    optionB.gameObject.SetActive(false);
                    ring.Background.gameObject.SetActive(false);
                    main.text = st.LabelA;               // no button-mash framing — just the words
                    main.fontSize = 34;
                    main.color = new Color(0.8f, 0.8f, 0.8f, 0.85f);
                    caption.text = string.IsNullOrEmpty(st.Caption) ? "[E / RT]" : st.Caption;
                    break;
            }
        }
    }
}
