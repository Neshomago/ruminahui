// Implements: approved UI plan (items 1-11) — creates the canvas, EventSystem (Input System UI module) and every element.
// SetCombatUIVisible hides combat HUD for cinematic beats (05 stage K: "no combat UI on screen"; 08 spare moment).
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Ruminahui
{
    public class UIRoot : Singleton<UIRoot>
    {
        public Canvas Canvas { get; private set; }
        public RectTransform HudLayer { get; private set; }
        public RectTransform OverlayLayer { get; private set; }
        CanvasGroup hudGroup;

        protected override void Awake()
        {
            base.Awake();
            var canvasGo = new GameObject("[UI]", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            Canvas = canvasGo.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            HudLayer = UIFactory.Stretch(UIFactory.Node(canvasGo.transform, "HUD"));
            hudGroup = HudLayer.gameObject.AddComponent<CanvasGroup>();
            hudGroup.interactable = false;
            hudGroup.blocksRaycasts = false;
            OverlayLayer = UIFactory.Stretch(UIFactory.Node(canvasGo.transform, "Overlay"));

            // HUD (hidden during cinematics)
            Add<EnemyMarkersUI>(HudLayer);
            Add<DamageNumbers>(HudLayer);
            Add<ResourceHUD>(HudLayer);
            Add<AbilityStripUI>(HudLayer);
            Add<CallInStripUI>(HudLayer);
            Add<StealthMeterUI>(HudLayer);
            Add<BossBarUI>(HudLayer);
            Add<ContextPromptUI>(HudLayer);
            Add<SwapSelectorUI>(HudLayer);
            // Overlays (always on top)
            Add<TestHintUI>(OverlayLayer);
            Add<ScreenFader>(OverlayLayer);
            // Above the fader: subtitles/prompts must read over cinematics and hard cuts to black (M5.6, M5.7 text card).
            Add<SubtitleUI>(OverlayLayer);
            Add<PromptUI>(OverlayLayer);
            Add<DebugPanel>(OverlayLayer);
            Add<PauseMenu>(OverlayLayer);
            Add<UpgradeMenuUI>(OverlayLayer);
            Add<MainMenuUI>(OverlayLayer);
            Add<SettingsPanel>(OverlayLayer);
        }

        void Start() => EnsureEventSystem();

        void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            es.transform.SetParent(transform, false);
        }

        T Add<T>(RectTransform layer) where T : UIElement
        {
            var rt = UIFactory.Stretch(UIFactory.Node(layer, typeof(T).Name));
            var el = rt.gameObject.AddComponent<T>();
            el.Build(rt);
            return el;
        }

        public void SetCombatUIVisible(bool visible) => hudGroup.alpha = visible ? 1f : 0f;
        public bool CombatUIVisible => hudGroup.alpha > 0.5f;
    }
}
