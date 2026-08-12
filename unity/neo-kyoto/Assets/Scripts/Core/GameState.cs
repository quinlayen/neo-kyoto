using System.Collections.Generic;

namespace NeoKyoto.Core
{
    /// <summary>
    /// Language features are gated until the contract that teaches them is done.
    /// The order mirrors the teaching progression from the Python prototype.
    /// </summary>
    public enum Feature { Loops, Conditionals, ForLoops, Functions }

    public class GameState
    {
        public static readonly Feature[] UnlockSequence =
        {
            Feature.Loops,
            Feature.Conditionals,
            Feature.ForLoops,
            Feature.Functions
        };

        private readonly HashSet<Feature> _unlocked = new HashSet<Feature>();
        private readonly HashSet<string> _completed = new HashSet<string>();
        private readonly HashSet<string> _retiredCommands = new HashSet<string>();

        /// <summary>Contract ids in board order; the next one unlocks as each is completed.</summary>
        public IEnumerable<string> CompletedContracts { get { return _completed; } }

        public void MarkCompleted(string contractId, int unlockIndex)
        {
            _completed.Add(contractId);
            if (unlockIndex >= 0 && unlockIndex < UnlockSequence.Length)
                _unlocked.Add(UnlockSequence[unlockIndex]);
        }

        public bool IsUnlocked(Feature feature) { return _unlocked.Contains(feature); }

        public bool IsContractCompleted(string contractId) { return _completed.Contains(contractId); }

        /// <summary>
        /// Commands from finished contracts stay callable but do nothing — so old
        /// scripts still run instead of failing with a confusing name error.
        /// </summary>
        public void RetireCommands(IEnumerable<string> names)
        {
            foreach (var name in names) _retiredCommands.Add(name);
        }

        public bool IsRetired(string name) { return _retiredCommands.Contains(name); }

        public IEnumerable<string> RetiredCommands { get { return _retiredCommands; } }

        public IEnumerable<Feature> UnlockedFeatures { get { return _unlocked; } }

        /// <summary>Rebuilds state from a save. Retired commands are stored rather
        /// than re-derived, so loading does not need to instantiate every contract.</summary>
        public void Restore(IEnumerable<string> completed, IEnumerable<Feature> unlocked,
                            IEnumerable<string> retired)
        {
            Reset();
            if (completed != null) foreach (var id in completed) _completed.Add(id);
            if (unlocked != null) foreach (var f in unlocked) _unlocked.Add(f);
            if (retired != null) foreach (var name in retired) _retiredCommands.Add(name);
        }

        public void UnlockAll(IEnumerable<string> contractIds)
        {
            foreach (var f in UnlockSequence) _unlocked.Add(f);
            foreach (var id in contractIds) _completed.Add(id);
        }

        public void Reset()
        {
            _unlocked.Clear();
            _completed.Clear();
            _retiredCommands.Clear();
        }
    }
}
