using System.Collections.Generic;
using NeoKyoto.Contracts;
using UnityEngine;

namespace NeoKyoto.World
{
    /// <summary>Block 7 — a power pylon feeding a cluster of homes.</summary>
    public class PowerNodeSite : ContractSiteView
    {
        private Renderer _coreLamp;
        private readonly List<Renderer> _windows = new List<Renderer>();
        private Light _light;

        public override Vector3 FocusPoint { get { return new Vector3(0f, 3.5f, 0f); } }
        public override float FocusDistance { get { return 20f; } }

        protected override void Build()
        {
            var structure = WorldPalette.MakeMaterial(WorldPalette.Structure);

            WorldBuilder.Primitive(PrimitiveType.Cylinder, transform, new Vector3(0, 0.3f, 0),
                new Vector3(3.2f, 0.3f, 3.2f), structure, "Base");
            WorldBuilder.Primitive(PrimitiveType.Cube, transform, new Vector3(0, 3f, 0),
                new Vector3(1.2f, 6f, 1.2f), structure, "Tower");

            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                WorldBuilder.Primitive(PrimitiveType.Cube, transform,
                    new Vector3(Mathf.Cos(a) * 1.6f, 5.2f, Mathf.Sin(a) * 1.6f),
                    new Vector3(0.25f, 0.25f, 2.4f), structure, "Strut")
                    .transform.localRotation = Quaternion.Euler(0, -a * Mathf.Rad2Deg, 0);
            }

            _coreLamp = WorldBuilder.Lamp(transform, new Vector3(0, 6.6f, 0), 1.5f, WorldPalette.Fault);

            var lightGo = new GameObject("NodeLight");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0, 6.6f, 0);
            _light = lightGo.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.range = 30f;
            _light.intensity = 2f;

            // Homes that light up as the node stabilises.
            var buildingMat = WorldPalette.MakeMaterial(WorldPalette.Building);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f;
                var pos = new Vector3(Mathf.Cos(a) * 9f, 2f, Mathf.Sin(a) * 9f);
                WorldBuilder.Primitive(PrimitiveType.Cube, transform, pos,
                    new Vector3(3f, 4f, 3f), buildingMat, "Home");
                var w = WorldBuilder.Lamp(transform,
                    pos + new Vector3(0, 0.4f, 0) + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * -1.55f,
                    0.9f, WorldPalette.Warning);
                w.transform.localScale = new Vector3(1.6f, 1.6f, 0.15f);
                w.transform.localRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)));
                _windows.Add(w);
            }
        }

        public override void Refresh()
        {
            var c = Contract as Contract01;
            if (c == null || c.Node == null) return;

            float stability = c.Node.StabilityFraction;
            bool stable = c.Node.IsGoalMet();
            Color color = Color.Lerp(WorldPalette.Fault, WorldPalette.Good, stability);

            // Unstable power flickers; stable power holds steady.
            float intensity = stable ? 2.0f : Mathf.Lerp(0.25f, 1.5f, Pulse(14f) * (1f - stability * 0.6f));
            SetLamp(_coreLamp, color, intensity);

            if (_light != null)
            {
                _light.color = color;
                _light.intensity = stable ? 3.0f : Mathf.Lerp(0.5f, 2.2f, Pulse(14f));
            }

            for (int i = 0; i < _windows.Count; i++)
            {
                bool lit = stability > (i + 0.5f) / _windows.Count;
                float f = lit ? (stable ? 1.6f : 1.1f) : 0.04f;
                SetLamp(_windows[i], lit ? WorldPalette.Warning : WorldPalette.Inert, f);
            }
        }

        private void Update() { Refresh(); }
    }

    /// <summary>Sector 12 — eight cargo drones on a delivery lane.</summary>
    public class DroneRouterSite : ContractSiteView
    {
        private readonly List<Transform> _drones = new List<Transform>();
        private readonly List<Renderer> _lamps = new List<Renderer>();

        public override Vector3 FocusPoint { get { return new Vector3(0f, 2.5f, 0f); } }
        public override float FocusDistance { get { return 27f; } }
        public override float CameraYaw { get { return 12f; } }

        protected override void Build()
        {
            BuildLane(transform);
            var body = WorldPalette.MakeMaterial(WorldPalette.Structure);

            for (int i = 0; i < 8; i++)
            {
                var go = new GameObject("Drone" + (i + 1));
                go.transform.SetParent(transform, false);
                WorldBuilder.Primitive(PrimitiveType.Cube, go.transform, Vector3.zero,
                    new Vector3(0.9f, 0.35f, 0.9f), body, "Body");
                WorldBuilder.Primitive(PrimitiveType.Cube, go.transform, new Vector3(0, 0.2f, 0),
                    new Vector3(1.5f, 0.08f, 0.14f), body, "RotorBar");
                WorldBuilder.Primitive(PrimitiveType.Cube, go.transform, new Vector3(0, 0.2f, 0),
                    new Vector3(0.14f, 0.08f, 1.5f), body, "RotorBar2");
                _lamps.Add(WorldBuilder.Lamp(go.transform, new Vector3(0, -0.28f, 0), 0.34f, WorldPalette.Warning));
                _drones.Add(go.transform);
            }
        }

        internal static void BuildLane(Transform parent)
        {
            var laneMat = WorldPalette.MakeMaterial(new Color(0.10f, 0.16f, 0.22f), new Color(0.02f, 0.10f, 0.16f));
            for (int i = 0; i < 16; i++)
            {
                WorldBuilder.Primitive(PrimitiveType.Cube, parent,
                    new Vector3(-11.25f + i * 1.5f, 0.02f, 0f),
                    new Vector3(1.0f, 0.04f, 3.0f), laneMat, "LaneStripe");
            }
        }

        internal static Vector3 LanePosition(int index, int total)
        {
            float t = total <= 1 ? 0.5f : index / (float)(total - 1);
            return new Vector3(Mathf.Lerp(-9.5f, 9.5f, t), 2.4f, 0f);
        }

        public override void Refresh()
        {
            var c = Contract as Contract02;
            if (c == null || c.Router == null) return;

            for (int i = 0; i < _drones.Count && i < c.Router.Drones.Count; i++)
            {
                var drone = c.Router.Drones[i];
                bool corrected = drone.Status == "CORRECTED";
                Vector3 target = LanePosition(i, c.Router.Drones.Count);

                if (corrected)
                {
                    // On course: locked to the lane with a gentle hover.
                    target.y += Mathf.Sin(Time.time * 2f + i) * 0.12f;
                    _drones[i].localRotation = Quaternion.Slerp(_drones[i].localRotation,
                        Quaternion.identity, Time.deltaTime * 6f);
                    SetLamp(_lamps[i], WorldPalette.Cyan, 1.6f);
                }
                else
                {
                    // Misrouted: drifting off the lane and pitched over.
                    target += new Vector3(
                        Mathf.Sin(Time.time * 1.3f + i * 2.1f) * 1.6f,
                        Mathf.Sin(Time.time * 2.2f + i) * 0.7f,
                        Mathf.Cos(Time.time * 1.1f + i * 1.7f) * 3.4f);
                    _drones[i].localRotation = Quaternion.Euler(
                        Mathf.Sin(Time.time * 3f + i) * 22f, i * 40f, Mathf.Cos(Time.time * 2.5f + i) * 22f);
                    SetLamp(_lamps[i], WorldPalette.Warning, 0.7f + Pulse(9f) * 0.8f);
                }

                _drones[i].localPosition = Vector3.Lerp(_drones[i].localPosition, target, Time.deltaTime * 3.5f);
            }
        }

        private void Update() { Refresh(); }
    }

    /// <summary>Sector 14 — same fleet, two different faults.</summary>
    public class DroneDispatchSite : ContractSiteView
    {
        private readonly List<Transform> _drones = new List<Transform>();
        private readonly List<Renderer> _lamps = new List<Renderer>();

        public override Vector3 FocusPoint { get { return new Vector3(0f, 2.5f, 0f); } }
        public override float FocusDistance { get { return 27f; } }
        public override float CameraYaw { get { return 12f; } }

        protected override void Build()
        {
            DroneRouterSite.BuildLane(transform);
            var body = WorldPalette.MakeMaterial(WorldPalette.Structure);

            for (int i = 0; i < 8; i++)
            {
                var go = new GameObject("Drone" + (i + 1));
                go.transform.SetParent(transform, false);
                WorldBuilder.Primitive(PrimitiveType.Cube, go.transform, Vector3.zero,
                    new Vector3(0.9f, 0.35f, 0.9f), body, "Body");
                WorldBuilder.Primitive(PrimitiveType.Cube, go.transform, new Vector3(0, 0.2f, 0),
                    new Vector3(1.5f, 0.08f, 0.14f), body, "RotorBar");
                WorldBuilder.Primitive(PrimitiveType.Cube, go.transform, new Vector3(0, 0.2f, 0),
                    new Vector3(0.14f, 0.08f, 1.5f), body, "RotorBar2");
                _lamps.Add(WorldBuilder.Lamp(go.transform, new Vector3(0, -0.28f, 0), 0.34f, WorldPalette.Warning));
                _drones.Add(go.transform);
            }
        }

        public override void Refresh()
        {
            var c = Contract as Contract03;
            if (c == null || c.Dispatch == null) return;

            for (int i = 0; i < _drones.Count && i < c.Dispatch.DroneIds.Count; i++)
            {
                string id = c.Dispatch.DroneIds[i];
                string state = c.Dispatch.Drones[id];
                bool isCurrent = c.Dispatch.Current == id;

                Vector3 target = DroneRouterSite.LanePosition(i, c.Dispatch.DroneIds.Count);
                Color color;
                float intensity;

                if (state == "OPERATIONAL")
                {
                    target.y += Mathf.Sin(Time.time * 2f + i) * 0.12f;
                    _drones[i].localRotation = Quaternion.Slerp(_drones[i].localRotation,
                        Quaternion.identity, Time.deltaTime * 6f);
                    color = WorldPalette.Good;
                    intensity = 1.6f;
                }
                else if (state == "GROUNDED")
                {
                    // Hardware fault: it is on the deck, not flying.
                    target.y = 0.35f;
                    _drones[i].localRotation = Quaternion.Slerp(_drones[i].localRotation,
                        Quaternion.Euler(18f, i * 33f, -12f), Time.deltaTime * 4f);
                    color = WorldPalette.Fault;
                    intensity = 0.5f + Pulse(3f) * 0.5f;
                }
                else
                {
                    target += new Vector3(
                        Mathf.Sin(Time.time * 1.3f + i * 2.1f) * 1.6f,
                        Mathf.Sin(Time.time * 2.2f + i) * 0.7f,
                        Mathf.Cos(Time.time * 1.1f + i * 1.7f) * 3.4f);
                    _drones[i].localRotation = Quaternion.Euler(
                        Mathf.Sin(Time.time * 3f + i) * 22f, i * 40f, Mathf.Cos(Time.time * 2.5f + i) * 22f);
                    color = WorldPalette.Warning;
                    intensity = 0.7f + Pulse(9f) * 0.8f;
                }

                if (isCurrent) intensity += 1.2f + Pulse(10f) * 0.8f;

                SetLamp(_lamps[i], color, intensity);
                _drones[i].localPosition = Vector3.Lerp(_drones[i].localPosition, target, Time.deltaTime * 3.5f);
            }
        }

        private void Update() { Refresh(); }
    }

    /// <summary>Transit Hub — six numbered signal masts along the track.</summary>
    public class TransitSignalsSite : ContractSiteView
    {
        private readonly List<Renderer> _lamps = new List<Renderer>();
        private Renderer _reportBeacon;

        public override Vector3 FocusPoint { get { return new Vector3(0f, 3f, 0f); } }
        public override float FocusDistance { get { return 24f; } }

        protected override void Build()
        {
            var structure = WorldPalette.MakeMaterial(WorldPalette.Structure);
            var railMat = WorldPalette.MakeMaterial(new Color(0.14f, 0.15f, 0.20f));

            WorldBuilder.Primitive(PrimitiveType.Cube, transform, new Vector3(0, 0.15f, 1.6f),
                new Vector3(26f, 0.18f, 0.35f), railMat, "Rail");
            WorldBuilder.Primitive(PrimitiveType.Cube, transform, new Vector3(0, 0.15f, -1.6f),
                new Vector3(26f, 0.18f, 0.35f), railMat, "Rail2");

            for (int i = 0; i < 6; i++)
            {
                float x = Mathf.Lerp(-10f, 10f, i / 5f);
                WorldBuilder.Primitive(PrimitiveType.Cylinder, transform, new Vector3(x, 1.9f, 3.2f),
                    new Vector3(0.22f, 1.9f, 0.22f), structure, "Mast");
                WorldBuilder.Primitive(PrimitiveType.Cube, transform, new Vector3(x, 3.9f, 3.2f),
                    new Vector3(0.9f, 0.9f, 0.5f), structure, "SignalHead");
                _lamps.Add(WorldBuilder.Lamp(transform, new Vector3(x, 3.9f, 2.9f), 0.55f, WorldPalette.Fault));
            }

            WorldBuilder.Primitive(PrimitiveType.Cube, transform, new Vector3(0, 1.6f, -6f),
                new Vector3(5f, 3.2f, 3f), WorldPalette.MakeMaterial(WorldPalette.Building), "ControlHut");
            _reportBeacon = WorldBuilder.Lamp(transform, new Vector3(0, 3.6f, -6f), 0.7f, WorldPalette.Inert);
        }

        public override void Refresh()
        {
            var c = Contract as Contract04;
            if (c == null || c.Signals == null) return;

            for (int i = 0; i < _lamps.Count; i++)
            {
                string state = c.Signals.Signals.ContainsKey(i + 1) ? c.Signals.Signals[i + 1] : "STUCK";
                if (state == "FIXED") SetLamp(_lamps[i], WorldPalette.Good, 1.7f);
                else if (state == "SCRAMBLED") SetLamp(_lamps[i], WorldPalette.Scrambled, 0.3f + Pulse(16f) * 1.3f);
                else SetLamp(_lamps[i], WorldPalette.Fault, 1.1f);
            }

            SetLamp(_reportBeacon,
                c.Signals.ReportSubmitted ? WorldPalette.Cyan : WorldPalette.Inert,
                c.Signals.ReportSubmitted ? 1.9f : 0.12f);
        }

        private void Update() { Refresh(); }
    }

    /// <summary>Data Center — racks of servers, one of them crashed.</summary>
    public class DataCenterSite : ContractSiteView
    {
        private readonly List<Renderer> _rackLamps = new List<Renderer>();
        private Renderer _faultLamp;

        public override Vector3 FocusPoint { get { return new Vector3(0f, 2.2f, 0f); } }
        public override float FocusDistance { get { return 20f; } }

        protected override void Build()
        {
            var rackMat = WorldPalette.MakeMaterial(WorldPalette.Structure);
            var floorMat = WorldPalette.MakeMaterial(new Color(0.09f, 0.10f, 0.13f));

            WorldBuilder.Primitive(PrimitiveType.Cube, transform, new Vector3(0, 0.05f, 0),
                new Vector3(22f, 0.1f, 16f), floorMat, "Floor");

            for (int row = 0; row < 2; row++)
            {
                for (int i = 0; i < 6; i++)
                {
                    float x = Mathf.Lerp(-7.5f, 7.5f, i / 5f);
                    float z = row == 0 ? -3.5f : 3.5f;
                    WorldBuilder.Primitive(PrimitiveType.Cube, transform, new Vector3(x, 1.6f, z),
                        new Vector3(2.0f, 3.2f, 1.2f), rackMat, "Rack");

                    for (int led = 0; led < 4; led++)
                    {
                        var lamp = WorldBuilder.Lamp(transform,
                            new Vector3(x - 0.6f + led * 0.4f, 2.6f, z - 0.65f), 0.16f, WorldPalette.Cyan);
                        _rackLamps.Add(lamp);
                    }
                }
            }

            // The power-grid rack: front and centre, dark until the report is read.
            WorldBuilder.Primitive(PrimitiveType.Cube, transform, new Vector3(0, 1.8f, 0),
                new Vector3(2.4f, 3.6f, 1.4f), rackMat, "PowerGridRack");
            _faultLamp = WorldBuilder.Lamp(transform, new Vector3(0, 3.0f, -0.75f), 0.5f, WorldPalette.Fault);
        }

        public override void Refresh()
        {
            var c = Contract as Contract05;
            bool found = c != null && c.TargetFound;

            for (int i = 0; i < _rackLamps.Count; i++)
            {
                float blink = 0.5f + 0.5f * Mathf.Sin(Time.time * (2f + (i % 5) * 0.7f) + i);
                SetLamp(_rackLamps[i], WorldPalette.Cyan, 0.3f + blink * 0.7f);
            }

            SetLamp(_faultLamp,
                found ? WorldPalette.Good : WorldPalette.Fault,
                found ? 1.9f : 0.45f + Pulse(4f) * 1.1f);
        }

        private void Update() { Refresh(); }
    }
}
