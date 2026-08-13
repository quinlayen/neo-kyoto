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

        // ─── Scoring ───

        private readonly Dictionary<string, ContractScore> _scores = new Dictionary<string, ContractScore>();

        public int Credits { get; private set; }

        public IEnumerable<KeyValuePair<string, ContractScore>> Scores { get { return _scores; } }

        public ContractScore ScoreFor(string contractId)
        {
            ContractScore s;
            return _scores.TryGetValue(contractId, out s) ? s : null;
        }

        public int StarsFor(string contractId)
        {
            var s = ScoreFor(contractId);
            return s != null ? s.Stars : 0;
        }

        public int TotalStars
        {
            get
            {
                int total = 0;
                foreach (var kv in _scores) total += kv.Value.Stars;
                return total;
            }
        }

        public string Rank { get { return Scoring.RankFor(TotalStars); } }

        /// <summary>
        /// Records a result. A replay only pays the difference, so improving a
        /// rating is rewarded but re-running the same solution is not farmable.
        /// </summary>
        public int RecordScore(string contractId, int stars, int callsToGoal,
                               bool bonusFound, int baseCredits)
        {
            ContractScore existing;
            if (!_scores.TryGetValue(contractId, out existing))
            {
                existing = new ContractScore();
                _scores[contractId] = existing;
            }

            int paid = 0;
            if (stars > existing.Stars)
            {
                paid = Scoring.CreditsFor(stars, baseCredits)
                     - Scoring.CreditsFor(existing.Stars, baseCredits);
                Credits += paid;
                existing.Stars = stars;
            }

            // Keep the best run's call count, and never un-find a bonus.
            if (callsToGoal > 0 && (existing.CallsToGoal == 0 || callsToGoal < existing.CallsToGoal))
                existing.CallsToGoal = callsToGoal;
            if (bonusFound) existing.BonusFound = true;

            return paid;
        }

        public void RestoreScores(IEnumerable<KeyValuePair<string, ContractScore>> scores, int credits)
        {
            _scores.Clear();
            if (scores != null) foreach (var kv in scores) _scores[kv.Key] = kv.Value;
            Credits = credits;
        }

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
            _scores.Clear();
            Credits = 0;
        }
    }
}
