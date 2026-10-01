// Implements: 11-camera-performance.md, Part B Step 7 (pooled AudioSources for one-shot SFX);
//             06 Enemy Roster, Highland Scout tell (audio-only pre-ambush cue) — placeholder procedural beeps until real SFX exist.
using System.Collections.Generic;
using UnityEngine;

namespace Ruminahui
{
    public enum PlaceholderCue { Hit, Block, Parry, Tell, AmbushSnap, Roar, Flare, Rally, Swap, Vision, Heave, Pickup, Wind, Horn, DogBark, Spotted }

    public class AudioPool : Singleton<AudioPool>
    {
        public int size = 16;
        readonly List<AudioSource> sources = new List<AudioSource>();
        readonly Dictionary<PlaceholderCue, AudioClip> cueClips = new Dictionary<PlaceholderCue, AudioClip>();
        int next;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < size; i++)
            {
                var go = new GameObject("AudioSource_" + i);
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0.7f;
                src.rolloffMode = AudioRolloffMode.Linear;
                src.maxDistance = 45f;
                sources.Add(src);
            }
        }

        public void Play(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || sources.Count == 0) return;
            var src = sources[next];
            next = (next + 1) % sources.Count;
            src.transform.position = position;
            src.pitch = pitch;
            src.volume = volume;
            src.clip = clip;
            src.Play();
        }

        public void PlayCue(PlaceholderCue cue, Vector3 position, float volume = 0.5f)
        {
            if (!cueClips.TryGetValue(cue, out var clip))
            {
                clip = BuildCue(cue);
                cueClips[cue] = clip;
            }
            Play(clip, position, volume);
        }

        // PLACEHOLDER-BALANCE: procedural tones only so tells are audible before real audio lands.
        static AudioClip BuildCue(PlaceholderCue cue)
        {
            float freq, length;
            bool noise = false;
            switch (cue)
            {
                case PlaceholderCue.Hit: freq = 180f; length = 0.08f; noise = true; break;
                case PlaceholderCue.Block: freq = 320f; length = 0.1f; break;
                case PlaceholderCue.Parry: freq = 880f; length = 0.18f; break;
                case PlaceholderCue.Tell: freq = 520f; length = 0.15f; break;
                case PlaceholderCue.AmbushSnap: freq = 1400f; length = 0.06f; noise = true; break;
                case PlaceholderCue.Roar: freq = 90f; length = 0.7f; noise = true; break;
                case PlaceholderCue.Flare: freq = 1200f; length = 0.25f; noise = true; break;
                case PlaceholderCue.Rally: freq = 440f; length = 0.5f; break;
                case PlaceholderCue.Swap: freq = 660f; length = 0.2f; break;
                case PlaceholderCue.Vision: freq = 220f; length = 1.2f; break;
                case PlaceholderCue.Heave: freq = 120f; length = 0.4f; break;
                case PlaceholderCue.Wind: freq = 60f; length = 3.5f; noise = true; break;      // shared by M0.1 opening and M5.6 ending
                case PlaceholderCue.Horn: freq = 150f; length = 1.4f; break;
                case PlaceholderCue.DogBark: freq = 600f; length = 0.12f; noise = true; break;
                case PlaceholderCue.Spotted: freq = 980f; length = 0.3f; break;
                default: freq = 760f; length = 0.15f; break;
            }
            const int rate = 44100;
            int samples = Mathf.CeilToInt(rate * length);
            var data = new float[samples];
            var rng = new System.Random((int)cue * 7919);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = 1f - (i / (float)samples);
                float s = Mathf.Sin(2f * Mathf.PI * freq * t);
                if (noise) s = Mathf.Lerp(s, (float)(rng.NextDouble() * 2.0 - 1.0), 0.6f);
                data[i] = s * env * 0.6f;
            }
            var clip = AudioClip.Create("Cue_" + cue, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
