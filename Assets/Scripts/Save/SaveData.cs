using System.Collections.Generic;
using FieldFriends.Data;
using FieldFriends.Party;

namespace FieldFriends.Save
{
    /// <summary>
    /// Serializable save data. Single save slot.
    /// Captures party, current area, and player position.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public AreaID CurrentArea;
        public int PlayerGridX;
        public int PlayerGridY;
        public List<CreatureSaveData> Party = new List<CreatureSaveData>();
    }

    [System.Serializable]
    public class CreatureSaveData
    {
        public string SpeciesName;
        public int CurrentHP;
        public int MaxHP;
        public int ATK;
        public int DEF;
        public int SPD;
        public int Friendship;
        public CreatureState State;
        public CreatureType Type;
        public AbilityID Ability;
        public AbilityID UpgradedAbility;
        public bool HasUpgrade;

        public static CreatureSaveData FromInstance(CreatureInstance c)
        {
            return new CreatureSaveData
            {
                SpeciesName = c.SpeciesName,
                CurrentHP = c.CurrentHP,
                MaxHP = c.MaxHP,
                ATK = c.ATK,
                DEF = c.DEF,
                SPD = c.SPD,
                Friendship = c.Friendship,
                State = c.State,
                Type = c.Type,
                Ability = c.Ability,
                UpgradedAbility = c.UpgradedAbility,
                HasUpgrade = c.HasUpgrade,
            };
        }

        public CreatureInstance ToInstance()
        {
            return new CreatureInstance
            {
                SpeciesName = SpeciesName,
                CurrentHP = CurrentHP,
                MaxHP = MaxHP,
                ATK = ATK,
                DEF = DEF,
                SPD = SPD,
                Friendship = Friendship,
                State = State,
                Type = Type,
                Ability = Ability,
                UpgradedAbility = UpgradedAbility,
                HasUpgrade = HasUpgrade,
            };
        }
    }
}
