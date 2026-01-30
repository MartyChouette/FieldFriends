using UnityEngine;

namespace FieldFriends.Data
{
    /// <summary>
    /// Immutable definition of a creature species.
    /// Created as ScriptableObject assets under Resources/Data/Creatures.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCreature", menuName = "FieldFriends/Creature Data")]
    public class CreatureData : ScriptableObject
    {
        [Header("Identity")]
        public string creatureName;
        public CreatureType creatureType;
        public Sprite portrait;     // 16x16 or 24x24
        public bool isLargeSprite;  // true for Bloomel, Still (24x24)

        [Header("Base Stats (1-10)")]
        [Range(1, 10)] public int baseHP  = 5;
        [Range(1, 10)] public int baseATK = 5;
        [Range(1, 10)] public int baseDEF = 5;
        [Range(1, 10)] public int baseSPD = 5;

        [Header("Abilities")]
        public string abilityName;
        [TextArea(1, 3)]
        public string abilityDescription;
        public AbilityID abilityID;

        [Header("Friendship Upgrade (optional)")]
        public string upgradedAbilityName;
        [TextArea(1, 3)]
        public string upgradedAbilityDescription;
        public AbilityID upgradedAbilityID;
        public bool hasUpgrade;

        [Header("Flavor")]
        [TextArea(1, 2)]
        public string idleText; // e.g. "Mossbit nudges forward."
    }

    /// <summary>
    /// All creature abilities by ID, used for gameplay hooks.
    /// </summary>
    public enum AbilityID
    {
        None,

        // Field
        PartTheGrass,   // Mossbit: reveal hidden paths/items
        RootHold,       // Mossbit upgrade: small DEF buff
        SnareStep,      // Bramblet: chance slow
        DigCache,       // Loamle: find healing roots

        // Wind
        HopLift,        // Skirl: cross gaps, +walk speed
        QuickReturn,    // Skirl upgrade: auto-escape once per area
        ScoutAhead,     // Flit: lower encounter rate
        GentleGust,     // Wispin: chance evade

        // Water
        StreamGlide,    // Plen: water traversal
        FindShine,      // Plen upgrade: find items near water
        ClearPool,      // Ripplet: post-battle heal
        SlipAway,       // Drift: easy escape

        // Meadow
        SoftLight,      // Bloomel: party DEF boost
        SteadyField,    // Bloomel upgrade: DEF persists after battle
        CalmField,      // Petalyn: reduce enemy aggression
        Wait            // Still: nullify enemy turn
    }
}
