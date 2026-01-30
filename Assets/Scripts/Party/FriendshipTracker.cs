namespace FieldFriends.Party
{
    /// <summary>
    /// Friendship grows from:
    /// - Walking together (every step)
    /// - Not swapping often (bonus after 20+ steps without swap)
    /// - Choosing WAIT in battle (+3 per use)
    /// - Resting together at Willow End (hidden, area-enter bonus)
    ///
    /// Milestones unlock upgraded abilities at threshold.
    /// Thresholds are hidden from the player.
    /// </summary>
    public static class FriendshipTracker
    {
        public const int UpgradeThreshold = 100;
        public const int WaitActionBonus = 3;
        public const int RestAreaBonus = 10;
        public const int WalkBonus = 1;
        public const int LoyaltyBonus = 2; // when not swapped recently

        public static void OnWaitUsed(CreatureInstance creature)
        {
            if (creature == null || creature.IsResting) return;
            creature.Friendship += WaitActionBonus;
        }

        public static void OnRestAtHome(CreatureInstance creature)
        {
            if (creature == null) return;
            creature.Friendship += RestAreaBonus;
        }

        public static bool HasReachedUpgrade(CreatureInstance creature)
        {
            return creature.HasUpgrade &&
                   creature.Friendship >= UpgradeThreshold;
        }
    }
}
