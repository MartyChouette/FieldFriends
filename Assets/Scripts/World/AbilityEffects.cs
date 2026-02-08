using UnityEngine;
using System.Collections;
using FieldFriends.Core;
using FieldFriends.Data;
using FieldFriends.Party;
using FieldFriends.UI;

namespace FieldFriends.World
{
    /// <summary>
    /// Overworld ability effects. Each creature ability has a world
    /// effect in addition to its battle effect. These are checked
    /// automatically based on the lead party member's abilities.
    ///
    /// Abilities and their overworld effects:
    /// - Part the Grass (Mossbit): reveal hidden paths/items
    /// - Snare Step (Bramblet): no overworld effect
    /// - Dig Cache (Loamle): find healing roots occasionally
    /// - Hop Lift (Skirl): cross small gaps, +walk speed
    /// - Scout Ahead (Flit): lower encounter rate (handled in EncounterSystem)
    /// - Gentle Gust (Wispin): no overworld effect (battle only)
    /// - Stream Glide (Plen): water traversal
    /// - Clear Pool (Ripplet): no overworld effect (battle heal)
    /// - Slip Away (Drift): no overworld effect (battle flee)
    /// - Soft Light (Bloomel): no overworld effect (battle DEF)
    /// - Calm Field (Petalyn): no overworld effect (battle aggro)
    /// - Wait (Still): no overworld effect
    ///
    /// Upgraded abilities (friendship milestones):
    /// - Root Hold (Mossbit): no overworld effect (DEF buff)
    /// - Quick Return (Skirl): auto-escape once per area
    /// - Find Shine (Plen): find items near water
    /// - Steady Field (Bloomel): no overworld effect (persistent DEF)
    /// </summary>
    public class AbilityEffects : MonoBehaviour
    {
        [SerializeField] PartyManager partyManager;

        int _stepsSinceLastDig;
        bool _quickReturnUsed;
        bool _revealedThisArea;

        const int DigCacheInterval = 25; // steps between dig finds
        const float SpeedBoostMultiplier = 1.3f;

        void OnEnable()
        {
            AreaManager.OnAreaChanged += OnAreaChanged;
        }

        void OnDisable()
        {
            AreaManager.OnAreaChanged -= OnAreaChanged;
        }

        void OnAreaChanged(AreaID area)
        {
            _quickReturnUsed = false;
            _revealedThisArea = false;
        }

        /// <summary>
        /// Called by GridMovement on each step.
        /// </summary>
        public void OnStep(Vector2Int position)
        {
            if (partyManager == null) return;

            var lead = partyManager.GetLeadCreature();
            if (lead == null) return;

            // Dig Cache (Loamle) - find healing roots
            if (lead.HasAbility(AbilityID.DigCache))
            {
                _stepsSinceLastDig++;
                if (_stepsSinceLastDig >= DigCacheInterval)
                {
                    _stepsSinceLastDig = 0;
                    if (Random.value < 0.4f)
                    {
                        TriggerDigCache();
                    }
                }
            }

            // Find Shine (Plen upgrade) - find items near water
            if (lead.HasAbility(AbilityID.FindShine))
            {
                // Check if near water tile
                if (IsNearWater(position) && Random.value < 0.15f)
                {
                    TriggerFindShine();
                }
            }
        }

        /// <summary>
        /// Check if the lead creature grants a speed boost.
        /// </summary>
        public float GetSpeedMultiplier()
        {
            if (partyManager == null) return 1f;
            var lead = partyManager.GetLeadCreature();
            if (lead != null && lead.HasAbility(AbilityID.HopLift))
                return SpeedBoostMultiplier;
            return 1f;
        }

        /// <summary>
        /// Check if the lead creature can traverse water tiles.
        /// </summary>
        public bool CanTraverseWater()
        {
            if (partyManager == null) return false;
            var lead = partyManager.GetLeadCreature();
            return lead != null && lead.HasAbility(AbilityID.StreamGlide);
        }

        /// <summary>
        /// Check if the lead creature can cross small gaps.
        /// </summary>
        public bool CanCrossGaps()
        {
            if (partyManager == null) return false;
            var lead = partyManager.GetLeadCreature();
            return lead != null && lead.HasAbility(AbilityID.HopLift);
        }

        /// <summary>
        /// Check if Quick Return (Skirl upgrade) can be used.
        /// Auto-escapes once per area.
        /// </summary>
        public bool TryQuickReturn()
        {
            if (_quickReturnUsed) return false;
            if (partyManager == null) return false;
            var lead = partyManager.GetLeadCreature();
            if (lead != null && lead.HasAbility(AbilityID.QuickReturn))
            {
                _quickReturnUsed = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Part the Grass (Mossbit) - reveal hidden paths.
        /// Called when entering a new area to check for reveals.
        /// </summary>
        public bool CanRevealHidden()
        {
            if (_revealedThisArea) return false;
            if (partyManager == null) return false;
            var lead = partyManager.GetLeadCreature();
            if (lead != null && lead.HasAbility(AbilityID.PartTheGrass))
            {
                _revealedThisArea = true;
                return true;
            }
            return false;
        }

        void TriggerDigCache()
        {
            // Heal lead creature a small amount
            var lead = partyManager.GetLeadCreature();
            if (lead != null && lead.CurrentHP < lead.MaxHP)
            {
                int healAmount = Mathf.Max(1, lead.MaxHP / 6);
                lead.Heal(healAmount);

                var dialogueBox = FindFirstObjectByType<DialogueBox>();
                if (dialogueBox != null)
                {
                    StartCoroutine(dialogueBox.ShowDialogue(
                        $"{lead.Name} digs up a healing root."));
                }
            }
        }

        void TriggerFindShine()
        {
            // Heal entire party a small amount
            foreach (var creature in partyManager.Party)
            {
                if (!creature.IsResting)
                {
                    int healAmount = Mathf.Max(1, creature.MaxHP / 8);
                    creature.Heal(healAmount);
                }
            }

            var dialogueBox = FindFirstObjectByType<DialogueBox>();
            if (dialogueBox != null)
            {
                var lead = partyManager.GetLeadCreature();
                StartCoroutine(dialogueBox.ShowDialogue(
                    $"{lead.Name} finds something in the water."));
            }
        }

        bool IsNearWater(Vector2Int pos)
        {
            // Simple check: look for water tiles nearby using tilemap
            var mapGen = FindFirstObjectByType<ProceduralMapGenerator>();
            if (mapGen == null) return false;

            // Check area type as proxy
            var areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager == null) return false;

            var area = areaManager.CurrentArea;
            return area == AreaID.CreekPath ||
                   area == AreaID.Stonebridge;
        }
    }
}
