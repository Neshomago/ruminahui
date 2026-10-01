// Implements: approved UI item U4 (2026-10-01) — subtitles for 04-dialogue-script.md playback: speaker name in their colour + line,
// bottom-centre, on by default (Settings ▸ Subtitles). Subscribes to DialogueRunner.BeatPlayed (lines + text cards).
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class SubtitleUI : UIElement
    {
        Image bg;
        Text text;
        float hideAt;

        protected override void OnBuild()
        {
            bg = UIFactory.Panel(Root, "Subtitle", new Color(0f, 0f, 0f, 0.6f));
            UIFactory.Place(bg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 250f), new Vector2(1100f, 70f));
            text = UIFactory.Label(bg.transform, "Text", "", 24, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(text.rectTransform).offsetMin = new Vector2(16f, 6f);
            text.rectTransform.offsetMax = new Vector2(-16f, -6f);
            text.supportRichText = true;
            bg.gameObject.SetActive(false);
            DialogueRunner.BeatPlayed += OnBeat;
        }

        void OnDestroy() => DialogueRunner.BeatPlayed -= OnBeat;

        public static Color SpeakerColor(string speaker)
        {
            switch (speaker)
            {
                case "Rumiñahui": case "Pillahuaso": return Color.Lerp(CharacterFactory.RuminahuiColor, Color.white, 0.45f);
                case "Chaska": return Color.Lerp(CharacterFactory.ChaskaColor, Color.white, 0.35f);
                case "Atoc": return Color.Lerp(CharacterFactory.AtocColor, Color.white, 0.4f);
                case "Willka": return new Color(1f, 0.85f, 0.55f);
                default: return new Color(0.85f, 0.85f, 0.85f);
            }
        }

        void OnBeat(DialogueBeat beat)
        {
            var settings = SaveSystem.Instance != null ? SaveSystem.Instance.Settings : null;
            if (settings != null && !settings.subtitles) return;

            if (beat.Kind == DialogueBeatKind.Line)
            {
                var hex = ColorUtility.ToHtmlStringRGB(SpeakerColor(beat.speaker));
                Show($"<color=#{hex}><b>{beat.speaker}</b></color>  {beat.text}", DialogueRunner.LineDuration(beat.text), FontStyle.Normal);
            }
            else if (beat.Kind == DialogueBeatKind.TextCard)
            {
                Show(beat.text, DialogueRunner.LineDuration(beat.text) + 1f, FontStyle.Italic);
            }
        }

        void Show(string content, float seconds, FontStyle style)
        {
            text.text = content;
            text.fontStyle = style;
            bg.gameObject.SetActive(true);
            hideAt = Time.time + seconds;
        }

        void Update()
        {
            if (bg.gameObject.activeSelf && Time.time >= hideAt) bg.gameObject.SetActive(false);
        }
    }
}
