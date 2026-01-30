using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using FieldFriends.Data;
using FieldFriends.Core;

namespace FieldFriends.World
{
    /// <summary>
    /// Manages area transitions and tracks the current area.
    /// Triggers autosave on area change.
    /// </summary>
    public class AreaManager : MonoBehaviour
    {
        public static event Action<AreaID> OnAreaChanged;

        [SerializeField] AreaID startingArea = AreaID.WillowEnd;

        AreaID _currentArea;
        bool _transitioning;

        public AreaID CurrentArea => _currentArea;

        void Awake()
        {
            _currentArea = startingArea;
        }

        public void TransitionTo(AreaID targetArea, Vector2Int spawnPos)
        {
            if (_transitioning) return;
            _transitioning = true;

            _currentArea = targetArea;
            OnAreaChanged?.Invoke(_currentArea);

            // Autosave on area change
            Save.SaveManager saveManager = FindFirstObjectByType<Save.SaveManager>();
            if (saveManager != null)
            {
                saveManager.AutoSave();
            }

            var areaDef = AreaDatabase.Get(targetArea);
            string sceneName = areaDef.Name.Replace(" ", "");

            // Load the scene, then position the player
            SceneManager.LoadScene(sceneName);

            // After scene loads, reposition player
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                GridMovement player = FindFirstObjectByType<GridMovement>();
                if (player != null)
                {
                    player.SetGridPosition(spawnPos);
                }
                _transitioning = false;
            };
        }

        public AreaDatabase.AreaDef GetCurrentAreaDef()
        {
            return AreaDatabase.Get(_currentArea);
        }
    }
}
