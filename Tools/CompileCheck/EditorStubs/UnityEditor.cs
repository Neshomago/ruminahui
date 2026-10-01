// STUB — compile-check only. UnityEditor, URP and Addressables-editor members used by Assets/Scripts/Editor.
#pragma warning disable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MenuItem : Attribute
    {
        public MenuItem(string itemName) { }
        public MenuItem(string itemName, bool isValidateFunction) { }
        public int priority { get; set; }
    }

    public static class AssetDatabase
    {
        public static void CreateAsset(UnityEngine.Object asset, string path) { }
        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object => null;
        public static bool IsValidFolder(string path) => true;
        public static string CreateFolder(string parentFolder, string newFolderName) => "";
        public static string AssetPathToGUID(string path) => "";
        public static void SaveAssets() { }
    }

    public static class EditorUtility
    {
        public static bool DisplayDialog(string title, string message, string ok, string cancel) => true;
        public static void SetDirty(UnityEngine.Object target) { }
    }

    public static class PrefabUtility
    {
        public static GameObject SaveAsPrefabAsset(GameObject instanceRoot, string assetPath) => null;
    }

    public class EditorBuildSettingsScene
    {
        public EditorBuildSettingsScene(string path, bool enabled) { }
    }

    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes { get; set; }
    }
}

namespace UnityEditor.SceneManagement
{
    using UnityEngine.SceneManagement;
    public enum NewSceneSetup { EmptyScene, DefaultGameObjects }
    public enum NewSceneMode { Single, Additive }
    public enum OpenSceneMode { Single, Additive, AdditiveWithoutLoading }

    public static class EditorSceneManager
    {
        public static Scene NewScene(NewSceneSetup setup, NewSceneMode mode = NewSceneMode.Single) => default;
        public static bool SaveScene(Scene scene, string dstScenePath = "", bool saveAsCopy = false) => true;
        public static Scene OpenScene(string scenePath, OpenSceneMode mode = OpenSceneMode.Single) => default;
        public static bool SaveCurrentModifiedScenesIfUserWantsTo() => true;
    }
}

namespace UnityEditor.AddressableAssets.Settings
{
    public class AddressableAssetGroup : ScriptableObject { }
    public class AddressableAssetEntry { public string address { get; set; } }

    public class AddressableAssetSettings : ScriptableObject
    {
        public enum ModificationEvent { GroupAdded, GroupRemoved, EntryCreated, EntryAdded, EntryMoved, EntryRemoved, EntryModified, BatchModification }
        public AddressableAssetGroup DefaultGroup { get; set; }
        public AddressableAssetEntry CreateOrMoveEntry(string guid, AddressableAssetGroup targetParent, bool readOnly = false, bool postEvent = true) => null;
        public void SetDirty(ModificationEvent modificationEvent, object eventData, bool postEvent, bool settingsModified = false) { }
    }
}

namespace UnityEditor.AddressableAssets
{
    using UnityEditor.AddressableAssets.Settings;
    public static class AddressableAssetSettingsDefaultObject
    {
        public static AddressableAssetSettings GetSettings(bool create) => null;
    }
}




namespace UnityEditor
{
    public class AssetImporter : UnityEngine.Object { }
    public struct AudioImporterSampleSettings
    {
        public UnityEngine.AudioClipLoadType loadType;
        public UnityEngine.AudioCompressionFormat compressionFormat;
        public float quality;
    }
    public sealed class AudioImporter : AssetImporter
    {
        public AudioImporterSampleSettings defaultSampleSettings { get; set; }
        public bool forceToMono { get; set; }
    }
    public class AssetPostprocessor
    {
        public string assetPath { get; set; }
        public AssetImporter assetImporter => null;
    }
}

namespace UnityEngine
{
    public enum AudioClipLoadType { DecompressOnLoad, CompressedInMemory, Streaming }
    public enum AudioCompressionFormat { PCM, Vorbis, ADPCM, MP3, VAG, HEVAG, XMA, AAC, GCADPCM, ATRAC9 }
}
