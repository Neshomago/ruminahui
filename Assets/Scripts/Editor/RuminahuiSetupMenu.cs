// Implements: 11-camera-performance.md Part B Step 1 (URP, set up BEFORE content) + Step 9 (two quality tiers: Standard /
// Performance) + Step 3 (every mission scene marked Addressable, additive streaming) + AI_BUILD_PROMPT.md PROJECT STRUCTURE
// (one scene per mission named by Mission List ID; /Prefabs/Placeholder).
// Menu: Rumiñahui ▸ 1. Configure Project · 2. Build Scenes · 3. Save Placeholder Prefabs.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Ruminahui.EditorTools
{
    public static class RuminahuiSetupMenu
    {
        const string SettingsFolder = "Assets/Settings";
        const string ScenesFolder = "Assets/Scenes";
        const string TestScenesFolder = "Assets/Scenes/Tests";
        const string PrefabFolder = "Assets/Prefabs/Placeholder";
        const string MaterialPath = "Assets/Resources/Placeholder/PlaceholderLit.mat";
        const string RegistryPath = "Assets/Resources/PrefabRegistry.asset";

        // ───────────────────────── 1. Configure project ─────────────────────────
        [MenuItem("Rumiñahui/1. Configure Project (URP, material, registry)", priority = 1)]
        public static void ConfigureProject()
        {
            EnsureFolder(SettingsFolder);
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Placeholder");
            ConfigureURP();
            CreatePlaceholderMaterial();
            GetOrCreateRegistry();
            AddressableAssetSettingsDefaultObject.GetSettings(true);
            AssetDatabase.SaveAssets();
            Debug.Log("[Rumiñahui] Project configured. Next: Rumiñahui ▸ 2. Build Scenes.");
        }

        static void ConfigureURP()
        {
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset)
            {
                Debug.Log("[Rumiñahui] URP already active — leaving render pipeline settings alone.");
                return;
            }

            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, SettingsFolder + "/URP_Renderer.asset");

            var standard = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(standard, SettingsFolder + "/URP_Standard.asset");

            // Performance tier: heavier features trimmed (Step 9).
            var performance = UniversalRenderPipelineAsset.Create(rendererData);
            performance.renderScale = 0.8f;
            performance.supportsHDR = false;
            performance.shadowDistance = 30f;
            AssetDatabase.CreateAsset(performance, SettingsFolder + "/URP_Performance.asset");

            GraphicsSettings.defaultRenderPipeline = standard;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = i == 0 ? performance : standard;
            }
            QualitySettings.SetQualityLevel(current, false);
            Debug.Log("[Rumiñahui] URP assets created (Standard + Performance) and assigned to Graphics/Quality settings.");
        }

        static void CreatePlaceholderMaterial()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(MaterialPath) != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("[Rumiñahui] URP Lit shader not found — is com.unity.render-pipelines.universal installed?");
                return;
            }
            var mat = new Material(shader) { name = "PlaceholderLit", enableInstancing = true };
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        static PrefabRegistry GetOrCreateRegistry()
        {
            var reg = AssetDatabase.LoadAssetAtPath<PrefabRegistry>(RegistryPath);
            if (reg != null) return reg;
            reg = ScriptableObject.CreateInstance<PrefabRegistry>();
            reg.usePrefabs = false; // becomes true once step 3 saves prefabs
            AssetDatabase.CreateAsset(reg, RegistryPath);
            return reg;
        }

        // ───────────────────────── 2. Build scenes ─────────────────────────
        [MenuItem("Rumiñahui/2. Build Scenes (missing only)", priority = 2)]
        public static void BuildScenesMissing() => BuildScenes(false);

        [MenuItem("Rumiñahui/2b. Rebuild ALL Scenes (overwrite)", priority = 3)]
        public static void BuildScenesOverwrite()
        {
            if (!EditorUtility.DisplayDialog("Rebuild all scenes?",
                    "This overwrites every scene in Assets/Scenes with a fresh placeholder (SceneSetup + light). Content is built from code, but any manual edits to these scene files will be lost.",
                    "Overwrite", "Cancel")) return;
            BuildScenes(true);
        }

        static void BuildScenes(bool overwrite)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureFolder(ScenesFolder);
            EnsureFolder(TestScenesFolder);

            var scenes = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(MissionDatabase.BootScene, $"{ScenesFolder}/{MissionDatabase.BootScene}.unity"),
            };
            foreach (var m in MissionDatabase.Missions) scenes.Add(new KeyValuePair<string, string>(m.Id, $"{ScenesFolder}/{m.Id}.unity"));
            foreach (var t in MissionDatabase.TestScenes) scenes.Add(new KeyValuePair<string, string>(t, $"{TestScenesFolder}/{t}.unity"));

            int created = 0;
            foreach (var kv in scenes)
            {
                if (!overwrite && File.Exists(kv.Value)) continue;
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                var light = new GameObject("Directional Light");
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                var l = light.AddComponent<Light>();
                l.type = LightType.Directional;
                l.intensity = 1.1f;
                l.shadows = LightShadows.Soft;

                var setup = new GameObject("SceneSetup");
                setup.AddComponent<SceneSetup>().contentId = kv.Key;

                EditorSceneManager.SaveScene(scene, kv.Value);
                created++;
            }

            // Build Settings (fallback path) — boot first.
            var buildList = new List<EditorBuildSettingsScene>();
            foreach (var kv in scenes) buildList.Add(new EditorBuildSettingsScene(kv.Value, true));
            EditorBuildSettings.scenes = buildList.ToArray();

            // Addressables: address = content id (e.g. "M3.4").
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            foreach (var kv in scenes)
            {
                var guid = AssetDatabase.AssetPathToGUID(kv.Value);
                if (string.IsNullOrEmpty(guid)) continue;
                var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                entry.address = kv.Key;
            }
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true, true);
            AssetDatabase.SaveAssets();

            EditorSceneManager.OpenScene($"{ScenesFolder}/{MissionDatabase.BootScene}.unity");
            Debug.Log($"[Rumiñahui] Scenes ready ({created} created). {scenes.Count} scenes in Build Settings and Addressables. Open any scene and press Play.");
        }

        // ───────────────────────── 3. Prefabs ─────────────────────────
        [MenuItem("Rumiñahui/3. Save Placeholder Prefabs", priority = 4)]
        public static void SavePrefabs()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(PrefabFolder);
            var reg = GetOrCreateRegistry();

            foreach (CharacterId id in System.Enum.GetValues(typeof(CharacterId)))
            {
                var go = CharacterFactory.Build(id, forPrefab: true);
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabFolder}/Character_{id}.prefab");
                Object.DestroyImmediate(go);
                reg.Set("Character/" + id, prefab);
            }
            foreach (EnemyType t in System.Enum.GetValues(typeof(EnemyType)))
            {
                var go = EnemyFactory.Build(t);
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabFolder}/Enemy_{t}.prefab");
                Object.DestroyImmediate(go);
                reg.Set(EnemyFactory.PoolKey(t), prefab);
            }
            reg.usePrefabs = true;
            EditorUtility.SetDirty(reg);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Rumiñahui] Placeholder prefabs saved to {PrefabFolder} and registered. Swap a prefab's PlaceholderVisual.modelPrefab to drop in real art. Untick PrefabRegistry.usePrefabs to go back to code-built.");
        }

        [MenuItem("Rumiñahui/Open Boot Scene", priority = 20)]
        public static void OpenBoot()
        {
            var path = $"{ScenesFolder}/{MissionDatabase.BootScene}.unity";
            if (!File.Exists(path)) { Debug.LogWarning("[Rumiñahui] Run 2. Build Scenes first."); return; }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(path);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
