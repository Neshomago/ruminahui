// Implements: approved UI plan item 9 — pooled damage pop-ups (11-camera-performance.md Part B Step 2: UI pop-ups are pooled).
// Suppressed for the spare finisher (08 Section 2: "no damage number").
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class DamageNumbers : UIElement
    {
        public const string PoolKey = "UI/DamageNumber";
        static DamageNumbers instance;
        public static bool Enabled = true;

        protected override void OnBuild()
        {
            instance = this;
            if (PoolManager.Instance == null) return;
            PoolManager.Instance.Register(PoolKey, () =>
            {
                var t = UIFactory.Label(Root, "DamageNumber", "", 24, TextAnchor.MiddleCenter, Color.white);
                t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                t.rectTransform.sizeDelta = new Vector2(120f, 40f);
                t.gameObject.AddComponent<Outline>().effectColor = Color.black;
                var po = t.gameObject.AddComponent<PooledObject>();
                po.ReturnToPoolRoot = false; // stays under the canvas
                t.gameObject.AddComponent<DamageNumberItem>();
                return t.gameObject;
            }, 8);
        }

        void OnDestroy() { if (instance == this) instance = null; }

        public static void Show(Vector3 worldPos, float amount, Color color)
        {
            if (!Enabled || instance == null || PoolManager.Instance == null) return;
            var go = PoolManager.Instance.Spawn(PoolKey, Vector3.zero, Quaternion.identity);
            if (go == null) return;
            go.GetComponent<DamageNumberItem>().Init(instance.Root, worldPos, Mathf.CeilToInt(amount).ToString(), color);
        }
    }
}
