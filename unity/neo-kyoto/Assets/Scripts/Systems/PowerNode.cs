using System;
using System.Globalization;

namespace NeoKyoto.Systems
{
    /// <summary>Block 7's power node — the C1 target. Twelve rebalances stabilise it.</summary>
    public class PowerNode
    {
        public const int RequiredRebalances = 12;

        public string Status { get; private set; }
        public double Load { get; private set; }
        public int RebalanceCount { get; private set; }

        public Action<string> Output;

        /// <summary>Raised after every rebalance so the world view can react.</summary>
        public event Action Changed;

        public PowerNode()
        {
            Status = "FLICKERING";
            Load = 0.97;
            RebalanceCount = 0;
        }

        public string Rebalance()
        {
            RebalanceCount++;
            Load = Math.Max(0.4, Load - 0.05);

            if (RebalanceCount >= RequiredRebalances && Status != "STABLE")
                Status = "STABLE";

            string msg = "    Rebalance #" + RebalanceCount +
                         " — load " + Load.ToString("0.00", CultureInfo.InvariantCulture) +
                         " — Status: " + Status;
            if (Output != null) Output(msg);
            if (Changed != null) Changed();
            return msg;
        }

        public bool IsGoalMet() { return Status == "STABLE"; }

        /// <summary>0 when flickering at full load, 1 when stable.</summary>
        public float StabilityFraction
        {
            get { return Mathf01((float)RebalanceCount / RequiredRebalances); }
        }

        private static float Mathf01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }

        public string GetStatusText()
        {
            string indicator = Status == "FLICKERING" ? "[!!]" : (Status == "STABLE" ? "[OK]" : "[??]");
            return "  BLOCK 7 POWER NODE\n" +
                   "  Status:     " + indicator + " " + Status + "\n" +
                   "  Load:       " + Load.ToString("0.00", CultureInfo.InvariantCulture) + "\n" +
                   "  Rebalances: " + RebalanceCount + "\n";
        }
    }
}
