using UnityEngine;
using FieldFriends.Data;
using FieldFriends.Party;
using FieldFriends.Battle;
using FieldFriends.World;
using FieldFriends.Save;

namespace FieldFriends.Core
{
    /// <summary>
    /// Central game state orchestrator.
    /// Manages transitions between Overworld, Battle, Paused, Dialogue states.
    /// Persists across scenes.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] GameState currentState = GameState.Overworld;

        public GameState CurrentState => currentState;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            currentState = state;
        }

        /// <summary>
        /// Start a battle with a wild creature by name.
        /// Called by EncounterSystem.
        /// </summary>
        public void StartBattle(string creatureName)
        {
            if (currentState != GameState.Overworld) return;

            var def = CreatureDatabase.FindByName(creatureName);
            if (def == null) return;

            CreatureInstance wildCreature = CreatureInstance.WildFromDef(def.Value);

            currentState = GameState.Battle;

            // Lock player movement
            var player = FindFirstObjectByType<GridMovement>();
            if (player != null) player.SetMovementLocked(true);

            // Start battle
            var battleManager = FindFirstObjectByType<BattleManager>();
            var partyManager = FindFirstObjectByType<PartyManager>();

            if (battleManager != null && partyManager != null)
            {
                battleManager.StartBattle(wildCreature, partyManager);
            }
        }

        void OnBattleEnded(bool won)
        {
            currentState = GameState.Overworld;

            // Unlock player movement
            var player = FindFirstObjectByType<GridMovement>();
            if (player != null) player.SetMovementLocked(false);

            // On loss: return to Willow End
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
                        areaManager.TransitionTo(AreaID.WillowEnd, new Vector2Int(10, 9));
                    }
                }
            }
        }

        /// <summary>
        /// Initialize a new game with a starter creature.
        /// </summary>
        public void StartNewGame()
        {
            var partyManager = FindFirstObjectByType<PartyManager>();
            if (partyManager == null) return;

            // Start with Mossbit as the first companion
            var mossbit = CreatureDatabase.FindByName("Mossbit");
            if (mossbit != null)
            {
                partyManager.AddToParty(CreatureInstance.FromDef(mossbit.Value));
            }

            currentState = GameState.Overworld;
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
                foreach (var cData in data.Party)
                {
                    partyManager.AddToParty(cData.ToInstance());
                }
            }

            // Restore area and position
            var areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager != null)
            {
                areaManager.TransitionTo(
                    data.CurrentArea,
                    new Vector2Int(data.PlayerGridX, data.PlayerGridY));
            }

            currentState = GameState.Overworld;
        }
    }
}
