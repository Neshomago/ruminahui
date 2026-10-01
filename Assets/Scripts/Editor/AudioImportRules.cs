// Implements: 11-camera-performance.md Part B Step 7.2 — "Compress ambient/music tracks appropriately (streaming, compressed) versus
// short combat SFX (decompress-on-load)… get this convention right early". Applied automatically by folder on import:
//   …/Audio/Music/, …/Audio/Ambience/  → Streaming, Vorbis q0.5      (long, played one at a time)
//   …/Audio/VO/                          → Compressed in memory, Vorbis q0.7, mono
//   …/Audio/SFX/                         → Decompress on load, ADPCM, mono (short, frequent, pooled AudioSources)
// Clips outside these folders are left alone (with a warning) so nothing is silently mis-imported.
using UnityEditor;
using UnityEngine;

namespace Ruminahui.EditorTools
{
    public class AudioImportRules : AssetPostprocessor
    {
        public enum AudioCategory { None, Music, Ambience, VO, SFX }

        /// <summary>Pure folder → category mapping (unit-testable).</summary>
        public static AudioCategory Categorize(string assetPath)
        {
            var p = assetPath.Replace('\\', '/');
            if (p.Contains("/Audio/Music/")) return AudioCategory.Music;
            if (p.Contains("/Audio/Ambience/")) return AudioCategory.Ambience;
            if (p.Contains("/Audio/VO/")) return AudioCategory.VO;
            if (p.Contains("/Audio/SFX/")) return AudioCategory.SFX;
            return AudioCategory.None;
        }

        void OnPreprocessAudio()
        {
            var importer = assetImporter as AudioImporter;
            if (importer == null) return;
            var category = Categorize(assetPath);
            if (category == AudioCategory.None)
            {
                Debug.LogWarning($"[Audio] {assetPath} is outside Assets/Audio/{{Music,Ambience,VO,SFX}} — import settings not enforced.");
                return;
            }

            var s = importer.defaultSampleSettings;
            switch (category)
            {
                case AudioCategory.Music:
                case AudioCategory.Ambience:
                    s.loadType = AudioClipLoadType.Streaming;
                    s.compressionFormat = AudioCompressionFormat.Vorbis;
                    s.quality = 0.5f;
                    importer.forceToMono = false;
                    break;
                case AudioCategory.VO:
                    s.loadType = AudioClipLoadType.CompressedInMemory;
                    s.compressionFormat = AudioCompressionFormat.Vorbis;
                    s.quality = 0.7f;
                    importer.forceToMono = true;
                    break;
                case AudioCategory.SFX:
                    s.loadType = AudioClipLoadType.DecompressOnLoad;
                    s.compressionFormat = AudioCompressionFormat.ADPCM;
                    importer.forceToMono = true;
                    break;
            }
            importer.defaultSampleSettings = s;
        }
    }
}
