using System.Collections.Generic;

namespace NeoKyoto.Core
{
    /// <summary>
    /// Star ratings, credits and contractor rank. Mirrors the Python prototype's
    /// gamification layer (game_state.py) so both stay in step.
    ///
    /// Rating policy is deliberately isolated in <see cref="RateContract"/> — it is
    /// the part most likely to change as the design settles.
    /// </summary>
    public static class Scoring
    {
        public const int MaxStars = 3;

        /// <summary>Total stars needed for each contractor rank.</summary>
        private static readonly (int Stars, string Title)[] Ranks =
        {
            (0,  "Junior Contractor"),
            (6,  "Contractor"),
            (13, "Senior Contractor"),
            (21, "Systems Engineer"),
            (29, "Chief Architect"),
        };

        public static string RankFor(int totalStars)
        {
            string rank = Ranks[0].Title;
            foreach (var r in Ranks)
                if (totalStars >= r.Stars) rank = r.Title;
            return rank;
        }

        /// <summary>Stars still needed for the next rank, or 0 at the top rank.</summary>
        public static int StarsToNextRank(int totalStars)
        {
            foreach (var r in Ranks)
                if (totalStars < r.Stars) return r.Stars - totalStars;
            return 0;
        }

        public static string NextRankTitle(int totalStars)
        {
            foreach (var r in Ranks)
                if (totalStars < r.Stars) return r.Title;
            return null;
        }

        /// <summary>
        /// How far through the current rank band the player is, 0-1, so a progress
        /// bar fills toward the next promotion rather than toward total completion.
        /// Returns 1 at the top rank.
        /// </summary>
        public static float RankProgress(int totalStars)
        {
            int lower = 0;
            foreach (var r in Ranks)
            {
                if (totalStars < r.Stars)
                {
                    int span = r.Stars - lower;
                    return span <= 0 ? 1f : (float)(totalStars - lower) / span;
                }
                lower = r.Stars;
            }
            return 1f;
        }

        /// <summary>
        /// Filled and empty rating marks, e.g. "◆◆◇".
        /// Cascadia Mono has no ★ or ☆ glyph — they render as placeholder boxes —
        /// so the diamond pair stands in for stars throughout the UI.
        /// </summary>
        public static string FormatStars(int count)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < MaxStars; i++) sb.Append(i < count ? '◆' : '◇');
            return sb.ToString();
        }

        /// <summary>
        /// Scores a scripted contract.
        ///
        /// Calls are counted up to the moment the goal is met, not for the whole run.
        /// A `while True` loop cannot stop itself, so it always burns the sandbox call
        /// cap; counting the whole run would score the loop far worse than writing the
        /// same command out by hand, which is the opposite of what the game teaches.
        /// </summary>
        public static int RateContract(int callsToGoal, int threeStar, int twoStar)
        {
            if (threeStar > 0 && callsToGoal <= threeStar) return 3;
            if (twoStar > 0 && callsToGoal <= twoStar) return 2;
            return 1;
        }

        /// <summary>Credits are stars x base rate; a replay pays only the improvement.</summary>
        public static int CreditsFor(int stars, int baseCredits) { return stars * baseCredits; }
    }

    /// <summary>Per-contract score, persisted with the rest of the save.</summary>
    public class ContractScore
    {
        public int Stars;
        public int CallsToGoal;
        public bool BonusFound;
    }
}
