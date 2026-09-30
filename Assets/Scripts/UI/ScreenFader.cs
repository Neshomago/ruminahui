// Implements: approved UI plan item 8 + 05 Section 3 — full-screen fade/flash that masks the M3.5 camera cut + controller swap
// (no loading screen), vision flashes, and mission transitions.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class ScreenFader : UIElement
    {
        public static ScreenFader Instance { get; private set; }
        Image image;

        protected override void OnBuild()
        {
            Instance = this;
            image = UIFactory.Panel(Root, "Fade", new Color(0f, 0f, 0f, 0f));
            UIFactory.Stretch(image.rectTransform);
            image.raycastTarget = false;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public IEnumerator FadeOut(float duration, Color color)
        {
            color.a = image.color.a;
            image.color = color;
            yield return Animate(1f, duration);
        }

        public IEnumerator FadeIn(float duration) => Animate(0f, duration);

        /// <summary>Quick coloured flash (vision beats).</summary>
        public IEnumerator VisionFlash(float duration, Color color)
        {
            yield return FadeOut(duration * 0.4f, color);
            yield return FadeIn(duration * 0.6f);
        }

        IEnumerator Animate(float targetAlpha, float duration)
        {
            float start = image.color.a;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var c = image.color;
                c.a = Mathf.Lerp(start, targetAlpha, duration <= 0f ? 1f : t / duration);
                image.color = c;
                yield return null;
            }
            var end = image.color;
            end.a = targetAlpha;
            image.color = end;
        }
    }
}
