using NeoKyoto.Contracts;
using UnityEngine;

namespace NeoKyoto.World
{
    /// <summary>
    /// A physical site in the city that mirrors one contract's system. The world
    /// is the primary feedback channel, so every state change shows up here.
    /// </summary>
    public abstract class ContractSiteView : MonoBehaviour
    {
        protected Contract Contract;

        public void Bind(Contract contract)
        {
            Contract = contract;
            Build();
            Refresh();
        }

        /// <summary>Creates the site geometry once, when the contract opens.</summary>
        protected abstract void Build();

        /// <summary>Pushes current system state onto the geometry.</summary>
        public abstract void Refresh();

        /// <summary>Where the camera should look, from how far, and at what angle.</summary>
        public virtual Vector3 FocusPoint { get { return new Vector3(0f, 2f, 0f); } }
        public virtual float FocusDistance { get { return 18f; } }

        /// <summary>Yaw in degrees. Wide sites use a flatter angle so the row reads across.</summary>
        public virtual float CameraYaw { get { return 34f; } }

        protected static float Pulse(float speed = 6f)
        {
            return 0.5f + 0.5f * Mathf.Sin(Time.time * speed);
        }

        protected static void SetLamp(Renderer r, Color color, float intensity)
        {
            if (r == null) return;
            var mat = r.material;
            WorldPalette.SetBaseColor(mat, color * 0.45f);
            WorldPalette.SetEmission(mat, color * intensity);
        }
    }
}
