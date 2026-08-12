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

        private static Shader _litShader;

        public static Shader LitShader
        {
            get
            {
                if (_litShader == null)
                {
                    _litShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (_litShader == null) _litShader = Shader.Find("Standard");
                }
                return _litShader;
            }
        }

        public static Material MakeMaterial(Color color, Color? emission = null)
        {
            var mat = new Material(LitShader);
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (emission.HasValue) SetEmission(mat, emission.Value);
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
