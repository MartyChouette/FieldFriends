using UnityEngine;
using FieldFriends.Data;
using FieldFriends.Core;

namespace FieldFriends.World
{
    /// <summary>
    /// Listens for player steps and rolls for random encounters
    /// based on the current area's encounter table.
    /// Max encounter rate: ~12% per step.
    /// Quiet Grove: no encounters.
    /// </summary>
    public class EncounterSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] AreaManager areaManager;

        [Header("Tuning")]
        [SerializeField] int minStepsBetweenEncounters = 3;

        int _stepsSinceLastEncounter;

        void OnEnable()
        {
            GridMovement.OnStepTaken += OnPlayerStep;
        }

        void OnDisable()
        {
            GridMovement.OnStepTaken -= OnPlayerStep;
        }

        void OnPlayerStep(Vector2Int gridPos)
        {
            if (areaManager == null) return;

            var area = areaManager.GetCurrentAreaDef();
            if (!area.HasEncounters || area.Encounters.Length == 0)
                return;

            _stepsSinceLastEncounter++;
            if (_stepsSinceLastEncounter < minStepsBetweenEncounters)
                return;

            // Apply encounter rate modifiers from party abilities
            float rate = area.EncounterRate;
            rate = ApplyPartyModifiers(rate);

            if (Random.value > rate)
                return;

            // Roll which creature to encounter
            _stepsSinceLastEncounter = 0;
            string creatureName = RollEncounter(area.Encounters);

            if (creatureName != null)
            {
                TriggerBattle(creatureName);
            }
        }

        float ApplyPartyModifiers(float baseRate)
        {
            // Flit's Scout Ahead: lower encounter rate
            var partyManager = FindFirstObjectByType<Party.PartyManager>();
            if (partyManager != null && partyManager.PartyHasAbility(AbilityID.ScoutAhead))
            {
                baseRate *= 0.5f;
            }
            return baseRate;
        }

        string RollEncounter(EncounterSlot[] table)
        {
            int totalWeight = 0;
            for (int i = 0; i < table.Length; i++)
                totalWeight += table[i].Weight;

            if (totalWeight <= 0) return null;

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < table.Length; i++)
            {
                cumulative += table[i].Weight;
                if (roll < cumulative)
                    return table[i].creatureName;
            }

            return table[table.Length - 1].creatureName;
        }

        void TriggerBattle(string creatureName)
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.StartBattle(creatureName);
            }
        }
    }
}
