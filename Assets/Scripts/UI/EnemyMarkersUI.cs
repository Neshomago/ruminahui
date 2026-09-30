// Implements: approved UI plan item 3 — small HP bar over each enemy, Condor's Eye mark icon, lock-on reticle.
// Screen-space markers from a small reused set (no per-enemy canvases).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ruminahui
{
    public class EnemyMarkersUI : UIElement
    {
        class Marker
        {
            public RectTransform root;
            public UIBar hp;
            public Text icon;
        }

        public float maxDistance = 30f;
        readonly List<Marker> markers = new List<Marker>();

        protected override void OnBuild() { }

        Marker Get(int i)
        {
            while (markers.Count <= i)
            {
                var root = UIFactory.Node(Root, "Marker" + markers.Count);
                root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
                root.sizeDelta = new Vector2(80f, 30f);
                var hp = UIFactory.Bar(root, "HP", UIFactory.HpColor, new Vector2(70f, 8f));
                UIFactory.Place(hp.Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(70f, 8f));
                var icon = UIFactory.Label(root, "Icon", "", 18, TextAnchor.LowerCenter, Color.white);
                UIFactory.Place(icon.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(80f, 24f));
                markers.Add(new Marker { root = root, hp = hp, icon = icon });
            }
            return markers[i];
        }

        void LateUpdate()
        {
            var cam = Camera.main;
            var c = Controlled;
            int used = 0;
            if (cam != null && c != null)
            {
                var locked = c.Targeting != null ? c.Targeting.Locked : null;
                foreach (var t in CombatTarget.All)
                {
                    if (t.faction != Faction.Enemy || !t.IsTargetable) continue;
                    if (Vector3.Distance(t.transform.position, c.transform.position) > maxDistance) continue;
                    var sp = cam.WorldToScreenPoint(t.transform.position + Vector3.up * (t.aimHeight + 1.1f));
                    if (sp.z <= 0f) continue;
                    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Root, sp, null, out var local)) continue;

                    var m = Get(used++);
                    m.root.gameObject.SetActive(true);
                    m.root.anchoredPosition = local;
                    // Decoys show a full bar like the real thing — reading them is the player's job (08 Phase 2).
                    m.hp.Set(t.isDecoy ? 1f : t.Health.Normalized);
                    string icon = "";
                    if (t.Status.IsMarked) icon += "◆";
                    if (t == locked) icon = "[ " + icon + " ]";
                    m.icon.text = icon;
                    m.icon.color = t.Status.IsMarked ? new Color(0.4f, 0.9f, 1f) : Color.white;
                }
            }
            for (int i = used; i < markers.Count; i++) markers[i].root.gameObject.SetActive(false);
        }
    }
}
