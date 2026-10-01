// Implements: 04 prompts (WARN HIM / STAND-RUN / STAY SILENT) as coroutines mission directors can yield on. Gameplay input is
// blocked while a prompt is up; the prompt reads Interact (A) and Dodge (B) directly. Runs on unscaled time.
using System.Collections;
using UnityEngine;

namespace Ruminahui
{
    public class PromptSystem : Singleton<PromptSystem>
    {
        public readonly PromptState State = new PromptState();

        /// <summary>Single-button timed prompt (QTE). Result: true if pressed in the window.</summary>
        public IEnumerator Timed(string label, float window, System.Action<bool> result, string caption = "")
        {
            State.BeginTimed(label, window, caption);
            yield return Run();
            result?.Invoke(State.Outcome == PromptOutcome.Pressed);
            State.Clear();
        }

        /// <summary>Two-option choice. Result: 0 = A (Interact), 1 = B (Dodge).</summary>
        public IEnumerator Choice(string a, string b, System.Action<int> result, string caption = "", float timeout = 0f)
        {
            State.BeginChoice(a, b, caption, timeout);
            yield return Run();
            result?.Invoke(State.Outcome == PromptOutcome.ChoseB ? 1 : 0);
            State.Clear();
        }

        /// <summary>Quiet prompt: waits for a press. Never fails.</summary>
        public IEnumerator Quiet(string label, string caption = "")
        {
            State.BeginQuiet(label, caption);
            yield return Run();
            State.Clear();
        }

        IEnumerator Run()
        {
            var input = GameInput.Instance;
            if (input != null) input.PushCutscene();
            AudioPool.Instance?.PlayCue(PlaceholderCue.Tell, Vector3.zero, 0.3f);
            yield return null; // don't let the press that triggered the beat also answer the prompt
            while (true)
            {
                bool a = input != null && input.Interact.WasPressedThisFrame();
                bool b = input != null && input.Dodge.WasPressedThisFrame();
                if (State.Tick(Time.unscaledDeltaTime, a, b)) break;
                yield return null;
            }
            if (input != null) input.PopCutscene();
        }
    }
}
