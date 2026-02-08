using FieldFriends.Core;

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
    /// All constants sourced from GameConstants.
    /// </summary>
    public static class FriendshipTracker
    {
        public static void OnWaitUsed(CreatureInstance creature)
        {
            if (creature == null || creature.IsResting) return;
            creature.Friendship += GameConstants.FriendshipWaitBonus;
        }

        public static void OnRestAtHome(CreatureInstance creature)
        {
            if (creature == null) return;
            creature.Friendship += GameConstants.FriendshipRestBonus;
        }

        public static void OnStep(CreatureInstance creature, bool loyal)
        {
            if (creature == null || creature.IsResting) return;
            creature.Friendship += GameConstants.FriendshipWalkBonus;
            if (loyal)
                creature.Friendship += GameConstants.FriendshipLoyaltyBonus;
        }

        public static bool HasReachedUpgrade(CreatureInstance creature)
        {
            return creature.HasUpgrade &&
                   creature.Friendship >= GameConstants.FriendshipUpgradeThreshold;
        }
    }
}
