using System.Collections.Generic;
using UnityEngine;

namespace NeoKyoto.Core
{
    /// <summary>
    /// How a district reads on the map. Warm and unstable, or cool and steady —
    /// the same colour language the world already uses (`GDD.md` §5), so the map
    /// has nothing new to teach.
    /// </summary>
    public enum DistrictState { Locked, Failing, Stabilised, Mastered }

    /// <summary>
    /// Where the overmap camera sits to look at a district. An orbit around the
    /// district's anchor rather than a hand-placed transform, so a district can be
    /// moved and its shot follows it.
    /// </summary>
    [System.Serializable]
    public struct DistrictFraming
    {
        [Tooltip("Degrees down from horizontal. Shallow reads as a skyline, steep as a map.")]
        public float pitch;

        [Tooltip("Degrees around Y. The main lever for making two districts look like " +
                 "different places rather than the same blocks from the same side.")]
        public float yaw;

        [Tooltip("Metres from the aim point back to the camera.")]
        public float distance;

        [Tooltip("Where the camera aims, relative to the anchor. Y raises the aim past " +
                 "neighbouring towers. X and Z push the subject off-centre — which the work " +
                 "site needs, because the deck's windows own the middle of the frame and a " +
                 "centred subject is a subject behind a window (DECK_SPEC §2's protected " +
                 "focal region, in practice).")]
        public Vector3 aimOffset;

        public float fieldOfView;
    }

    /// <summary>
    /// A place in the city and the contracts that live there. See `OVERMAP.md`.
    /// </summary>
    public class District
    {
        public string Id;
        public string Name;

        /// <summary>
        /// Where this district actually is, in world space, in the city scene. Not a
        /// normalised map coordinate — the overmap flies a camera here rather than
        /// drawing a marker on a painting.
        /// </summary>
        public Vector3 Anchor;

        /// <summary>The overmap shot for this district, orbited around <see cref="Anchor"/>.</summary>
        public DistrictFraming MapFraming;

        /// <summary>
        /// Where the contract's own geometry stands, on a real street, and the kerbside shot
        /// of it. This is the spot the player plugs the deck into.
        ///
        /// Separate from <see cref="Anchor"/> on purpose: the anchor is a map pin and can sit
        /// over a rooftop, which several of them do. The work site has to be pavement you
        /// could kneel on.
        /// </summary>
        public Vector3 WorkSite;
        public DistrictFraming WorkFraming;

        /// <summary>
        /// Scale applied to the contract's geometry when it stands in the city. The sites
        /// were built for the placeholder world, where the camera sits 18 m out and nothing
        /// sets the scale; on a real pavement they are building-sized. 1 leaves them alone.
        /// </summary>
        public float WorkSiteScale = 1f;

        /// <summary>
        /// False until a district has had a street found for it. Those contracts still run —
        /// they fall back to the placeholder ground, which is also the only option on a clone
        /// with no asset kit.
        /// </summary>
        public bool HasWorkSite { get { return WorkFraming.distance > 0f; } }

        /// <summary>
        /// District ids that must be <b>completed</b> before this one opens.
        /// Completed, deliberately — never mastered. `ECONOMY.md` holds the line that
        /// nothing required to progress may sit behind quality, the same reason the
        /// curriculum is granted rather than sold.
        /// </summary>
        public string[] Requires;

        /// <summary>
        /// What Voss says when the player selects this district locked. `OVERMAP.md`:
        /// a silhouette that answers nothing generates curiosity and then punishes it,
        /// which is worse than not drawing it at all.
        /// </summary>
        public string LockedLine;

        /// <summary>Resolved from <see cref="ContractIds"/> once, at first access.</summary>
        public readonly List<ContractDef> Contracts = new List<ContractDef>();

        public string[] ContractIds;
    }

    /// <summary>
    /// The district model the overmap needs. Until the panorama exists this drives a
    /// grouped list on the board — deliberately, so the model is validated before the
    /// most expensive asset in the feature is committed to.
    ///
    /// The chain below reproduces the old linear `IsAvailable` exactly (one contract
    /// per district, each requiring the last), so introducing it changes no behaviour.
    /// What it adds is the ability to express Act 2: several districts open at once.
    /// </summary>
    public static class DistrictRegistry
    {
        private static readonly List<District> _all = new List<District>
        {
            // SW quadrant — 1,264 renderers, tallest 133 m. Dense mid-rise blocks with a
            // rooftop five-a-side pitch as its landmark. Reads residential, which is what
            // C1's briefing needs: four hundred units, a third of them without light.
            new District {
                Id = "block_7", Name = "Block 7",
                Anchor = new Vector3(-75f, 0f, -75f),
                MapFraming = new DistrictFraming {
                    pitch = 32f, yaw = 35f, distance = 225f, aimOffset = new Vector3(0f, 5f, 0f), fieldOfView = 50f },

                // A kerb on the open street east of the anchor — sidewalk at y=0, streetlight,
                // vending machines, a warning barrier. Found by raycasting for ground below
                // y=1 with nothing overhead for 40 m, because the anchor itself sits on top of
                // a slums-block roof at y=35 and the first spot tried was a dead-end courtyard.
                // The camera stands in the road looking back at the kerb.
                // The aim is pushed past the kerb toward the road, so the junction box sits
                // low and left of the deck's windows rather than behind them.
                WorkSite = new Vector3(-39f, 0f, -65f),
                WorkFraming = new DistrictFraming {
                    pitch = 10f, yaw = 352f, distance = 14f,
                    aimOffset = new Vector3(4f, 1.5f, 4f), fieldOfView = 50f },

                // The site geometry is 21 x 7 x 19 m — built for the placeholder world seen
                // from 18 m, which is a building on a pavement. Starting value 0.35: about
                // 7 m of kerbside cabinet. Test: it reads as equipment a person could plug
                // into, and stays clear of the window field.
                WorkSiteScale = 0.35f,

                ContractIds = new[] { "contract_01" },
            },

            // NE quadrant — 740 renderers, tallest only 64 m. Low-rise, with exposed
            // rooftop machinery, clustered tanks and cable runs. Reads utility/plant.
            // The steep pitch and raised aim clear the black tower crown on its east side.
            new District {
                Id = "sector_12", Name = "Sector 12",
                Anchor = new Vector3(75f, 0f, 75f),
                MapFraming = new DistrictFraming {
                    pitch = 48f, yaw = 215f, distance = 236f, aimOffset = new Vector3(0f, 25f, 0f), fieldOfView = 50f },
                ContractIds = new[] { "contract_02" },
                Requires = new[] { "block_7" },
                LockedLine = "Sector 12's queue is spoken for until Block 7 signs off. Finish there first.",
            },

            // Shares the NE quadrant with Sector 12 and is meant to — they are adjacent
            // sectors running the same drone fleet (C2 and C3 are both drone work).
            // ⚠ UNVERIFIED framing: the other four were set from screenshots, this one is
            // a starting value. Test: an observer distinguishes it from Sector 12 with the
            // labels hidden. If they can't, move the anchor before touching pitch or yaw.
            new District {
                Id = "sector_14", Name = "Sector 14",
                Anchor = new Vector3(140f, 0f, 20f),
                MapFraming = new DistrictFraming {
                    pitch = 38f, yaw = 300f, distance = 200f, aimOffset = new Vector3(0f, 15f, 0f), fieldOfView = 50f },
                ContractIds = new[] { "contract_03" },
                Requires = new[] { "sector_12" },
                LockedLine = "Nothing routed to Sector 14 yet. Clear Sector 12 and I'll see what's open.",
            },

            // SE quadrant — 794 renderers, tallest 112 m. The monorail S-curve dominates
            // the frame, twin dark towers behind it. Unmistakably transit, and the
            // elevated line is a literal thing to break.
            new District {
                Id = "transit_hub", Name = "Transit Hub",
                Anchor = new Vector3(75f, 0f, -75f),
                MapFraming = new DistrictFraming {
                    pitch = 34f, yaw = 135f, distance = 225f, aimOffset = new Vector3(0f, 5f, 0f), fieldOfView = 50f },
                ContractIds = new[] { "contract_04" },
                Requires = new[] { "sector_14" },
                LockedLine = "Transit won't take an unvetted contractor. Sector 14 is your vetting.",
            },

            // NW quadrant — 719 renderers, tallest 264 m. Neon canyon: CITYNET, ARC
            // HORIZON, the NEO HORIZON DISTRICT billboard. Reads corporate, and it should
            // feel like somewhere that doesn't want you there.
            new District {
                Id = "data_center", Name = "Data Center",
                Anchor = new Vector3(-75f, 0f, 75f),
                MapFraming = new DistrictFraming {
                    pitch = 30f, yaw = 310f, distance = 279f, aimOffset = new Vector3(0f, 30f, 0f), fieldOfView = 50f },
                ContractIds = new[] { "contract_05" },
                Requires = new[] { "transit_hub" },
                LockedLine = "Data Center's private. They take a referral off the Transit Hub job, not before.",
            },
        };

        private static bool _resolved;

        public static List<District> All { get { Resolve(); return _all; } }

        // ─── Resolution and validation ───

        /// <summary>
        /// Binds contract ids to defs once. A contract in no district would silently
        /// vanish from the board, so the orphan check is worth its five lines.
        /// </summary>
        private static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;

            var placed = new HashSet<string>();

            foreach (var d in _all)
            {
                d.Contracts.Clear();
                if (d.ContractIds == null) continue;

                foreach (var id in d.ContractIds)
                {
                    int index = ContractRegistry.IndexOf(id);
                    if (index < 0)
                    {
                        Debug.LogError("District '" + d.Id + "' lists unknown contract '" + id + "'.");
                        continue;
                    }
                    if (!placed.Add(id))
                        Debug.LogError("Contract '" + id + "' is in more than one district.");
                    d.Contracts.Add(ContractRegistry.All[index]);
                }

                if (d.Contracts.Count == 0)
                    Debug.LogWarning("District '" + d.Id + "' has no contracts and will read as locked.");
            }

            foreach (var def in ContractRegistry.All)
            {
                if (!placed.Contains(def.Id))
                    Debug.LogError("Contract '" + def.Id + "' belongs to no district — it will not "
                                 + "appear on the board. Add it to DistrictRegistry.");
            }
        }

        public static District Find(string districtId)
        {
            Resolve();
            foreach (var d in _all) if (d.Id == districtId) return d;
            return null;
        }

        public static District DistrictOf(string contractId)
        {
            Resolve();
            foreach (var d in _all)
                foreach (var c in d.Contracts)
                    if (c.Id == contractId) return d;
            return null;
        }

        // ─── State ───

        public static bool IsComplete(District d, GameState state)
        {
            if (d.Contracts.Count == 0) return false;
            foreach (var c in d.Contracts) if (!state.IsContractCompleted(c.Id)) return false;
            return true;
        }

        public static bool IsUnlocked(District d, GameState state)
        {
            Resolve();
            if (d.Requires == null) return true;

            foreach (var id in d.Requires)
            {
                var prereq = Find(id);
                if (prereq == null)
                {
                    Debug.LogError("District '" + d.Id + "' requires unknown district '" + id + "'.");
                    return false;
                }
                if (!IsComplete(prereq, state)) return false;
            }
            return true;
        }

        /// <summary>
        /// Worst-case aggregation, per `OVERMAP.md`: the marker takes the state of the
        /// district's <b>worst</b> contract. Amber the moment anything in here is
        /// unfixed — that is the only rule that keeps the promise "amber means there
        /// is work here" once a district holds more than one job.
        /// </summary>
        public static DistrictState StateOf(District d, GameState state)
        {
            if (!IsUnlocked(d, state) || d.Contracts.Count == 0) return DistrictState.Locked;

            foreach (var c in d.Contracts)
                if (!state.IsContractCompleted(c.Id)) return DistrictState.Failing;

            foreach (var c in d.Contracts)
                if (state.StarsFor(c.Id) < Scoring.MaxStars) return DistrictState.Stabilised;

            return DistrictState.Mastered;
        }

        /// <summary>Contracts here still to be completed at all.</summary>
        public static int OpenCount(District d, GameState state)
        {
            int open = 0;
            foreach (var c in d.Contracts) if (!state.IsContractCompleted(c.Id)) open++;
            return open;
        }

        /// <summary>Contracts here at full marks. The caption's numerator.</summary>
        public static int MasteredCount(District d, GameState state)
        {
            int mastered = 0;
            foreach (var c in d.Contracts) if (state.StarsFor(c.Id) >= Scoring.MaxStars) mastered++;
            return mastered;
        }

        // ─── Framing ───

        /// <summary>
        /// Where the overmap camera goes to look at a district, orbited around its
        /// anchor. Same maths the editor's scene view uses, so a framing found by flying
        /// around and reading off pitch/yaw/distance transfers exactly.
        /// </summary>
        public static void CameraFor(District d, out Vector3 position, out Quaternion rotation)
        {
            CameraFor(d.Anchor, d.MapFraming, out position, out rotation);
        }

        /// <summary>Any framing around any point — the map shot and the kerbside shot share it.</summary>
        public static void CameraFor(Vector3 anchor, DistrictFraming f,
                                     out Vector3 position, out Quaternion rotation)
        {
            rotation = Quaternion.Euler(f.pitch, f.yaw, 0f);
            var aim = anchor + f.aimOffset;
            position = aim - rotation * Vector3.forward * f.distance;
        }

        // ─── Availability ───

        /// <summary>
        /// A contract is available when its district is open and its own prerequisites
        /// are done. Districts gate between places; <see cref="ContractDef.Requires"/>
        /// orders jobs within one.
        /// </summary>
        public static bool IsAvailable(ContractDef def, GameState state)
        {
            var d = DistrictOf(def.Id);
            if (d == null || !IsUnlocked(d, state)) return false;

            if (def.Requires != null)
                foreach (var id in def.Requires)
                    if (!state.IsContractCompleted(id)) return false;

            return true;
        }
    }
}
