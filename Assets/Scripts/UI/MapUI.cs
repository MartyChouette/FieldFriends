using UnityEngine;
using UnityEngine.UI;
using FieldFriends.Data;
using FieldFriends.World;
using FieldFriends.Palette;

namespace FieldFriends.UI
{
    /// <summary>
    /// Minimal, node-based map. Player position shown as filled square.
    /// No legend, no cursor animation.
    /// </summary>
    public class MapUI : MonoBehaviour
    {
        [Header("Node Positions (UI anchored)")]
        [SerializeField] RectTransform[] areaNodes = new RectTransform[7];
        [SerializeField] Text[] areaLabels = new Text[7];
        [SerializeField] Image[] nodeImages = new Image[7];

        [Header("Player Marker")]
        [SerializeField] Image playerMarker;

        // Area order matches AreaID enum:
        // 0=WillowEnd, 1=SouthField, 2=CreekPath, 3=HillRoad,
        // 4=Stonebridge, 5=NorthMeadow, 6=QuietGrove

        public void Refresh()
        {
            var areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager == null) return;

            AreaID current = areaManager.CurrentArea;

            for (int i = 0; i < areaNodes.Length; i++)
            {
                if (areaNodes[i] == null) continue;

                AreaID id = (AreaID)i;
                bool isCurrent = id == current;

                // Highlight current area node
                if (nodeImages[i] != null)
                {
                    nodeImages[i].color = isCurrent
                        ? FieldFriendsPalette.MutedInk
                        : FieldFriendsPalette.PastelMint;
                }

                if (areaLabels[i] != null)
                {
                    areaLabels[i].text = AreaDatabase.Get(id).Name;
                    areaLabels[i].color = FieldFriendsPalette.MutedInk;
                }

                // Position the player marker on the current node
                if (isCurrent && playerMarker != null)
                {
                    playerMarker.rectTransform.position = areaNodes[i].position;
                }
            }
        }
    }
}
