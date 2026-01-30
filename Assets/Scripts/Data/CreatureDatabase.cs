using UnityEngine;

namespace FieldFriends.Data
{
    /// <summary>
    /// Hardcoded creature definitions for all 12 species.
    /// Used to bootstrap ScriptableObject assets or as a runtime fallback.
    /// </summary>
    public static class CreatureDatabase
    {
        public struct CreatureDef
        {
            public string Name;
            public CreatureType Type;
            public int HP, ATK, DEF, SPD;
            public AbilityID Ability;
            public AbilityID UpgradedAbility;
            public bool HasUpgrade;
            public bool IsLarge;
            public string IdleText;
        }

        public static readonly CreatureDef[] All = new CreatureDef[]
        {
            // --- Field ---
            new CreatureDef {
                Name = "Mossbit", Type = CreatureType.Field,
                HP = 8, ATK = 3, DEF = 7, SPD = 2,
                Ability = AbilityID.PartTheGrass,
                UpgradedAbility = AbilityID.RootHold, HasUpgrade = true,
                IdleText = "Mossbit nudges forward."
            },
            new CreatureDef {
                Name = "Bramblet", Type = CreatureType.Field,
                HP = 6, ATK = 4, DEF = 5, SPD = 3,
                Ability = AbilityID.SnareStep,
                IdleText = "Bramblet shifts in place."
            },
            new CreatureDef {
                Name = "Loamle", Type = CreatureType.Field,
                HP = 7, ATK = 5, DEF = 4, SPD = 2,
                Ability = AbilityID.DigCache,
                IdleText = "Loamle paws at the ground."
            },

            // --- Wind ---
            new CreatureDef {
                Name = "Skirl", Type = CreatureType.Wind,
                HP = 4, ATK = 5, DEF = 2, SPD = 8,
                Ability = AbilityID.HopLift,
                UpgradedAbility = AbilityID.QuickReturn, HasUpgrade = true,
                IdleText = "Skirl hops lightly."
            },
            new CreatureDef {
                Name = "Flit", Type = CreatureType.Wind,
                HP = 5, ATK = 3, DEF = 3, SPD = 7,
                Ability = AbilityID.ScoutAhead,
                IdleText = "Flit looks around."
            },
            new CreatureDef {
                Name = "Wispin", Type = CreatureType.Wind,
                HP = 3, ATK = 6, DEF = 2, SPD = 9,
                Ability = AbilityID.GentleGust,
                IdleText = "Wispin drifts gently."
            },

            // --- Water ---
            new CreatureDef {
                Name = "Plen", Type = CreatureType.Water,
                HP = 6, ATK = 4, DEF = 4, SPD = 4,
                Ability = AbilityID.StreamGlide,
                UpgradedAbility = AbilityID.FindShine, HasUpgrade = true,
                IdleText = "Plen ripples quietly."
            },
            new CreatureDef {
                Name = "Ripplet", Type = CreatureType.Water,
                HP = 7, ATK = 3, DEF = 5, SPD = 3,
                Ability = AbilityID.ClearPool,
                IdleText = "Ripplet settles down."
            },
            new CreatureDef {
                Name = "Drift", Type = CreatureType.Water,
                HP = 4, ATK = 5, DEF = 3, SPD = 7,
                Ability = AbilityID.SlipAway,
                IdleText = "Drift floats along."
            },

            // --- Meadow ---
            new CreatureDef {
                Name = "Bloomel", Type = CreatureType.Meadow,
                HP = 8, ATK = 4, DEF = 6, SPD = 2,
                Ability = AbilityID.SoftLight,
                UpgradedAbility = AbilityID.SteadyField, HasUpgrade = true,
                IsLarge = true,
                IdleText = "Bloomel glows faintly."
            },
            new CreatureDef {
                Name = "Petalyn", Type = CreatureType.Meadow,
                HP = 5, ATK = 4, DEF = 5, SPD = 5,
                Ability = AbilityID.CalmField,
                IdleText = "Petalyn sways slowly."
            },
            new CreatureDef {
                Name = "Still", Type = CreatureType.Meadow,
                HP = 9, ATK = 2, DEF = 8, SPD = 1,
                Ability = AbilityID.Wait,
                IsLarge = true,
                IdleText = "Still is still."
            },
        };

        public static CreatureDef? FindByName(string name)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].Name == name) return All[i];
            }
            return null;
        }
    }
}
