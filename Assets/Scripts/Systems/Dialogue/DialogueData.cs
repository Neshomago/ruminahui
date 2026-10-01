// Implements: 04-dialogue-script.md as data (AI_BUILD_PROMPT.md: "use 04 … later as text data sources").
// Generated JSON lives in Assets/Resources/Dialogue/<M0_1>.json — produced by Tools/DialogueImport/import_dialogue.py from the doc.
// The doc is the source of truth: edit the doc, re-run the importer, never hand-edit the JSON.
namespace Ruminahui
{
    public enum DialogueBeatKind { Line, Action, Gameplay, Qte, TextCard, Unknown }

    [System.Serializable]
    public class DialogueBeat
    {
        public string id;          // stable "<mission>.<scene>.<beat>", e.g. "M5.6.2.05"
        public string kind;        // line | action | gameplay | qte | textcard
        public string speaker;     // display name ("Rumiñahui"), lines only
        public string direction;   // parenthetical delivery note, lines only
        public string text;
        public string[] prompts;   // quoted prompt labels from gameplay/QTE notes ("WARN HIM", "STAY SILENT")

        public DialogueBeatKind Kind
        {
            get
            {
                switch (kind)
                {
                    case "line": return DialogueBeatKind.Line;
                    case "action": return DialogueBeatKind.Action;
                    case "gameplay": return DialogueBeatKind.Gameplay;
                    case "qte": return DialogueBeatKind.Qte;
                    case "textcard": return DialogueBeatKind.TextCard;
                    default: return DialogueBeatKind.Unknown;
                }
            }
        }

        public bool HasPrompts => prompts != null && prompts.Length > 0;
    }

    [System.Serializable]
    public class DialogueScene
    {
        public string heading;     // slugline / section ("EXT. PÍLLARO VILLAGE — NIGHT", "TEXT CARD (closing the game)")
        public DialogueBeat[] beats;
    }

    [System.Serializable]
    public class DialogueScript
    {
        public string missionId;
        public string docTitle;
        public string subtitle;
        public DialogueScene[] scenes;

        public System.Collections.Generic.IEnumerable<DialogueBeat> AllBeats()
        {
            if (scenes == null) yield break;
            foreach (var s in scenes)
                if (s.beats != null)
                    foreach (var b in s.beats) yield return b;
        }

        public DialogueBeat Find(string beatId)
        {
            foreach (var b in AllBeats()) if (b.id == beatId) return b;
            return null;
        }
    }
}
