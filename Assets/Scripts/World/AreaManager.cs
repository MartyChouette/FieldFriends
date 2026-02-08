using UnityEngine;
using System;
using System.Collections;
using FieldFriends.Data;
using FieldFriends.Core;

namespace FieldFriends.World
{
    /// <summary>
    /// Manages area transitions and tracks the current area.
    /// Uses AreaSceneSetup for procedural map generation instead of
    /// scene loading. Triggers autosave on area change.
    /// </summary>
    public class AreaManager : MonoBehaviour
    {
        public static event Action<AreaID> OnAreaChanged;

        AreaID _currentArea = AreaID.WillowEnd;
        bool _transitioning;

        public AreaID CurrentArea => _currentArea;

        public void TransitionTo(AreaID targetArea, Vector2Int spawnPos)
        {
            if (_transitioning) return;
            StartCoroutine(TransitionRoutine(targetArea, spawnPos));
        }

        IEnumerator TransitionRoutine(AreaID targetArea, Vector2Int spawnPos)
        {
            _transitioning = true;

            // Fade out
            if (ScreenTransition.Instance != null)
                yield return ScreenTransition.Instance.FadeOut();

            _currentArea = targetArea;

            // Build the area using AreaSceneSetup
            var setup = AreaSceneSetup.Instance;
            if (setup == null)
            {
                var obj = new GameObject("AreaSceneSetup");
                setup = obj.AddComponent<AreaSceneSetup>();
            }
            setup.SetupArea(targetArea, spawnPos);

            // Notify listeners (audio, encounter system, etc.)
            OnAreaChanged?.Invoke(_currentArea);

            // Autosave
            var saveManager = FindFirstObjectByType<Save.SaveManager>();
            if (saveManager != null)
                saveManager.AutoSave();

            // Fade in
            if (ScreenTransition.Instance != null)
                yield return ScreenTransition.Instance.FadeIn();

            _transitioning = false;
        }

        public AreaDatabase.AreaDef GetCurrentAreaDef()
        {
            return AreaDatabase.Get(_currentArea);
        }
    }
}
