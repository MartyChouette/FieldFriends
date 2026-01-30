using UnityEngine;
using FieldFriends.Data;
using FieldFriends.Core;

namespace FieldFriends.World
{
    /// <summary>
    /// Placed at area edges. When the player steps on this tile,
    /// triggers an area transition. Autosave happens on transition.
    /// </summary>
    public class AreaTransition : MonoBehaviour
    {
        [Header("Destination")]
        [SerializeField] AreaID targetArea;
        [SerializeField] Vector2Int spawnGridPosition;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            AreaManager areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager != null)
            {
                areaManager.TransitionTo(targetArea, spawnGridPosition);
            }
        }
    }
}
