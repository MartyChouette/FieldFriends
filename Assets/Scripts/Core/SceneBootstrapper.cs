using UnityEngine;
using UnityEngine.SceneManagement;
using FieldFriends.World;
using FieldFriends.Party;
using FieldFriends.Battle;
using FieldFriends.Save;
using FieldFriends.Audio;
using FieldFriends.UI;

namespace FieldFriends.Core
{
    /// <summary>
    /// Bootstraps the entire game at runtime. Creates all required
    /// managers, UI, and persistent objects so the game runs from
    /// a single empty scene. Attach this to a single GameObject
    /// in the startup scene.
    ///
    /// Object hierarchy created:
    ///   [GameRoot] (DontDestroyOnLoad)
    ///     - GameManager
    ///     - InputManager
    ///     - ScreenTransition
    ///     - AudioManager
    ///     - SaveManager
    ///     - PartyManager
    ///     - AreaManager
    ///     - EncounterSystem
    ///   [UIRoot] (DontDestroyOnLoad)
    ///     - BattleManager + BattleUI
    ///     - PauseMenu
    ///     - DialogueBox
    ///     - FriendsUI
    ///     - MapUI
    /// </summary>
    public class SceneBootstrapper : MonoBehaviour
    {
        void Awake()
        {
            // Prevent double-bootstrap
            if (FindFirstObjectByType<GameManager>() != null)
            {
                Destroy(gameObject);
                return;
            }

            CreateGameRoot();
            CreateUIRoot();

            // Boot to title screen
            var titleScreen = FindFirstObjectByType<TitleScreen>();
            if (titleScreen != null)
            {
                titleScreen.Show();
            }
        }

        void CreateGameRoot()
        {
            var root = new GameObject("[GameRoot]");
            DontDestroyOnLoad(root);

            root.AddComponent<GameManager>();
            root.AddComponent<InputManager>();
            root.AddComponent<ScreenTransition>();
            root.AddComponent<AudioManager>();
            root.AddComponent<SaveManager>();
            root.AddComponent<PartyManager>();

            var areaManager = root.AddComponent<AreaManager>();
            root.AddComponent<EncounterSystem>();
        }

        void CreateUIRoot()
        {
            var uiRoot = new GameObject("[UIRoot]");
            DontDestroyOnLoad(uiRoot);

            // Canvas for all game UI
            var canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = uiRoot.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(160, 144);
            scaler.matchWidthOrHeight = 0f;

            uiRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Battle system
            var battleObj = CreateChild(uiRoot, "Battle");
            var battleManager = battleObj.AddComponent<BattleManager>();
            var battleUI = battleObj.AddComponent<BattleUI>();
            battleObj.SetActive(false);

            // Dialogue box
            var dialogueObj = CreateChild(uiRoot, "DialogueBox");
            dialogueObj.AddComponent<DialogueBox>();

            // Pause menu
            var pauseObj = CreateChild(uiRoot, "PauseMenu");
            pauseObj.AddComponent<PauseMenu>();

            // Friends UI (child of pause)
            var friendsObj = CreateChild(pauseObj, "FriendsUI");
            friendsObj.AddComponent<FriendsUI>();

            // Map UI (child of pause)
            var mapObj = CreateChild(pauseObj, "MapUI");
            mapObj.AddComponent<MapUI>();

            // Title screen
            var titleObj = CreateChild(uiRoot, "TitleScreen");
            titleObj.AddComponent<TitleScreen>();
        }

        static GameObject CreateChild(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }
    }
}
