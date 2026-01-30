namespace FieldFriends.Data
{
    /// <summary>
    /// Type effectiveness chart.
    /// Field > Water > Wind > Field. Meadow is neutral.
    /// No multiplier exceeds 1.5x.
    /// </summary>
    public static class TypeChart
    {
        public static float GetMultiplier(CreatureType attacker, CreatureType defender)
        {
            // Meadow is neutral against everything
            if (attacker == CreatureType.Meadow || defender == CreatureType.Meadow)
                return 1.0f;

            // Field > Water
            if (attacker == CreatureType.Field && defender == CreatureType.Water)
                return 1.5f;
            // Water > Wind
            if (attacker == CreatureType.Water && defender == CreatureType.Wind)
                return 1.5f;
            // Wind > Field
            if (attacker == CreatureType.Wind && defender == CreatureType.Field)
                return 1.5f;

            // Reverse: disadvantage
            if (attacker == CreatureType.Water && defender == CreatureType.Field)
                return 0.75f;
            if (attacker == CreatureType.Wind && defender == CreatureType.Water)
                return 0.75f;
            if (attacker == CreatureType.Field && defender == CreatureType.Wind)
                return 0.75f;

            return 1.0f;
        }
    }
}
