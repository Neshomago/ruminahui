// Implements: approved UI plan item 9 — one pooled floating damage number.
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class DamageNumberItem : MonoBehaviour, IPoolable
    {
        public float lifetime = 0.8f;
        Text text;
        RectTransform rt;
        RectTransform canvasRoot;
        Vector3 world;
        float age;

        void Awake()
        {
            text = GetComponent<Text>();
            rt = (RectTransform)transform;
        }

        public void Init(RectTransform root, Vector3 worldPos, string value, Color color)
        {
            canvasRoot = root;
            world = worldPos + Random.insideUnitSphere * 0.3f;
            text.text = value;
            text.color = color;
            age = 0f;
            Place();
        }

        public void OnSpawned() => age = 0f;
        public void OnDespawned() { }

        void Update()
        {
            age += Time.deltaTime;
            world += Vector3.up * Time.deltaTime * 1.2f;
            var c = text.color;
            c.a = 1f - Mathf.Clamp01(age / lifetime);
            text.color = c;
            Place();
            if (age >= lifetime) PoolManager.Instance.Despawn(gameObject);
        }

        void Place()
        {
            var cam = Camera.main;
            if (cam == null || canvasRoot == null) return;
            var sp = cam.WorldToScreenPoint(world);
            if (sp.z <= 0f) { rt.anchoredPosition = new Vector2(99999f, 0f); return; }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, sp, null, out var local)) rt.anchoredPosition = local;
        }
    }
}
