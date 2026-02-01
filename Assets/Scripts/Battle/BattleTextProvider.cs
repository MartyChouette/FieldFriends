using FieldFriends.Party;

namespace FieldFriends.Battle
{
    /// <summary>
    /// All battle text, spec-accurate.
    /// Short sentences. No exclamation points. No moral framing.
    /// </summary>
    public static class BattleTextProvider
    {
        // --- Encounter start ---
        public static string EncounterStart(string creatureName)
            => $"A wild {creatureName} appeared.";

        // --- Player actions ---
        public static string MoveAttack(string attackerName)
            => $"{attackerName} nudges forward.";

        public static string WaitAction(string creatureName)
            => $"{creatureName} waits quietly.";

        public static string BackAttempt()
            => "You try to step back.";

        public static string BackSuccess()
            => "You slip away.";

        public static string BackFail()
            => "You can't get away.";

        // --- Enemy actions ---
        public static string EnemyAttack(string enemyName)
            => $"{enemyName} moves closer.";

        public static string EnemyWait(string enemyName)
            => $"{enemyName} watches quietly.";

        // --- Damage ---
        public static string TakeDamage(string targetName)
            => $"{targetName} takes a hit.";

        public static string WeakHit(string targetName)
            => $"It barely connects.";

        public static string StrongHit(string targetName)
            => $"It lands well.";

        // --- KO / Rest ---
        public static string NeedsRest(string creatureName)
            => $"{creatureName} needs to rest.";

        public static string TheyLookTired(string creatureName)
            => $"They look tired.";

        public static string SwapIn(string creatureName)
            => $"{creatureName} steps forward.";

        // --- Battle end ---
        public static string Win()
            => "You keep moving.";

        public static string Lose()
            => "You head back for now.";

        public static string AllResting()
            => "Everyone needs to rest.";

        // --- Ability text ---
        public static string StillNullifies()
            => "Nothing happens.";

        public static string CalmFieldActive(string creatureName)
            => $"{creatureName} calms the air.";

        public static string ClearPoolHeal(string creatureName)
            => $"{creatureName} clears the water.";

        public static string SnareStepSlow()
            => "Something holds them back.";

        public static string GentleGustEvade()
            => "The wind shifts.";

        // --- Turn order ---
        public static string TurnStart(string creatureName)
            => $"{creatureName}'s turn.";

        // --- Friendship ---
        public static string FriendshipGrew(string creatureName)
            => $"{creatureName} seems closer.";

        public static string AbilityUpgrade(string creatureName, string abilityName)
            => $"{creatureName} learned {abilityName}.";
    }
}
