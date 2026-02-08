using FieldFriends.Core;
using FieldFriends.Data;

namespace FieldFriends.Party
{
    /// <summary>
    /// A live instance of a creature in the player's party or encountered in battle.
    /// Tracks current HP and friendship.
    /// </summary>
    [System.Serializable]
    public class CreatureInstance
    {
        public string SpeciesName;
        public CreatureType Type;
        public int MaxHP;
        public int CurrentHP;
        public int ATK;
        public int DEF;
        public int SPD;
        public AbilityID Ability;
        public AbilityID UpgradedAbility;
        public bool HasUpgrade;
        public CreatureState State;

        // Friendship: hidden counter, increases by walking together
        public int Friendship;

        public string Name => SpeciesName;
        public bool IsResting => State == CreatureState.Resting;
        public bool IsAbilityUpgraded => HasUpgrade && Friendship >= GameConstants.FriendshipUpgradeThreshold;

        public bool HasAbility(AbilityID id)
        {
            return ActiveAbility == id || (IsAbilityUpgraded && UpgradedAbility == id);
        }

        public AbilityID ActiveAbility =>
            IsAbilityUpgraded ? UpgradedAbility : Ability;

        /// <summary>
        /// Create an instance from a database definition.
        /// Stats are scaled: base stat * 3 for HP, others used directly.
        /// </summary>
        public static CreatureInstance FromDef(CreatureDatabase.CreatureDef def)
        {
            return new CreatureInstance
            {
                SpeciesName = def.Name,
                Type = def.Type,
                MaxHP = def.HP * GameConstants.HPMultiplier,
                CurrentHP = def.HP * GameConstants.HPMultiplier,
                ATK = def.ATK,
                DEF = def.DEF,
                SPD = def.SPD,
                Ability = def.Ability,
                UpgradedAbility = def.UpgradedAbility,
                HasUpgrade = def.HasUpgrade,
                State = CreatureState.Active,
                Friendship = 0
            };
        }

        /// <summary>
        /// Create a wild encounter creature (no friendship).
        /// </summary>
        public static CreatureInstance WildFromDef(CreatureDatabase.CreatureDef def)
        {
            var instance = FromDef(def);
            instance.Friendship = -1; // wild
            return instance;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP -= amount;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                State = CreatureState.Resting;
            }
        }

        public void Heal(int amount)
        {
            CurrentHP += amount;
            if (CurrentHP > MaxHP)
                CurrentHP = MaxHP;

            if (CurrentHP > 0)
                State = CreatureState.Active;
        }

        public void FullHeal()
        {
            CurrentHP = MaxHP;
            State = CreatureState.Active;
        }
    }
}
