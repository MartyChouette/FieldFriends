using UnityEngine;
using FieldFriends.Data;
using FieldFriends.Party;
using FieldFriends.Battle;
using FieldFriends.World;
using FieldFriends.Save;
using FieldFriends.UI;

namespace FieldFriends.Core
{
    /// <summary>
    /// Central game state orchestrator.
    /// Manages transitions between Overworld, Battle, Paused, Dialogue states.
    /// Coordinates AreaSceneSetup, ScreenTransition, and all managers.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        GameState _currentState = GameState.Overworld;

        public GameState CurrentState => _currentState;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            BattleManager.OnBattleEnded += OnBattleEnded;
        }

        void OnDisable()
        {
            BattleManager.OnBattleEnded -= OnBattleEnded;
        }

        public void SetState(GameState state)
        {
            _currentState = state;
        }

        /// <summary>
        /// Start a battle with a wild creature by name.
        /// Called by EncounterSystem or QuietGroveEncounter.
        /// </summary>
        public void StartBattle(string creatureName)
        {
            if (_currentState != GameState.Overworld) return;

            var def = CreatureDatabase.FindByName(creatureName);
            if (def == null) return;

            CreatureInstance wildCreature = CreatureInstance.WildFromDef(def.Value);
            _currentState = GameState.Battle;

            // Lock player movement
            var player = FindFirstObjectByType<GridMovement>();
            if (player != null) player.SetMovementLocked(true);

            // Screen transition into battle
            if (ScreenTransition.Instance != null)
            {
                ScreenTransition.Instance.DoTransition(() =>
                {
                    BeginBattle(wildCreature);
                });
            }
            else
            {
                BeginBattle(wildCreature);
            }
        }

        void BeginBattle(CreatureInstance wildCreature)
        {
            var battleManager = FindFirstObjectByType<BattleManager>();
            var partyManager = FindFirstObjectByType<PartyManager>();

            if (battleManager != null && partyManager != null)
            {
                battleManager.gameObject.SetActive(true);
                battleManager.StartBattle(wildCreature, partyManager);
            }
        }

        void OnBattleEnded(bool won)
        {
            _currentState = GameState.Overworld;

            // Hide battle UI
            var battleManager = FindFirstObjectByType<BattleManager>();
            if (battleManager != null)
                battleManager.gameObject.SetActive(false);

            // Unlock player movement
            var player = FindFirstObjectByType<GridMovement>();
            if (player != null) player.SetMovementLocked(false);

            if (!won)
            {
                var partyManager = FindFirstObjectByType<PartyManager>();
                if (partyManager != null && partyManager.AllResting())
                {
                    // Heal all and warp home
                    partyManager.HealAll();
                    var areaManager = FindFirstObjectByType<AreaManager>();
                    if (areaManager != null)
                    {
                        areaManager.TransitionTo(AreaID.WillowEnd,
                            new Vector2Int(10, 5));
                    }
                }
            }
        }

        /// <summary>
        /// Initialize a new game with a starter creature.
        /// </summary>
        public void StartNewGame()
        {
            // Clear any existing save
            var saveManager = FindFirstObjectByType<SaveManager>();
            if (saveManager != null) saveManager.DeleteSave();

            // Set up party with Mossbit
            var partyManager = FindFirstObjectByType<PartyManager>();
            if (partyManager != null)
            {
                partyManager.ClearParty();
                var mossbit = CreatureDatabase.FindByName("Mossbit");
                if (mossbit != null)
                    partyManager.AddToParty(CreatureInstance.FromDef(mossbit.Value));
            }

            _currentState = GameState.Overworld;

            // Set up the starting area
            var areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager != null)
            {
                areaManager.TransitionTo(AreaID.WillowEnd,
                    new Vector2Int(10, 5));
            }

            // Play town music
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.PlayTrack(
                    Audio.AudioManager.MusicTrack.Town);
        }

        /// <summary>
        /// Load from save file.
        /// </summary>
        public void LoadGame()
        {
            var saveManager = FindFirstObjectByType<SaveManager>();
            if (saveManager == null || !saveManager.HasSave()) return;

            SaveData data = saveManager.LoadSave();
            if (data == null) return;

            // Restore party
            var partyManager = FindFirstObjectByType<PartyManager>();
            if (partyManager != null)
            {
                partyManager.ClearParty();
                foreach (var cData in data.Party)
                    partyManager.AddToParty(cData.ToInstance());
            }

            _currentState = GameState.Overworld;

            // Restore area and position
            var areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager != null)
            {
                areaManager.TransitionTo(
                    data.CurrentArea,
                    new Vector2Int(data.PlayerGridX, data.PlayerGridY));
            }
        }
    }
}
