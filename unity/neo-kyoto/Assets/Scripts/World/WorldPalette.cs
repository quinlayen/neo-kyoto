using UnityEngine;

namespace NeoKyoto.World
{
    /// <summary>Shared look for the city and its status lights.</summary>
    public static class WorldPalette
    {
        public static readonly Color Fault = new Color(0.95f, 0.25f, 0.20f);
        public static readonly Color Warning = new Color(1.00f, 0.68f, 0.15f);
        public static readonly Color Scrambled = new Color(0.85f, 0.30f, 0.90f);
        public static readonly Color Good = new Color(0.20f, 0.95f, 0.55f);
        public static readonly Color Cyan = new Color(0.25f, 0.85f, 1.00f);
        public static readonly Color Inert = new Color(0.35f, 0.38f, 0.45f);

        public static readonly Color Ground = new Color(0.06f, 0.07f, 0.10f);
        public static readonly Color Building = new Color(0.11f, 0.13f, 0.18f);
        public static readonly Color Structure = new Color(0.20f, 0.22f, 0.28f);

        private static Material _template;

        /// <summary>
        /// Every world material is cloned from an asset under Resources rather than
        /// built with Shader.Find. Shaders that no asset references are stripped from
        /// builds, so Shader.Find returns null at runtime and the whole world renders
        /// as nothing — which is exactly what happened in the first WebGL build.
        /// The template also ships with emission enabled so that variant survives too.
        /// </summary>
        public static Material Template
        {
            get
            {
                if (_template == null) _template = Resources.Load<Material>("WorldLit");
                return _template;
            }
        }

        public static Material MakeMaterial(Color color, Color? emission = null)
        {
            Material mat;
            if (Template != null)
            {
                mat = new Material(Template);
            }
            else
            {
                // Editor-only safety net; in a player this path means the asset is missing.
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader == null)
                {
                    Debug.LogError("No lit shader available — world will not render.");
                    return null;
                }
                mat = new Material(shader);
            }

            SetBaseColor(mat, color);
            SetEmission(mat, emission ?? Color.black);
            return mat;
        }

        public static void SetEmission(Material mat, Color emission)
        {
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
        }

        public static void SetBaseColor(Material mat, Color color)
        {
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        }
    }
}
