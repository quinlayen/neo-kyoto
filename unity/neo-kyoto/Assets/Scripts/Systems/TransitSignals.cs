using System;
using System.Collections.Generic;

namespace NeoKyoto.Systems
{
    /// <summary>
    /// Transit Hub — the C4 target. Signals are numbered, so commands take an
    /// argument, and the report can only be filed once the loop has finished.
    /// </summary>
    public class TransitSignals
    {
        private static readonly string[] Defaults =
        {
            "SCRAMBLED", "STUCK", "STUCK", "SCRAMBLED", "STUCK", "SCRAMBLED"
        };

        public readonly Dictionary<int, string> Signals = new Dictionary<int, string>();
        public bool ReportSubmitted { get; private set; }

        public Action<string> Output;
        public event Action Changed;

        public TransitSignals()
        {
            for (int i = 0; i < Defaults.Length; i++) Signals[i + 1] = Defaults[i];
            ReportSubmitted = false;
        }

        private void Print(string s) { if (Output != null) Output(s); }

        private bool Validate(int n)
        {
            if (!Signals.ContainsKey(n))
            {
                Print("    Error: no signal " + n + ". Valid signals are 1-6.");
                return false;
            }
            return true;
        }

        public string CheckSignal(int n)
        {
            if (!Validate(n)) return "UNKNOWN";
            string state = Signals[n];
            Print("    Signal " + n + ": " + state);
            return state;
        }

        public object ResetSignal(int n)
        {
            if (!Validate(n)) return null;
            string state = Signals[n];
            if (state == "STUCK")
            {
                Signals[n] = "FIXED";
                Print("    Signal " + n + ": reset applied → FIXED");
            }
            else if (state == "FIXED")
            {
                Print("    Signal " + n + ": already fixed.");
            }
            else
            {
                Print("    Signal " + n + ": not stuck — reset won't help.");
                Print("    Try calibrate_signal() for SCRAMBLED signals.");
            }
            if (Changed != null) Changed();
            return null;
        }

        public object CalibrateSignal(int n)
        {
            if (!Validate(n)) return null;
            string state = Signals[n];
            if (state == "SCRAMBLED")
            {
                Signals[n] = "FIXED";
                Print("    Signal " + n + ": calibrated → FIXED");
            }
            else if (state == "FIXED")
            {
                Print("    Signal " + n + ": already fixed.");
            }
            else
            {
                Print("    Signal " + n + ": not scrambled — calibration won't help.");
                Print("    Try reset_signal() for STUCK signals.");
            }
            if (Changed != null) Changed();
            return null;
        }

        public object SubmitReport()
        {
            if (!AllFixed())
            {
                Print("    Cannot submit: not all signals fixed yet.");
                return null;
            }
            ReportSubmitted = true;
            Print("    Report submitted. All signals verified and logged.");
            if (Changed != null) Changed();
            return null;
        }

        private bool AllFixed()
        {
            foreach (var kv in Signals) if (kv.Value != "FIXED") return false;
            return true;
        }

        public bool IsGoalMet() { return AllFixed() && ReportSubmitted; }

        public string GetStatusText()
        {
            int fixedCount = 0;
            foreach (var kv in Signals) if (kv.Value == "FIXED") fixedCount++;
            string report = ReportSubmitted ? "SUBMITTED" : "PENDING";
            string indicator = IsGoalMet() ? "[OK]" : "[!!]";
            return "  TRANSIT HUB — SIGNAL CONTROL\n" +
                   "  Status:     " + indicator + " " + fixedCount + "/" + Signals.Count + " signals fixed\n" +
                   "  Report:     " + report + "\n";
        }
    }
}
