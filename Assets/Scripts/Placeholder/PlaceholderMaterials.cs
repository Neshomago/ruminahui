// Implements: AI_BUILD_PROMPT.md "placeholder visuals systematically" + 11-camera-performance.md Part B Step 1 (URP).
// One shared URP Lit material; per-object colour goes through a MaterialPropertyBlock.
using UnityEngine;

namespace Ruminahui
{
    public static class PlaceholderMaterials
    {
        public const string ResourcePath = "Placeholder/PlaceholderLit";
        static Material lit;
        static MaterialPropertyBlock block;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        public static Material Lit
        {
            get
            {
                if (lit != null) return lit;
                lit = Resources.Load<Material>(ResourcePath);
                if (lit == null)
                {
                    // Fallback when the setup tool hasn't created the asset yet (editor only — builds need the asset).
                    var shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null) shader = Shader.Find("Standard");
                    lit = new Material(shader) { name = "PlaceholderLit (runtime)", enableInstancing = true };
                }
                return lit;
            }
        }

        public static void Apply(Renderer r, Color c)
        {
            if (r == null) return;
            r.sharedMaterial = Lit;
            SetColor(r, c);
        }

        public static void SetColor(Renderer r, Color c)
        {
            if (block == null) block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            block.SetColor(BaseColorId, c);
            block.SetColor(ColorId, c);
            r.SetPropertyBlock(block);
        }
    }
}
