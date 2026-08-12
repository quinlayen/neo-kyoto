using UnityEngine;

namespace NeoKyoto.World
{
    /// <summary>Primitive helpers so the whole city can be generated from code.</summary>
    public static class WorldBuilder
    {
        public static GameObject Primitive(PrimitiveType type, Transform parent, Vector3 pos,
                                           Vector3 scale, Material mat, string name = null)
        {
            var go = GameObject.CreatePrimitive(type);
            if (name != null) go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;

            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        /// <summary>
        /// A low-poly skyline for depth. Seeded so the city looks the same every run.
        /// </summary>
        public static void BuildSkyline(Transform parent, int seed = 20189)
        {
            var rng = new System.Random(seed);
            var mat = WorldPalette.MakeMaterial(WorldPalette.Building);
            var litMat = WorldPalette.MakeMaterial(WorldPalette.Building, new Color(0.05f, 0.12f, 0.18f));

            for (int i = 0; i < 46; i++)
            {
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float dist = 26f + (float)rng.NextDouble() * 40f;
                float h = 4f + (float)rng.NextDouble() * 26f;
                float w = 2.5f + (float)rng.NextDouble() * 5f;
                float d = 2.5f + (float)rng.NextDouble() * 5f;

                var pos = new Vector3(Mathf.Cos(angle) * dist, h * 0.5f, Mathf.Sin(angle) * dist);
                Primitive(PrimitiveType.Cube, parent, pos, new Vector3(w, h, d),
                    rng.NextDouble() > 0.6 ? litMat : mat, "Building");
            }
        }

        public static GameObject Ground(Transform parent)
        {
            var mat = WorldPalette.MakeMaterial(WorldPalette.Ground);
            return Primitive(PrimitiveType.Cube, parent, new Vector3(0, -0.5f, 0),
                new Vector3(200f, 1f, 200f), mat, "Ground");
        }

        /// <summary>A glowing indicator whose emission can be driven at runtime.</summary>
        public static Renderer Lamp(Transform parent, Vector3 pos, float size, Color color)
        {
            var mat = WorldPalette.MakeMaterial(color * 0.5f, color);
            var go = Primitive(PrimitiveType.Sphere, parent, pos, Vector3.one * size, mat, "Lamp");
            return go.GetComponent<Renderer>();
        }
    }
}
