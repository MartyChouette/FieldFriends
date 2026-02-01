namespace FieldFriends.Core
{
    /// <summary>
    /// All tunable constants in one place. No magic numbers in game code.
    /// </summary>
    public static class GameConstants
    {
        // --- Screen / Camera ---
        public const int TileSize = 8;
        public const int ScreenTilesX = 20;
        public const int ScreenTilesY = 18;
        public const int PixelsPerUnit = 8;
        public const float CameraOrthoSize = ScreenTilesY / 2f; // 9

        // --- Movement ---
        public const float MoveSpeed = 4f;  // tiles per second
        public const float MoveStepDuration = 1f / MoveSpeed;

        // --- Encounters ---
        public const float MaxEncounterRate = 0.12f;
        public const int MinStepsBetweenEncounters = 4;
        public const float ScoutAheadModifier = 0.5f;  // Flit's ability

        // --- Battle ---
        public const float TypeAdvantageMultiplier = 1.5f;
        public const float TypeDisadvantageMultiplier = 0.75f;
        public const int BaseDamage = 2;
        public const int TargetBattleTurns = 5; // 4-6 turn target
        public const float StillWaitNullifyChance = 0.30f;
        public const float PetalynCalmFieldModifier = 0.70f;
        public const float WispinGustEvadeChance = 0.30f;
        public const float BaseFleeChance = 0.50f;
        public const float DriftSlipAwayBonus = 0.30f;
        public const float SkirlQuickReturnBonus = 0.25f;

        // --- Encounter Rarity Weights ---
        public const float WeightCommon = 0.65f;
        public const float WeightUncommon = 0.25f;
        public const float WeightRare = 0.10f;

        // --- Party ---
        public const int MaxPartySize = 3;
        public const int HPMultiplier = 3;   // MaxHP = stat.HP * this

        // --- Friendship ---
        public const int FriendshipUpgradeThreshold = 100;
        public const int FriendshipWalkBonus = 1;
        public const int FriendshipLoyaltyBonus = 2;
        public const int FriendshipWaitBonus = 3;
        public const int FriendshipRestBonus = 10;
        public const int LoyaltyStepThreshold = 20;

        // --- Animation ---
        public const float IdleAnimMinLoop = 0.8f;
        public const float IdleAnimMaxLoop = 1.2f;
        public const int IdleAnimFrames = 2;

        // --- Screen Transition ---
        public const float FadeDuration = 0.4f;

        // --- Save ---
        public const string SaveFileName = "fieldfriends_save.json";

        // --- Audio ---
        public const float MusicVolume = 0.6f;
        public const float SFXVolume = 0.8f;
    }
}
