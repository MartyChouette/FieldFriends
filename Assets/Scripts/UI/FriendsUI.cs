using UnityEngine;
using UnityEngine.UI;
using FieldFriends.Party;
using FieldFriends.Palette;

namespace FieldFriends.UI
{
    /// <summary>
    /// Shows the player's party of up to 3 creatures.
    /// Displays name, HP bar, and type. Minimal UI noise.
    /// </summary>
    public class FriendsUI : MonoBehaviour
    {
        [Header("Creature Slots (3 max)")]
        [SerializeField] GameObject[] slotPanels = new GameObject[3];
        [SerializeField] Text[] nameLabels = new Text[3];
        [SerializeField] Slider[] hpBars = new Slider[3];
        [SerializeField] Text[] typeLabels = new Text[3];
        [SerializeField] Text[] statusLabels = new Text[3];

        public void Refresh()
        {
            var partyManager = FindFirstObjectByType<PartyManager>();
            if (partyManager == null) return;

            for (int i = 0; i < slotPanels.Length; i++)
            {
                if (i < partyManager.Count)
                {
                    var creature = partyManager.Party[i];
                    slotPanels[i].SetActive(true);

                    if (nameLabels[i] != null)
                    {
                        nameLabels[i].text = creature.SpeciesName;
                        nameLabels[i].color = FieldFriendsPalette.MutedInk;
                    }
                    if (hpBars[i] != null)
                    {
                        hpBars[i].maxValue = creature.MaxHP;
                        hpBars[i].value = creature.CurrentHP;
                    }
                    if (typeLabels[i] != null)
                    {
                        typeLabels[i].text = creature.Type.ToString();
                        typeLabels[i].color = FieldFriendsPalette.LavenderBlue;
                    }
                    if (statusLabels[i] != null)
                    {
                        statusLabels[i].text = creature.IsResting ? "resting" : "";
                        statusLabels[i].color = FieldFriendsPalette.DustyRose;
                    }
                }
                else
                {
                    slotPanels[i].SetActive(false);
                }
            }
        }
    }
}
