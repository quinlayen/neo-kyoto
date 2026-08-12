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

        public Func<Contract> Create;
    }

    /// <summary>
    /// Board order for the demo: Python fundamentals (C1-C4) then the first
    /// terminal job (C5).
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

        public static int IndexOf(string contractId)
        {
            for (int i = 0; i < All.Count; i++) if (All[i].Id == contractId) return i;
            return -1;
        }
    }
}
