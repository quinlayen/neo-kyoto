using System;
using System.Collections.Generic;

namespace NeoKyoto.Systems
{
    /// <summary>Sector 12's drone grid — the C2 target. Eight drones, all misrouted.</summary>
    public class DroneRouter
    {
        public class Drone
        {
            public string Id;
            public string Priority;
            public string Status;
        }

        public readonly List<Drone> Drones = new List<Drone>();

        public Action<string> Output;
        public event Action Changed;

        public DroneRouter()
        {
            Add("D-01", "CRITICAL");
            Add("D-02", "STANDARD");
            Add("D-03", "CRITICAL");
            Add("D-04", "LOW");
            Add("D-05", "STANDARD");
            Add("D-06", "CRITICAL");
            Add("D-07", "LOW");
            Add("D-08", "STANDARD");
        }

        private void Add(string id, string priority)
        {
            Drones.Add(new Drone { Id = id, Priority = priority, Status = "MISROUTED" });
        }

        private void Print(string s) { if (Output != null) Output(s); }

        public long ScanDrones()
        {
            int remaining = 0;
            foreach (var d in Drones) if (d.Status == "MISROUTED") remaining++;

            Print("  ┌─────────┬───────────┬────────────┐");
            Print("  │ Drone   │ Priority  │ Status     │");
            Print("  ├─────────┼───────────┼────────────┤");
            foreach (var d in Drones)
                Print("  │ " + d.Id.PadRight(7) + " │ " + d.Priority.PadRight(9) + " │ " + d.Status.PadRight(10) + " │");
            Print("  └─────────┴───────────┴────────────┘");
            Print("    " + remaining + " drones still misrouted.");
            return remaining;
        }

        public object RerouteNext()
        {
            foreach (var d in Drones)
            {
                if (d.Status == "MISROUTED")
                {
                    d.Status = "CORRECTED";
                    Print("    Drone " + d.Id + " (" + d.Priority + ") rerouted → CORRECTED");
                    if (Changed != null) Changed();
                    return d.Id;
                }
            }
            Print("    All drones already corrected.");
            return null;
        }

        public bool IsGoalMet()
        {
            foreach (var d in Drones) if (d.Status != "CORRECTED") return false;
            return true;
        }

        public string GetStatusText()
        {
            int corrected = 0;
            foreach (var d in Drones) if (d.Status == "CORRECTED") corrected++;
            string indicator = corrected == Drones.Count ? "[OK]" : "[!!]";
            return "  SECTOR 12 DRONE GRID\n" +
                   "  Status:     " + indicator + " " + corrected + "/" + Drones.Count + " drones corrected\n";
        }
    }
}
