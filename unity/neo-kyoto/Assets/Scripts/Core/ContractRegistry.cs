using System;
using System.Collections.Generic;
using NeoKyoto.Contracts;

namespace NeoKyoto.Core
{
    public class ContractDef
    {
        public string Id;
        public string Title;
        public string Location;

        /// <summary>Index into GameState.UnlockSequence, or -1 when this contract unlocks nothing new.</summary>
        public int UnlockIndex;

        /// <summary>
        /// Contract ids that must be completed first, for ordering jobs *within* one
        /// district. Null means available as soon as the district opens — which is every
        /// contract today, since each district holds exactly one. Gating between places
        /// is `District.Requires`, not this.
        /// </summary>
        public string[] Requires;

        public Func<Contract> Create;
    }

    /// <summary>
    /// Board order for the demo: Python fundamentals (C1-C4) then the first
    /// terminal job (C5).
    ///
    /// Which district each one sits in — and therefore what unlocks what — lives in
    /// `DistrictRegistry`. The order here is presentation only.
    /// </summary>
    public static class ContractRegistry
    {
        public static readonly List<ContractDef> All = new List<ContractDef>
        {
            new ContractDef {
                Id = "contract_01", Title = "Keep the Lights On", Location = "Block 7",
                UnlockIndex = 0, Create = () => new Contract01()
            },
            new ContractDef {
                Id = "contract_02", Title = "Drone Route Cleanup", Location = "Sector 12",
                UnlockIndex = 1, Create = () => new Contract02()
            },
            new ContractDef {
                Id = "contract_03", Title = "Drone Dispatch", Location = "Sector 14",
                UnlockIndex = -1, Create = () => new Contract03()
            },
            new ContractDef {
                Id = "contract_04", Title = "Signal Interference", Location = "Transit Hub",
                UnlockIndex = -1, Create = () => new Contract04()
            },
            new ContractDef {
                Id = "contract_05", Title = "System Recovery", Location = "Data Center",
                UnlockIndex = -1, Create = () => new Contract05()
            },
        };

        public static List<string> AllIds()
        {
            var ids = new List<string>();
            foreach (var d in All) ids.Add(d.Id);
            return ids;
        }

        /// <summary>Highest star total the board can reach, for rank progress.</summary>
        public static int MaxTotalStars { get { return All.Count * Scoring.MaxStars; } }

        private static Dictionary<string, int> _baseCredits;

        /// <summary>
        /// Payout rate for a contract. It lives on the Contract subclass rather than
        /// the def, so the board instantiates each one once and keeps only the number
        /// — duplicating the rate here would let the two drift apart.
        /// </summary>
        public static int BaseCreditsFor(string contractId)
        {
            if (_baseCredits == null)
            {
                _baseCredits = new Dictionary<string, int>();
                foreach (var d in All) _baseCredits[d.Id] = d.Create().BaseCredits;
            }
            int credits;
            return _baseCredits.TryGetValue(contractId, out credits) ? credits : 0;
        }

        public static int IndexOf(string contractId)
        {
            for (int i = 0; i < All.Count; i++) if (All[i].Id == contractId) return i;
            return -1;
        }
    }
}
