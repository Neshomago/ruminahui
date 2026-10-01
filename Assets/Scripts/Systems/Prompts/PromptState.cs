// Implements: 04-dialogue-script.md prompts — M0.1 QTE "WARN HIM" (single button, deliberately generous window, never winnable),
// M5.5 "STAND" / "RUN" (a choice framed honestly as unwinnable), M5.6 "STAY SILENT" (repeated, no fail state, no timer).
// Pure state machine (no MonoBehaviour) so timing rules are unit-testable; PromptSystem drives it and PromptUI draws it.
namespace Ruminahui
{
    public enum PromptKind { None, Timed, Choice, Quiet }

    public enum PromptOutcome { Pending, Pressed, Missed, ChoseA, ChoseB }

    public class PromptState
    {
        public PromptKind Kind { get; private set; } = PromptKind.None;
        public string LabelA { get; private set; } = "";
        public string LabelB { get; private set; } = "";
        public string Caption { get; private set; } = "";
        public float Window { get; private set; }
        public float Elapsed { get; private set; }
        public PromptOutcome Outcome { get; private set; } = PromptOutcome.Pending;

        public bool IsActive => Kind != PromptKind.None && Outcome == PromptOutcome.Pending;
        /// <summary>0 → 1 as a timed prompt runs out (the timing ring).</summary>
        public float Progress01 => Window > 0f ? System.Math.Min(1f, Elapsed / Window) : 0f;

        public void BeginTimed(string label, float window, string caption = "")
        {
            Reset(PromptKind.Timed, label, "", caption);
            Window = System.Math.Max(0.1f, window);
        }

        /// <summary>Choice: A = Interact (E / RT), B = Dodge (Shift / B). timeout 0 = wait forever.</summary>
        public void BeginChoice(string a, string b, string caption = "", float timeout = 0f)
        {
            Reset(PromptKind.Choice, a, b, caption);
            Window = timeout;
        }

        /// <summary>Quiet: press when ready. No timer, no fail.</summary>
        public void BeginQuiet(string label, string caption = "") => Reset(PromptKind.Quiet, label, "", caption);

        void Reset(PromptKind kind, string a, string b, string caption)
        {
            Kind = kind;
            LabelA = a ?? "";
            LabelB = b ?? "";
            Caption = caption ?? "";
            Elapsed = 0f;
            Window = 0f;
            Outcome = PromptOutcome.Pending;
        }

        /// <summary>Advance one frame. Returns true when the prompt resolved this tick.</summary>
        public bool Tick(float dt, bool pressedA, bool pressedB)
        {
            if (!IsActive) return false;
            Elapsed += dt;
            switch (Kind)
            {
                case PromptKind.Timed:
                    if (pressedA && Elapsed <= Window) { Outcome = PromptOutcome.Pressed; return true; }
                    if (Elapsed > Window) { Outcome = PromptOutcome.Missed; return true; }
                    return false;
                case PromptKind.Choice:
                    if (pressedA) { Outcome = PromptOutcome.ChoseA; return true; }
                    if (pressedB) { Outcome = PromptOutcome.ChoseB; return true; }
                    if (Window > 0f && Elapsed > Window) { Outcome = PromptOutcome.ChoseA; return true; } // default: first option
                    return false;
                case PromptKind.Quiet:
                    if (pressedA) { Outcome = PromptOutcome.Pressed; return true; }
                    return false;
            }
            return false;
        }

        public void Clear()
        {
            Kind = PromptKind.None;
            Outcome = PromptOutcome.Pending;
        }
    }
}
