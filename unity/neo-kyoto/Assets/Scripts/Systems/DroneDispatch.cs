using System;
using System.Collections.Generic;

namespace NeoKyoto.Systems
{
    /// <summary>
    /// Sector 14 — the C3 target. Two fault types mean one blind fix no longer
    /// works: the player must check first, then branch.
    /// </summary>
    public class DroneDispatch
    {
        private static readonly string[][] DroneDefs =
        {
            new[] { "D-01", "MISROUTED" },
            new[] { "D-02", "GROUNDED" },
            new[] { "D-03", "MISROUTED" },
            new[] { "D-04", "MISROUTED" },
            new[] { "D-05", "GROUNDED" },
            new[] { "D-06", "MISROUTED" },
            new[] { "D-07", "GROUNDED" },
            new[] { "D-08", "MISROUTED" },
        };

        public readonly List<string> DroneIds = new List<string>();
        public readonly Dictionary<string, string> Drones = new Dictionary<string, string>();

        public string Current { get; private set; }
        private int _pointer;

        public Action<string> Output;
        public event Action Changed;

        public DroneDispatch()
        {
            foreach (var def in DroneDefs)
            {
                DroneIds.Add(def[0]);
                Drones[def[0]] = def[1];
            }
            Current = null;
            _pointer = 0;
        }

        private void Print(string s) { if (Output != null) Output(s); }

        public string CheckNext()
        {
            for (int i = 0; i < DroneIds.Count; i++)
            {
                int idx = (_pointer + i) % DroneIds.Count;
                string id = DroneIds[idx];
                string state = Drones[id];
                if (state == "MISROUTED" || state == "GROUNDED")
                {
                    Current = id;
                    _pointer = (idx + 1) % DroneIds.Count;
                    Print("    " + id + ": " + state);
                    if (Changed != null) Changed();
                    return state;
                }
            }
            Print("    All drones operational.");
            Current = null;
            if (Changed != null) Changed();
            return "DONE";
        }

        public object Reroute()
        {
            if (Current == null)
            {
                Print("    No drone selected. Use check_next() first.");
                return null;
            }
            string state = Drones[Current];
            if (state == "MISROUTED")
            {
                Drones[Current] = "OPERATIONAL";
                Print("    " + Current + ": rerouted → OPERATIONAL");
            }
            else if (state == "GROUNDED")
            {
                Print("    " + Current + ": not misrouted — reroute won't help.");
                Print("    This drone is GROUNDED. Try repair().");
            }
            else
            {
                Print("    " + Current + ": already operational.");
            }
            if (Changed != null) Changed();
            return null;
        }

        public object Repair()
        {
            if (Current == null)
            {
                Print("    No drone selected. Use check_next() first.");
                return null;
            }
            string state = Drones[Current];
            if (state == "GROUNDED")
            {
                Drones[Current] = "OPERATIONAL";
                Print("    " + Current + ": repaired → OPERATIONAL");
            }
            else if (state == "MISROUTED")
            {
                Print("    " + Current + ": not grounded — repair won't help.");
                Print("    This drone is MISROUTED. Try reroute().");
            }
            else
            {
                Print("    " + Current + ": already operational.");
            }
            if (Changed != null) Changed();
            return null;
        }

        public bool IsGoalMet()
        {
            foreach (var kv in Drones) if (kv.Value != "OPERATIONAL") return false;
            return true;
        }

        public string GetStatusText()
        {
            int operational = 0;
            foreach (var kv in Drones) if (kv.Value == "OPERATIONAL") operational++;
            string indicator = operational == Drones.Count ? "[OK]" : "[!!]";
            return "  SECTOR 14 — DRONE DISPATCH\n" +
                   "  Status:     " + indicator + " " + operational + "/" + Drones.Count + " drones operational\n";
        }
    }
}
