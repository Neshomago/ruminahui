// Implements: approved UI plan — placeholder uGUI built in code (flat panels, built-in font). Each element is its own component
// so real UI art can later be a prefab with the same component. Uses legacy Text (TextMeshPro needs an essentials import).
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public static class UIFactory
    {
        static Font font;
        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.55f);
        public static readonly Color HpColor = new Color(0.8f, 0.2f, 0.2f);
        public static readonly Color StaColor = new Color(0.3f, 0.8f, 0.3f);
        public static readonly Color FocColor = new Color(1f, 0.8f, 0.25f);

        public static RectTransform Node(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Places a rect by anchor (0..1), pivot, anchored position and size.</summary>
        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Panel(Transform parent, string name, Color color)
        {
            var rt = Node(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, TextAnchor align, Color color)
        {
            var rt = Node(parent, name);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static UIBar Bar(Transform parent, string name, Color fill, Vector2 size)
        {
            var bg = Panel(parent, name, new Color(0f, 0f, 0f, 0.6f));
            bg.rectTransform.sizeDelta = size;
            var f = Panel(bg.transform, "Fill", fill);
            var frt = f.rectTransform;
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(1f, 1f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.offsetMin = new Vector2(2f, 2f);
            frt.offsetMax = new Vector2(-2f, -2f);
            return new UIBar(bg, f);
        }

        public static Button Button(Transform parent, string label, System.Action onClick, Vector2 size)
        {
            var rt = Node(parent, "Button_" + label);
            rt.sizeDelta = size;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.18f, 0.95f);
            var b = rt.gameObject.AddComponent<Button>();
            var colors = b.colors;
            colors.highlightedColor = new Color(0.35f, 0.3f, 0.2f, 1f);
            colors.pressedColor = new Color(0.6f, 0.5f, 0.25f, 1f);
            b.colors = colors;
            b.onClick.AddListener(() => onClick());
            var t = Label(rt, "Label", label, 16, TextAnchor.MiddleCenter, Color.white);
            Stretch(t.rectTransform);
            return b;
        }

        public static VerticalLayoutGroup VerticalList(RectTransform rt, float spacing, int padding)
        {
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.childControlHeight = false;
            v.childControlWidth = false;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = false;
            return v;
        }
    }

    /// <summary>Bar via anchor scaling — works without sprites (Image.fillAmount needs one).</summary>
    public class UIBar
    {
        public readonly Image Background;
        public readonly Image Fill;

        public UIBar(Image bg, Image fill) { Background = bg; Fill = fill; }

        public RectTransform Rect => Background.rectTransform;

        public void Set(float normalized)
        {
            var rt = Fill.rectTransform;
            rt.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
        }
    }
}
