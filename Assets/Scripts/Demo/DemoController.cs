using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using FieldFriends.Core;
using FieldFriends.Data;
using FieldFriends.Party;
using FieldFriends.World;
using FieldFriends.Save;
using FieldFriends.Audio;
using FieldFriends.UI;
using FieldFriends.Palette;

namespace FieldFriends.Demo
{
    /// <summary>
    /// Interactive demo hub overlay. Creates its own Canvas above the game UI.
    /// Provides numbered sections to showcase every game system.
    /// Navigate with Up/Down arrows, Z to select, X to go back, Tab to toggle.
    /// </summary>
    public class DemoController : MonoBehaviour
    {
        Canvas _canvas;
        GameObject _menuPanel;
        Text _titleText;
        Text _infoText;
        Text[] _menuItemTexts;
        GameObject _activeSubPanel;

        int _menuIndex;
        bool _menuVisible = true;
        bool _sectionActive;
        Coroutine _activeCoroutine;

        static readonly string[] MenuLabels = new string[]
        {
            "1. CREATURES",
            "2. AREAS",
            "3. BATTLE",
            "4. PARTY",
            "5. TYPE CHART",
            "6. MENUS",
            "7. DIALOGUE",
            "8. SAVE/LOAD",
            "9. TRANSITIONS",
            "10. AUDIO",
            "11. ENDING",
            "0. FREE ROAM"
        };

        void Start()
        {
            BuildDemoCanvas();
            ShowMenu();
        }

        void Update()
        {
            // Toggle menu with Tab
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_sectionActive)
                {
                    CloseActiveSection();
                }
                else
                {
                    _menuVisible = !_menuVisible;
                    _menuPanel.SetActive(_menuVisible);
                }
                return;
            }

            if (!_menuVisible || _sectionActive) return;

            // Navigation
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _menuIndex = Mathf.Max(0, _menuIndex - 1);
                UpdateMenuHighlight();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _menuIndex = Mathf.Min(MenuLabels.Length - 1, _menuIndex + 1);
                UpdateMenuHighlight();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            }

            // Select
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                SelectSection(_menuIndex);
            }

            // Dismiss with X
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
            {
                _menuVisible = false;
                _menuPanel.SetActive(false);
            }
        }

        // --- UI Building ---

        void BuildDemoCanvas()
        {
            var canvasObj = new GameObject("DemoCanvas");
            canvasObj.transform.SetParent(transform);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(160, 144);
            scaler.matchWidthOrHeight = 0f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Menu panel - semi-transparent background
            _menuPanel = new GameObject("MenuPanel");
            _menuPanel.transform.SetParent(canvasObj.transform, false);
            var panelImg = _menuPanel.AddComponent<Image>();
            panelImg.color = new Color(
                FieldFriendsPalette.CreamMist.r,
                FieldFriendsPalette.CreamMist.g,
                FieldFriendsPalette.CreamMist.b,
                0.92f);
            var panelRect = panelImg.rectTransform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Title
            _titleText = CreateText(_menuPanel, "Title", "FIELD FRIENDS DEMO",
                new Vector2(0.5f, 0.93f), 9);
            _titleText.color = FieldFriendsPalette.MutedInk;
            _titleText.fontStyle = FontStyle.Bold;

            // Menu items
            _menuItemTexts = new Text[MenuLabels.Length];
            for (int i = 0; i < MenuLabels.Length; i++)
            {
                float y = 0.83f - i * 0.065f;
                _menuItemTexts[i] = CreateText(_menuPanel, $"Item{i}", MenuLabels[i],
                    new Vector2(0.5f, y), 6);
                _menuItemTexts[i].alignment = TextAnchor.MiddleLeft;
                _menuItemTexts[i].rectTransform.anchorMin = new Vector2(0.15f, y);
                _menuItemTexts[i].rectTransform.anchorMax = new Vector2(0.15f, y);
            }

            // Info bar
            _infoText = CreateText(_menuPanel, "Info",
                "Arrow keys + Z to select, X to go back, Tab to toggle menu",
                new Vector2(0.5f, 0.04f), 4);
            _infoText.color = FieldFriendsPalette.LavenderBlue;

            UpdateMenuHighlight();
        }

        void ShowMenu()
        {
            _menuVisible = true;
            _menuPanel.SetActive(true);
            UpdateMenuHighlight();
        }

        void UpdateMenuHighlight()
        {
            for (int i = 0; i < _menuItemTexts.Length; i++)
            {
                _menuItemTexts[i].color = i == _menuIndex
                    ? FieldFriendsPalette.MutedInk
                    : FieldFriendsPalette.LavenderBlue;
            }
        }

        void SelectSection(int index)
        {
            switch (index)
            {
                case 0: ShowCreatures(); break;
                case 1: ShowAreas(); break;
                case 2: ShowBattle(); break;
                case 3: ShowParty(); break;
                case 4: ShowTypeChart(); break;
                case 5: ShowMenus(); break;
                case 6: ShowDialogue(); break;
                case 7: ShowSaveLoad(); break;
                case 8: ShowTransitions(); break;
                case 9: ShowAudio(); break;
                case 10: ShowEnding(); break;
                case 11: FreeRoam(); break;
            }
        }

        void CloseActiveSection()
        {
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
                _activeCoroutine = null;
            }
            if (_activeSubPanel != null)
            {
                Destroy(_activeSubPanel);
                _activeSubPanel = null;
            }
            _sectionActive = false;
            ShowMenu();
        }

        GameObject CreateSubPanel(string title)
        {
            _menuPanel.SetActive(false);
            _sectionActive = true;

            var panel = new GameObject("SubPanel");
            panel.transform.SetParent(_canvas.transform, false);
            var img = panel.AddComponent<Image>();
            img.color = new Color(
                FieldFriendsPalette.CreamMist.r,
                FieldFriendsPalette.CreamMist.g,
                FieldFriendsPalette.CreamMist.b,
                0.95f);
            var rect = img.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Title
            var titleText = CreateText(panel, "Title", title,
                new Vector2(0.5f, 0.95f), 8);
            titleText.color = FieldFriendsPalette.MutedInk;
            titleText.fontStyle = FontStyle.Bold;

            // Back hint
            var backText = CreateText(panel, "BackHint", "X / Tab to go back",
                new Vector2(0.5f, 0.02f), 4);
            backText.color = FieldFriendsPalette.LavenderBlue;

            _activeSubPanel = panel;
            return panel;
        }

        // --- SECTION 1: CREATURES ---

        void ShowCreatures()
        {
            var panel = CreateSubPanel("CREATURES");
            int scrollOffset = 0;
            var contentTexts = new List<Text>();

            RenderCreatureList(panel, contentTexts, scrollOffset);
            _activeCoroutine = StartCoroutine(CreatureScrollRoutine(panel, contentTexts));
        }

        void RenderCreatureList(GameObject panel, List<Text> texts, int offset)
        {
            // Clear old texts
            foreach (var t in texts)
                if (t != null) Destroy(t.gameObject);
            texts.Clear();

            var creatures = CreatureDatabase.All;
            int visibleCount = 6;
            int max = Mathf.Min(offset + visibleCount, creatures.Length);

            for (int i = offset; i < max; i++)
            {
                var def = creatures[i];
                float y = 0.85f - (i - offset) * 0.13f;

                // Name + Type
                string typeColor = GetTypeColorHex(def.Type);
                var nameText = CreateText(panel, $"C{i}Name",
                    $"{def.Name}  <color={typeColor}>[{def.Type}]</color>",
                    new Vector2(0.1f, y), 6);
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.supportRichText = true;
                nameText.color = FieldFriendsPalette.MutedInk;
                texts.Add(nameText);

                // Stats
                var statsText = CreateText(panel, $"C{i}Stats",
                    $"HP:{def.HP * 3}  ATK:{def.ATK}  DEF:{def.DEF}  SPD:{def.SPD}",
                    new Vector2(0.1f, y - 0.035f), 4);
                statsText.alignment = TextAnchor.MiddleLeft;
                statsText.color = FieldFriendsPalette.EveningLilac;
                texts.Add(statsText);

                // Ability + idle text
                string abilityStr = def.Ability.ToString();
                if (def.HasUpgrade)
                    abilityStr += $" -> {def.UpgradedAbility}";
                var abilityText = CreateText(panel, $"C{i}Ability",
                    $"{abilityStr}  |  {def.IdleText}",
                    new Vector2(0.1f, y - 0.065f), 4);
                abilityText.alignment = TextAnchor.MiddleLeft;
                abilityText.color = FieldFriendsPalette.LavenderBlue;
                texts.Add(abilityText);
            }

            // Scroll indicator
            string scrollInfo = $"Showing {offset + 1}-{max} of {creatures.Length}  (Up/Down to scroll)";
            var scrollText = CreateText(panel, "ScrollInfo", scrollInfo,
                new Vector2(0.5f, 0.06f), 4);
            scrollText.color = FieldFriendsPalette.LavenderBlue;
            texts.Add(scrollText);
        }

        IEnumerator CreatureScrollRoutine(GameObject panel, List<Text> texts)
        {
            int offset = 0;
            int maxOffset = Mathf.Max(0, CreatureDatabase.All.Length - 6);

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    if (offset < maxOffset)
                    {
                        offset++;
                        RenderCreatureList(panel, texts, offset);
                    }
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    if (offset > 0)
                    {
                        offset--;
                        RenderCreatureList(panel, texts, offset);
                    }
                }
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)
                    || Input.GetKeyDown(KeyCode.Tab))
                {
                    CloseActiveSection();
                    yield break;
                }
                yield return null;
            }
        }

        // --- SECTION 2: AREAS ---

        void ShowAreas()
        {
            _menuPanel.SetActive(false);
            _sectionActive = true;
            _activeCoroutine = StartCoroutine(AreaTourRoutine());
        }

        IEnumerator AreaTourRoutine()
        {
            var areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager == null) { CloseActiveSection(); yield break; }

            AreaID[] tour = new AreaID[]
            {
                AreaID.WillowEnd, AreaID.SouthField, AreaID.CreekPath,
                AreaID.HillRoad, AreaID.Stonebridge, AreaID.NorthMeadow,
                AreaID.QuietGrove
            };

            // Create overlay for area info
            var infoPanel = new GameObject("AreaInfoPanel");
            infoPanel.transform.SetParent(_canvas.transform, false);
            var img = infoPanel.AddComponent<Image>();
            img.color = new Color(
                FieldFriendsPalette.CreamMist.r,
                FieldFriendsPalette.CreamMist.g,
                FieldFriendsPalette.CreamMist.b,
                0.85f);
            var infoRect = img.rectTransform;
            infoRect.anchorMin = new Vector2(0f, 0f);
            infoRect.anchorMax = new Vector2(1f, 0.2f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;
            _activeSubPanel = infoPanel;

            var areaNameText = CreateText(infoPanel, "AreaName", "",
                new Vector2(0.5f, 0.7f), 7);
            areaNameText.color = FieldFriendsPalette.MutedInk;

            var encounterText = CreateText(infoPanel, "Encounters", "",
                new Vector2(0.5f, 0.3f), 4);
            encounterText.color = FieldFriendsPalette.LavenderBlue;

            for (int i = 0; i < tour.Length; i++)
            {
                var areaDef = AreaDatabase.Get(tour[i]);
                areaNameText.text = $"{i + 1}/7  {areaDef.Name}";

                if (areaDef.HasEncounters && areaDef.Encounters.Length > 0)
                {
                    string enc = "Encounters: ";
                    for (int e = 0; e < areaDef.Encounters.Length; e++)
                    {
                        if (e > 0) enc += ", ";
                        enc += $"{areaDef.Encounters[e].creatureName} ({areaDef.Encounters[e].rarity})";
                    }
                    enc += $"  Rate: {areaDef.EncounterRate:P0}";
                    encounterText.text = enc;
                }
                else
                {
                    encounterText.text = "No random encounters";
                }

                areaManager.TransitionTo(tour[i], new Vector2Int(10, 9));

                // Wait 3 seconds or Z to skip ahead
                float timer = 0f;
                while (timer < 3f)
                {
                    if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
                        break;
                    if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)
                        || Input.GetKeyDown(KeyCode.Tab))
                    {
                        Destroy(infoPanel);
                        _activeSubPanel = null;
                        _sectionActive = false;
                        ShowMenu();
                        yield break;
                    }
                    timer += Time.deltaTime;
                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);
            }

            // Return to WillowEnd
            areaManager.TransitionTo(AreaID.WillowEnd, new Vector2Int(10, 5));
            yield return new WaitForSeconds(0.5f);

            Destroy(infoPanel);
            _activeSubPanel = null;
            _sectionActive = false;
            ShowMenu();
        }

        // --- SECTION 3: BATTLE ---

        void ShowBattle()
        {
            _menuPanel.SetActive(false);
            _sectionActive = true;
            _activeCoroutine = StartCoroutine(BattleRoutine());
        }

        IEnumerator BattleRoutine()
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null) { CloseActiveSection(); yield break; }

            // Cycle through different battle matchups
            string[] enemies = new string[] { "Plen", "Skirl", "Bloomel" };
            string[] descriptions = new string[]
            {
                "Mossbit (Field) vs Plen (Water) - type advantage",
                "Mossbit (Field) vs Skirl (Wind) - type disadvantage",
                "Mossbit (Field) vs Bloomel (Meadow) - neutral"
            };

            for (int i = 0; i < enemies.Length; i++)
            {
                // Show info overlay
                var infoPanel = new GameObject("BattleInfo");
                infoPanel.transform.SetParent(_canvas.transform, false);
                var img = infoPanel.AddComponent<Image>();
                img.color = new Color(
                    FieldFriendsPalette.CreamMist.r,
                    FieldFriendsPalette.CreamMist.g,
                    FieldFriendsPalette.CreamMist.b,
                    0.9f);
                var rect = img.rectTransform;
                rect.anchorMin = new Vector2(0f, 0.85f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var infoText = CreateText(infoPanel, "Info",
                    $"Battle {i + 1}/3: {descriptions[i]}",
                    new Vector2(0.5f, 0.5f), 4);
                infoText.color = FieldFriendsPalette.MutedInk;

                // Heal party before each battle
                var partyManager = FindFirstObjectByType<PartyManager>();
                if (partyManager != null) partyManager.HealAll();

                // Ensure gamestate is overworld for StartBattle
                gameManager.SetState(GameState.Overworld);

                // Start battle
                gameManager.StartBattle(enemies[i]);

                // Wait for battle to end
                bool battleEnded = false;
                System.Action<bool> onEnded = (won) => battleEnded = true;
                BattleManager.OnBattleEnded += onEnded;

                while (!battleEnded)
                    yield return null;

                BattleManager.OnBattleEnded -= onEnded;

                Destroy(infoPanel);
                yield return new WaitForSeconds(0.5f);

                // Check if user wants to stop
                if (i < enemies.Length - 1)
                {
                    var promptPanel = new GameObject("NextPrompt");
                    promptPanel.transform.SetParent(_canvas.transform, false);
                    var pImg = promptPanel.AddComponent<Image>();
                    pImg.color = new Color(
                        FieldFriendsPalette.CreamMist.r,
                        FieldFriendsPalette.CreamMist.g,
                        FieldFriendsPalette.CreamMist.b,
                        0.9f);
                    var pRect = pImg.rectTransform;
                    pRect.anchorMin = new Vector2(0.2f, 0.4f);
                    pRect.anchorMax = new Vector2(0.8f, 0.6f);
                    pRect.offsetMin = Vector2.zero;
                    pRect.offsetMax = Vector2.zero;

                    var pText = CreateText(promptPanel, "Prompt",
                        "Z for next battle, X to stop",
                        new Vector2(0.5f, 0.5f), 5);
                    pText.color = FieldFriendsPalette.MutedInk;

                    while (true)
                    {
                        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
                        {
                            Destroy(promptPanel);
                            break;
                        }
                        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)
                            || Input.GetKeyDown(KeyCode.Tab))
                        {
                            Destroy(promptPanel);
                            _sectionActive = false;
                            ShowMenu();
                            yield break;
                        }
                        yield return null;
                    }
                }
            }

            _sectionActive = false;
            ShowMenu();
        }

        // --- SECTION 4: PARTY ---

        void ShowParty()
        {
            var panel = CreateSubPanel("PARTY");

            var partyManager = FindFirstObjectByType<PartyManager>();
            if (partyManager == null) return;

            // Show party info directly on the panel
            float y = 0.82f;
            foreach (var creature in partyManager.Party)
            {
                string typeColor = GetTypeColorHex(creature.Type);
                var nameText = CreateText(panel, $"P_{creature.SpeciesName}",
                    $"{creature.SpeciesName}  <color={typeColor}>[{creature.Type}]</color>",
                    new Vector2(0.1f, y), 6);
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.supportRichText = true;
                nameText.color = FieldFriendsPalette.MutedInk;

                var hpText = CreateText(panel, $"P_{creature.SpeciesName}_hp",
                    $"HP: {creature.CurrentHP}/{creature.MaxHP}  ATK:{creature.ATK}  DEF:{creature.DEF}  SPD:{creature.SPD}",
                    new Vector2(0.1f, y - 0.04f), 4);
                hpText.alignment = TextAnchor.MiddleLeft;
                hpText.color = FieldFriendsPalette.EveningLilac;

                string abilityStr = creature.ActiveAbility.ToString();
                if (creature.IsAbilityUpgraded)
                    abilityStr += " (UPGRADED)";
                var abilityText = CreateText(panel, $"P_{creature.SpeciesName}_ab",
                    $"Ability: {abilityStr}  Friendship: {creature.Friendship}",
                    new Vector2(0.1f, y - 0.075f), 4);
                abilityText.alignment = TextAnchor.MiddleLeft;
                abilityText.color = FieldFriendsPalette.LavenderBlue;

                y -= 0.16f;
            }

            // Info about friendship system
            var infoText = CreateText(panel, "FriendInfo",
                "Friendship grows by walking together (+1/step, +2 loyalty)\n" +
                "and using WAIT in battle (+3). Upgrade at 100.",
                new Vector2(0.5f, 0.15f), 4);
            infoText.color = FieldFriendsPalette.EveningLilac;

            // Also open FriendsUI if available
            var friendsUI = FindFirstObjectByType<FriendsUI>();
            if (friendsUI != null) friendsUI.Refresh();

            _activeCoroutine = StartCoroutine(WaitForDismiss());
        }

        // --- SECTION 5: TYPE CHART ---

        void ShowTypeChart()
        {
            var panel = CreateSubPanel("TYPE CHART");

            // Type cycle
            var cycleText = CreateText(panel, "Cycle",
                "Field  >  Water  >  Wind  >  Field",
                new Vector2(0.5f, 0.75f), 7);
            cycleText.color = FieldFriendsPalette.MutedInk;

            // Advantage arrows
            var advText = CreateText(panel, "Advantages",
                "Strong (1.5x):",
                new Vector2(0.5f, 0.62f), 5);
            advText.color = FieldFriendsPalette.MutedInk;

            string[] advantages = new string[]
            {
                "Field -> Water  (1.5x)",
                "Water -> Wind   (1.5x)",
                "Wind -> Field   (1.5x)",
            };
            for (int i = 0; i < advantages.Length; i++)
            {
                var t = CreateText(panel, $"Adv{i}", advantages[i],
                    new Vector2(0.5f, 0.55f - i * 0.06f), 5);
                t.color = FieldFriendsPalette.PaleTeal;
            }

            // Disadvantage
            var disText = CreateText(panel, "Disadvantages",
                "Weak (0.75x):",
                new Vector2(0.5f, 0.35f), 5);
            disText.color = FieldFriendsPalette.MutedInk;

            string[] disadvantages = new string[]
            {
                "Water -> Field  (0.75x)",
                "Wind -> Water   (0.75x)",
                "Field -> Wind   (0.75x)",
            };
            for (int i = 0; i < disadvantages.Length; i++)
            {
                var t = CreateText(panel, $"Dis{i}", disadvantages[i],
                    new Vector2(0.5f, 0.28f - i * 0.06f), 5);
                t.color = FieldFriendsPalette.DustyRose;
            }

            // Meadow note
            var meadowText = CreateText(panel, "Meadow",
                "Meadow is neutral against all types (1.0x)",
                new Vector2(0.5f, 0.1f), 5);
            meadowText.color = FieldFriendsPalette.ButterYellow;

            _activeCoroutine = StartCoroutine(WaitForDismiss());
        }

        // --- SECTION 6: MENUS ---

        void ShowMenus()
        {
            _menuPanel.SetActive(false);
            _sectionActive = true;

            // Open the pause menu directly
            var pauseMenu = FindFirstObjectByType<PauseMenu>();
            if (pauseMenu != null)
            {
                // Force overworld state so menu opens
                var gm = FindFirstObjectByType<GameManager>();
                if (gm != null) gm.SetState(GameState.Overworld);
                pauseMenu.Open();
            }

            _activeCoroutine = StartCoroutine(WaitForMenuClose());
        }

        IEnumerator WaitForMenuClose()
        {
            // Wait until pause menu closes
            yield return new WaitForSeconds(0.3f);
            while (true)
            {
                var pauseMenu = FindFirstObjectByType<PauseMenu>();
                if (pauseMenu == null) break;

                // Check if menu panel is inactive (menu was closed)
                // We detect close by checking the game state
                var gm = FindFirstObjectByType<GameManager>();
                if (gm != null && gm.CurrentState == GameState.Overworld)
                {
                    // Give a frame to let the close complete
                    yield return null;
                    if (gm.CurrentState == GameState.Overworld)
                        break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            _sectionActive = false;
            ShowMenu();
        }

        // --- SECTION 7: DIALOGUE ---

        void ShowDialogue()
        {
            _menuPanel.SetActive(false);
            _sectionActive = true;
            _activeCoroutine = StartCoroutine(DialogueRoutine());
        }

        IEnumerator DialogueRoutine()
        {
            var dialogueBox = FindFirstObjectByType<DialogueBox>();
            if (dialogueBox == null) { CloseActiveSection(); yield break; }

            string[] npcLines = new string[]
            {
                "The water's been calm lately.",
                "You heading out again?",
                "It's easy to lose track of time here.",
                "This bridge has always been here.",
                "Things grow better when you leave them alone.",
                "You can stay if you want."
            };

            for (int i = 0; i < npcLines.Length; i++)
            {
                yield return dialogueBox.ShowDialogue($"[NPC {i + 1}/6] {npcLines[i]}");
                yield return new WaitForSeconds(0.3f);
            }

            _sectionActive = false;
            ShowMenu();
        }

        // --- SECTION 8: SAVE/LOAD ---

        void ShowSaveLoad()
        {
            _menuPanel.SetActive(false);
            _sectionActive = true;
            _activeCoroutine = StartCoroutine(SaveLoadRoutine());
        }

        IEnumerator SaveLoadRoutine()
        {
            var saveManager = FindFirstObjectByType<SaveManager>();
            var dialogueBox = FindFirstObjectByType<DialogueBox>();
            if (saveManager == null || dialogueBox == null)
            {
                CloseActiveSection();
                yield break;
            }

            // Save
            saveManager.AutoSave();
            yield return dialogueBox.ShowDialogue("Game saved.");
            yield return new WaitForSeconds(0.3f);

            // Ask to load
            yield return dialogueBox.ShowDialogue("Press Z to load the save back.");
            yield return new WaitForSeconds(0.1f);

            // Load
            if (saveManager.HasSave())
            {
                var gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.LoadGame();
                    yield return new WaitForSeconds(1f);
                    yield return dialogueBox.ShowDialogue("Save loaded successfully.");
                }
            }

            yield return new WaitForSeconds(0.3f);
            _sectionActive = false;
            ShowMenu();
        }

        // --- SECTION 9: TRANSITIONS ---

        void ShowTransitions()
        {
            _menuPanel.SetActive(false);
            _sectionActive = true;
            _activeCoroutine = StartCoroutine(TransitionRoutine());
        }

        IEnumerator TransitionRoutine()
        {
            var transition = ScreenTransition.Instance;
            if (transition == null) { CloseActiveSection(); yield break; }

            var dialogueBox = FindFirstObjectByType<DialogueBox>();

            // Simple fade out/in
            if (dialogueBox != null)
                yield return dialogueBox.ShowDialogue("Fade out then in...");

            yield return transition.FadeOut();
            yield return new WaitForSeconds(0.3f);
            yield return transition.FadeIn();
            yield return new WaitForSeconds(0.5f);

            // Full transition with mid-action
            if (dialogueBox != null)
                yield return dialogueBox.ShowDialogue("Full transition with mid-action...");

            yield return transition.DoTransition(() =>
            {
                // Mid-action: nothing visible, just demonstrates the pattern
            });

            yield return new WaitForSeconds(0.5f);
            _sectionActive = false;
            ShowMenu();
        }

        // --- SECTION 10: AUDIO ---

        void ShowAudio()
        {
            var panel = CreateSubPanel("AUDIO");
            _activeCoroutine = StartCoroutine(AudioRoutine(panel));
        }

        IEnumerator AudioRoutine(GameObject panel)
        {
            int selection = 0;
            string[] options = new string[]
            {
                "Town Music",
                "Path Music",
                "QuietGrove Music",
                "Stop Music",
                "SFX: Tap",
                "SFX: Chime",
                "SFX: Water Click",
                "SFX: Step",
                "SFX: Menu Select",
                "SFX: Menu Move",
                "SFX: Hit",
                "SFX: Rest"
            };

            var optionTexts = new Text[options.Length];
            int visibleStart = 0;
            int visibleCount = 8;

            void RenderOptions()
            {
                foreach (var t in optionTexts)
                    if (t != null) Destroy(t.gameObject);

                int end = Mathf.Min(visibleStart + visibleCount, options.Length);
                for (int i = visibleStart; i < end; i++)
                {
                    float y = 0.82f - (i - visibleStart) * 0.08f;
                    optionTexts[i] = CreateText(panel, $"Opt{i}", options[i],
                        new Vector2(0.15f, y), 5);
                    optionTexts[i].alignment = TextAnchor.MiddleLeft;
                    optionTexts[i].color = i == selection
                        ? FieldFriendsPalette.MutedInk
                        : FieldFriendsPalette.LavenderBlue;
                }
            }

            RenderOptions();

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    selection = Mathf.Max(0, selection - 1);
                    if (selection < visibleStart) visibleStart = selection;
                    RenderOptions();
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    selection = Mathf.Min(options.Length - 1, selection + 1);
                    if (selection >= visibleStart + visibleCount)
                        visibleStart = selection - visibleCount + 1;
                    RenderOptions();
                }
                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
                {
                    PlayAudioOption(selection);
                }
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)
                    || Input.GetKeyDown(KeyCode.Tab))
                {
                    CloseActiveSection();
                    yield break;
                }
                yield return null;
            }
        }

        void PlayAudioOption(int index)
        {
            var audio = AudioManager.Instance;
            if (audio == null) return;

            switch (index)
            {
                case 0: audio.PlayTrack(AudioManager.MusicTrack.Town); break;
                case 1: audio.PlayTrack(AudioManager.MusicTrack.Path); break;
                case 2: audio.PlayTrack(AudioManager.MusicTrack.QuietGrove); break;
                case 3: audio.StopMusic(); break;
                case 4: audio.PlayTap(); break;
                case 5: audio.PlayChime(); break;
                case 6: audio.PlayWaterClick(); break;
                case 7: audio.PlayStep(); break;
                case 8: audio.PlayMenuSelect(); break;
                case 9: audio.PlayMenuMove(); break;
                case 10: audio.PlayHit(); break;
                case 11: audio.PlayRest(); break;
            }
        }

        // --- SECTION 11: ENDING ---

        void ShowEnding()
        {
            _menuPanel.SetActive(false);
            _sectionActive = true;
            _activeCoroutine = StartCoroutine(EndingRoutine());
        }

        IEnumerator EndingRoutine()
        {
            // Play ending sequence - it returns to title when done
            // We'll intercept and come back to demo instead
            var dialogueBox = FindFirstObjectByType<DialogueBox>();
            if (dialogueBox != null)
                yield return dialogueBox.ShowDialogue("Playing ending sequence...");

            yield return EndingSequence.PlayEnding(this);

            // After ending returns to title, hide title and show demo again
            yield return new WaitForSeconds(0.5f);
            var titleScreen = FindFirstObjectByType<TitleScreen>();
            if (titleScreen != null)
                titleScreen.gameObject.SetActive(false);

            // Restore game state
            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null) gm.SetState(GameState.Overworld);

            if (ScreenTransition.Instance != null)
                yield return ScreenTransition.Instance.FadeIn();

            _sectionActive = false;
            ShowMenu();
        }

        // --- SECTION 0: FREE ROAM ---

        void FreeRoam()
        {
            _menuVisible = false;
            _menuPanel.SetActive(false);

            // Ensure overworld state
            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null) gm.SetState(GameState.Overworld);

            // Unlock player movement
            var player = FindFirstObjectByType<GridMovement>();
            if (player != null) player.SetMovementLocked(false);
        }

        // --- Helpers ---

        IEnumerator WaitForDismiss()
        {
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)
                    || Input.GetKeyDown(KeyCode.Tab))
                {
                    CloseActiveSection();
                    yield break;
                }
                yield return null;
            }
        }

        string GetTypeColorHex(CreatureType type)
        {
            switch (type)
            {
                case CreatureType.Field: return "#BFD8C2";   // PastelMint
                case CreatureType.Wind: return "#9FAFD6";    // LavenderBlue
                case CreatureType.Water: return "#9ED6D3";   // PaleTeal
                case CreatureType.Meadow: return "#F6E27F";  // ButterYellow
                default: return "#4A4E69";
            }
        }

        static Text CreateText(GameObject parent, string name, string content,
            Vector2 anchorPos, int fontSize)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            var text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = FieldFriendsPalette.MutedInk;
            var rect = text.rectTransform;
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.sizeDelta = new Vector2(150, 20);
            rect.anchoredPosition = Vector2.zero;
            return text;
        }
    }
}
