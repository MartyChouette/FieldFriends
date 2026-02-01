using UnityEngine;
using FieldFriends.Core;
using FieldFriends.Data;
using FieldFriends.Party;
using FieldFriends.UI;
using FieldFriends.Palette;

namespace FieldFriends.World
{
    /// <summary>
    /// Sets up a complete playable area at runtime:
    /// - Generates the tilemap
    /// - Spawns the player
    /// - Places NPCs with their dialogue
    /// - Places area transition triggers
    /// - Places the Quiet Grove Still encounter
    /// - Configures the camera
    ///
    /// Called by GameManager when entering/loading an area.
    /// </summary>
    public class AreaSceneSetup : MonoBehaviour
    {
        public static AreaSceneSetup Instance { get; private set; }

        ProceduralMapGenerator _mapGen;
        GameObject _playerObj;
        GameObject _npcContainer;
        GameObject _transitionContainer;

        void Awake()
        {
            Instance = this;
        }

        public void SetupArea(AreaID area, Vector2Int? overrideSpawn = null)
        {
            CleanupPreviousArea();

            // Initialize map generator if needed
            if (_mapGen == null)
            {
                var genObj = new GameObject("MapGenerator");
                _mapGen = genObj.AddComponent<ProceduralMapGenerator>();
                _mapGen.Initialize();
            }

            // Generate the tilemap
            _mapGen.Generate(area);

            Vector2Int spawn = overrideSpawn ?? _mapGen.PlayerSpawn;

            // Spawn or reposition player
            EnsurePlayer(spawn);

            // Place NPCs
            PlaceNPCs(area);

            // Place area transitions
            PlaceTransitions(area);

            // Special: Quiet Grove Still encounter
            if (area == AreaID.QuietGrove)
            {
                PlaceStillEncounter();
            }

            // Configure camera
            var cam = FindFirstObjectByType<CameraController>();
            if (cam == null)
            {
                var camObj = Camera.main != null ? Camera.main.gameObject : new GameObject("MainCamera");
                if (camObj.GetComponent<Camera>() == null)
                    camObj.AddComponent<Camera>();
                cam = camObj.AddComponent<CameraController>();
            }
            cam.SetTarget(_playerObj.transform);
        }

        void CleanupPreviousArea()
        {
            // Destroy old NPCs and transitions
            if (_npcContainer != null) Destroy(_npcContainer);
            if (_transitionContainer != null) Destroy(_transitionContainer);
        }

        void EnsurePlayer(Vector2Int spawn)
        {
            if (_playerObj == null)
            {
                _playerObj = GameObject.FindWithTag("Player");
            }

            if (_playerObj == null)
            {
                _playerObj = new GameObject("Player");
                _playerObj.tag = "Player";

                // Sprite renderer
                var sr = _playerObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;
                var sprite = Resources.Load<Sprite>("Characters/player_down");
                if (sprite != null) sr.sprite = sprite;
                sr.color = Color.white;

                // Physics
                var rb = _playerObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0;

                var col = _playerObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.8f, 0.8f);
                col.isTrigger = false;

                // Grid movement
                _playerObj.AddComponent<GridMovement>();

                // Ability effects
                var abilityEffects = _playerObj.AddComponent<AbilityEffects>();

                DontDestroyOnLoad(_playerObj);
            }

            // Position player
            var movement = _playerObj.GetComponent<GridMovement>();
            if (movement != null)
            {
                movement.SetGridPosition(spawn);
            }
            else
            {
                _playerObj.transform.position = new Vector3(
                    spawn.x + 0.5f, spawn.y + 0.5f, 0);
            }
        }

        void PlaceNPCs(AreaID area)
        {
            _npcContainer = new GameObject("NPCs");

            switch (area)
            {
                case AreaID.WillowEnd:
                    CreateNPC("Pond Person", new Vector2Int(13, 12),
                        "The water's been calm lately.");
                    CreateNPC("House Person", new Vector2Int(6, 6),
                        "You heading out again?");
                    break;

                case AreaID.SouthField:
                    CreateNPC("Field Walker", new Vector2Int(15, 9),
                        "It's easy to lose track of time here.");
                    break;

                case AreaID.Stonebridge:
                    CreateNPC("Bridge Watcher", new Vector2Int(7, 7),
                        "This bridge has always been here.");
                    break;

                case AreaID.NorthMeadow:
                    CreateNPC("Meadow Sitter", new Vector2Int(12, 9),
                        "Things grow better when you leave them alone.");
                    break;

                case AreaID.QuietGrove:
                    CreateNPC("Grove Person", new Vector2Int(10, 14),
                        "You can stay if you want.");
                    break;
            }
        }

        void CreateNPC(string npcName, Vector2Int pos, string dialogue)
        {
            var npcObj = new GameObject($"NPC_{npcName}");
            npcObj.transform.SetParent(_npcContainer.transform);
            npcObj.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
            npcObj.tag = "NPC";

            // Sprite
            var sr = npcObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            var sprite = Resources.Load<Sprite>("Characters/npc");
            if (sprite != null) sr.sprite = sprite;

            // Collider for interaction trigger
            var col = npcObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 1.5f);
            col.isTrigger = true;

            // Dialogue component
            var npcDialogue = npcObj.AddComponent<NPCDialogue>();
            npcDialogue.SetDialogue(dialogue);

            // Collision body (NPCs block movement)
            var rb = npcObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            var bodyCol = npcObj.AddComponent<BoxCollider2D>();
            bodyCol.size = new Vector2(0.8f, 0.8f);
            bodyCol.isTrigger = false;
        }

        void PlaceTransitions(AreaID area)
        {
            _transitionContainer = new GameObject("Transitions");

            var areaDef = AreaDatabase.Get(area);
            foreach (var conn in areaDef.Connections)
            {
                Vector2Int pos;
                Vector2Int spawnInTarget;
                GetTransitionPoints(area, conn, out pos, out spawnInTarget);
                CreateTransition(conn, pos, spawnInTarget);
            }
        }

        void GetTransitionPoints(AreaID from, AreaID to,
            out Vector2Int exitPos, out Vector2Int entryPos)
        {
            // Default positions for transitions between areas
            // Exit at edge of current map, enter at opposite edge of target
            exitPos = new Vector2Int(10, 0);
            entryPos = new Vector2Int(10, 16);

            switch (from)
            {
                case AreaID.WillowEnd:
                    if (to == AreaID.SouthField)
                    {
                        exitPos = new Vector2Int(10, 0); // south exit
                        entryPos = new Vector2Int(10, 16);
                    }
                    else if (to == AreaID.CreekPath)
                    {
                        exitPos = new Vector2Int(0, 9); // west exit
                        entryPos = new Vector2Int(18, 9);
                    }
                    break;

                case AreaID.SouthField:
                    if (to == AreaID.WillowEnd)
                    {
                        exitPos = new Vector2Int(10, 17); // north exit
                        entryPos = new Vector2Int(10, 2);
                    }
                    else if (to == AreaID.HillRoad)
                    {
                        exitPos = new Vector2Int(10, 0); // south exit
                        entryPos = new Vector2Int(10, 16);
                    }
                    break;

                case AreaID.CreekPath:
                    if (to == AreaID.WillowEnd)
                    {
                        exitPos = new Vector2Int(19, 9); // east exit
                        entryPos = new Vector2Int(1, 9);
                    }
                    else if (to == AreaID.HillRoad)
                    {
                        exitPos = new Vector2Int(10, 0);
                        entryPos = new Vector2Int(10, 16);
                    }
                    break;

                case AreaID.HillRoad:
                    if (to == AreaID.SouthField)
                    {
                        exitPos = new Vector2Int(10, 17);
                        entryPos = new Vector2Int(10, 2);
                    }
                    else if (to == AreaID.CreekPath)
                    {
                        exitPos = new Vector2Int(10, 17);
                        entryPos = new Vector2Int(10, 2);
                    }
                    else if (to == AreaID.Stonebridge)
                    {
                        exitPos = new Vector2Int(10, 0);
                        entryPos = new Vector2Int(10, 16);
                    }
                    break;

                case AreaID.Stonebridge:
                    if (to == AreaID.HillRoad)
                    {
                        exitPos = new Vector2Int(10, 17);
                        entryPos = new Vector2Int(10, 2);
                    }
                    else if (to == AreaID.NorthMeadow)
                    {
                        exitPos = new Vector2Int(10, 0);
                        entryPos = new Vector2Int(10, 16);
                    }
                    break;

                case AreaID.NorthMeadow:
                    if (to == AreaID.Stonebridge)
                    {
                        exitPos = new Vector2Int(10, 17);
                        entryPos = new Vector2Int(10, 2);
                    }
                    else if (to == AreaID.QuietGrove)
                    {
                        exitPos = new Vector2Int(10, 0);
                        entryPos = new Vector2Int(10, 16);
                    }
                    break;

                case AreaID.QuietGrove:
                    if (to == AreaID.NorthMeadow)
                    {
                        exitPos = new Vector2Int(10, 17);
                        entryPos = new Vector2Int(10, 2);
                    }
                    break;
            }
        }

        void CreateTransition(AreaID targetArea, Vector2Int pos, Vector2Int spawnInTarget)
        {
            var obj = new GameObject($"Transition_to_{targetArea}");
            obj.transform.SetParent(_transitionContainer.transform);
            obj.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
            obj.tag = "Transition";

            var col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 1f); // wide trigger zone
            col.isTrigger = true;

            var transition = obj.AddComponent<AreaTransition>();
            transition.SetTarget(targetArea, spawnInTarget);
        }

        void PlaceStillEncounter()
        {
            var stillObj = new GameObject("StillEncounter");
            stillObj.transform.position = new Vector3(10.5f, 10.5f, 0);

            var col = stillObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 2f);
            col.isTrigger = true;

            stillObj.AddComponent<QuietGroveEncounter>();

            // Visual hint: a faint sprite
            var sr = stillObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            var sprite = Resources.Load<Sprite>("Creatures/still");
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.color = new Color(1f, 1f, 1f, 0.3f); // faint
            }
        }
    }
}
