using System.Collections.Generic;
using UnityEngine;
using FieldFriends.Data;

namespace FieldFriends.Party
{
    /// <summary>
    /// Manages the player's party of up to 3 creatures.
    /// Tracks friendship, handles party ordering, and provides ability queries.
    /// </summary>
    public class PartyManager : MonoBehaviour
    {
        public const int MaxPartySize = Core.GameConstants.MaxPartySize;

        [SerializeField] List<CreatureInstance> party = new List<CreatureInstance>();

        int _stepsSinceLastSwap;

        public IReadOnlyList<CreatureInstance> Party => party;
        public int Count => party.Count;

        void OnEnable()
        {
            Core.GridMovement.OnStepTaken += OnStepTaken;
        }

        void OnDisable()
        {
            Core.GridMovement.OnStepTaken -= OnStepTaken;
        }

        void OnStepTaken(Vector2Int pos)
        {
            _stepsSinceLastSwap++;
            bool loyal = _stepsSinceLastSwap > Core.GameConstants.LoyaltyStepThreshold;

            for (int i = 0; i < party.Count; i++)
            {
                FriendshipTracker.OnStep(party[i], loyal);
            }
        }

        public bool AddToParty(CreatureInstance creature)
        {
            if (party.Count >= MaxPartySize) return false;
            party.Add(creature);
            return true;
        }

        public void RemoveFromParty(int index)
        {
            if (index < 0 || index >= party.Count) return;
            party.RemoveAt(index);
        }

        public void SwapOrder(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= party.Count) return;
            if (indexB < 0 || indexB >= party.Count) return;
            var temp = party[indexA];
            party[indexA] = party[indexB];
            party[indexB] = temp;
            _stepsSinceLastSwap = 0;
        }

        /// <summary>
        /// Returns the lead (first non-resting) creature, or null if all resting.
        /// </summary>
        public CreatureInstance GetLead()
        {
            for (int i = 0; i < party.Count; i++)
            {
                if (!party[i].IsResting) return party[i];
            }
            return null;
        }

        /// <summary>
        /// Returns all non-resting creatures.
        /// </summary>
        public List<CreatureInstance> GetActiveParty()
        {
            var active = new List<CreatureInstance>();
            for (int i = 0; i < party.Count; i++)
            {
                if (!party[i].IsResting) active.Add(party[i]);
            }
            return active;
        }

        public bool AllResting()
        {
            for (int i = 0; i < party.Count; i++)
            {
                if (!party[i].IsResting) return false;
            }
            return true;
        }

        /// <summary>
        /// Check if any active party member has a specific ability.
        /// </summary>
        public bool PartyHasAbility(AbilityID ability)
        {
            for (int i = 0; i < party.Count; i++)
            {
                if (!party[i].IsResting && party[i].ActiveAbility == ability)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Heal all party members fully (e.g., rest at home).
        /// </summary>
        public void HealAll()
        {
            for (int i = 0; i < party.Count; i++)
                party[i].FullHeal();
        }

        /// <summary>
        /// Clear all party members (for new game / load).
        /// </summary>
        public void ClearParty()
        {
            party.Clear();
            _stepsSinceLastSwap = 0;
        }

        /// <summary>
        /// Get the lead non-resting creature (alias for GetLead).
        /// </summary>
        public CreatureInstance GetLeadCreature()
        {
            return GetLead();
        }
    }
}
