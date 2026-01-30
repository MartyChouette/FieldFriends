using UnityEngine;

namespace FieldFriends.Data
{
    /// <summary>
    /// Definition of a single area in Bramble Vale.
    /// </summary>
    [CreateAssetMenu(fileName = "NewArea", menuName = "FieldFriends/Area Data")]
    public class AreaData : ScriptableObject
    {
        public AreaID areaID;
        public string displayName;

        [Header("Encounters")]
        public bool hasEncounters = true;
        [Range(0f, 0.12f)]
        public float encounterRate = 0.10f;
        public EncounterSlot[] encounterTable;

        [Header("Connections")]
        public AreaID[] connectedAreas;

        [Header("Scene")]
        public string sceneName;
    }

    [System.Serializable]
    public struct EncounterSlot
    {
        public string creatureName;
        public EncounterRarity rarity;

        /// <summary>
        /// Weight used in encounter roll. Common = 60, Uncommon = 30, Rare = 10.
        /// </summary>
        public int Weight
        {
            get
            {
                switch (rarity)
                {
                    case EncounterRarity.Common:   return 60;
                    case EncounterRarity.Uncommon: return 30;
                    case EncounterRarity.Rare:     return 10;
                    default: return 0;
                }
            }
        }
    }
}
