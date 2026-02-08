using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using FieldFriends.Core;
using FieldFriends.World;
using FieldFriends.Party;
using FieldFriends.Battle;
using FieldFriends.Save;
using FieldFriends.Audio;
using FieldFriends.UI;
using FieldFriends.Data;
using FieldFriends.Palette;

namespace FieldFriends.Demo
{
    /// <summary>
    /// Demo bootstrapper that creates the full game infrastructure with
    /// wired-up UI elements, sets up a demo party, and launches the
    /// DemoController overlay. Attach to an empty GameObject in a new scene.
    /// </summary>
    public class DemoBootstrapper : MonoBehaviour
    {
        void Awake()
        {
            if (FindFirstObjectByType<GameManager>() != null)
            {
                Destroy(gameObject);
                return;
            }

            CreateGameRoot();
            var uiRoot = CreateUIRoot();
            SetupDemoParty();
            SetupStartingArea();

            // Add demo controller
            var demoObj = new GameObject("DemoController");
            demoObj.AddComponent<DemoController>();
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
            root.AddComponent<AreaManager>();
            root.AddComponent<EncounterSystem>();
        }

        GameObject CreateUIRoot()
        {
            var uiRoot = new GameObject("[UIRoot]");
            DontDestroyOnLoad(uiRoot);

            var canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(160, 144);
            scaler.matchWidthOrHeight = 0f;

            uiRoot.AddComponent<GraphicRaycaster>();

            // Battle system
            var battleObj = CreateChild(uiRoot, "Battle");
            var battleManager = battleObj.AddComponent<BattleManager>();
            var battleUI = battleObj.AddComponent<BattleUI>();
            BuildBattleUI(battleObj, battleUI, battleManager);
            battleObj.SetActive(false);

            // Dialogue box
            var dialogueObj = CreateChild(uiRoot, "DialogueBox");
            var dialogueBox = dialogueObj.AddComponent<DialogueBox>();
            BuildDialogueUI(dialogueObj, dialogueBox);

            // Pause menu
            var pauseObj = CreateChild(uiRoot, "PauseMenu");
            var pauseMenu = pauseObj.AddComponent<PauseMenu>();

            // Friends UI (child of pause)
            var friendsObj = CreateChild(pauseObj, "FriendsUI");
            var friendsUI = friendsObj.AddComponent<FriendsUI>();
            BuildFriendsUI(friendsObj, friendsUI);

            // Map UI (child of pause)
            var mapObj = CreateChild(pauseObj, "MapUI");
            var mapUI = mapObj.AddComponent<MapUI>();
            BuildMapUI(mapObj, mapUI);

            // Wire pause menu
            BuildPauseMenuUI(pauseObj, pauseMenu, friendsUI, mapUI);

            return uiRoot;
        }

        void SetupDemoParty()
        {
            var partyManager = FindFirstObjectByType<PartyManager>();
            if (partyManager == null) return;

            partyManager.ClearParty();

            // Mossbit with friendship=120 (shows upgrade)
            var mossbitDef = CreatureDatabase.FindByName("Mossbit");
            if (mossbitDef != null)
            {
                var mossbit = CreatureInstance.FromDef(mossbitDef.Value);
                mossbit.Friendship = 120;
                partyManager.AddToParty(mossbit);
            }

            // Skirl (Wind type for diversity)
            var skirlDef = CreatureDatabase.FindByName("Skirl");
            if (skirlDef != null)
                partyManager.AddToParty(CreatureInstance.FromDef(skirlDef.Value));

            // Ripplet (Water type for diversity)
            var rippletDef = CreatureDatabase.FindByName("Ripplet");
            if (rippletDef != null)
                partyManager.AddToParty(CreatureInstance.FromDef(rippletDef.Value));
        }

        void SetupStartingArea()
        {
            var areaManager = FindFirstObjectByType<AreaManager>();
            if (areaManager != null)
                areaManager.TransitionTo(AreaID.WillowEnd, new Vector2Int(10, 5));

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayTrack(AudioManager.MusicTrack.Town);
        }

        // --- UI Building ---

        void BuildDialogueUI(GameObject parent, DialogueBox dialogueBox)
        {
            // Panel: bottom-anchored, CreamMist bg
            var panel = CreateUIPanel(parent, "Panel",
                new Vector2(0f, 0f), new Vector2(1f, 0.25f),
                FieldFriendsPalette.CreamMist);
            panel.SetActive(false);

            // Text label
            var textLabel = CreateText(panel, "TextLabel", "",
                new Vector2(0.5f, 0.5f), 6);
            textLabel.color = FieldFriendsPalette.MutedInk;

            SetField(dialogueBox, "panel", panel);
            SetField(dialogueBox, "textLabel", textLabel);
        }

        void BuildBattleUI(GameObject parent, BattleUI battleUI, BattleManager battleManager)
        {
            // Background panel
            var bg = CreateUIPanel(parent, "BattleBG",
                Vector2.zero, Vector2.one,
                FieldFriendsPalette.CreamMist);

            // Enemy name + HP (top area)
            var enemyHPBar = CreateSlider(bg, "EnemyHP",
                new Vector2(0.5f, 0.85f), new Vector2(80, 8),
                FieldFriendsPalette.PastelMint, FieldFriendsPalette.DustyRose);

            // Player name + HP (mid area)
            var playerHPBar = CreateSlider(bg, "PlayerHP",
                new Vector2(0.5f, 0.55f), new Vector2(80, 8),
                FieldFriendsPalette.PastelMint, FieldFriendsPalette.PaleTeal);

            // Player/Enemy sprites (placeholder images)
            var enemySprite = CreateImage(bg, "EnemySprite",
                new Vector2(0.5f, 0.75f), new Vector2(24, 24),
                FieldFriendsPalette.LavenderBlue);

            var playerSprite = CreateImage(bg, "PlayerSprite",
                new Vector2(0.3f, 0.5f), new Vector2(24, 24),
                FieldFriendsPalette.PastelMint);

            // Text box
            var textBox = CreateText(bg, "TextBox", "",
                new Vector2(0.5f, 0.25f), 5);
            textBox.color = FieldFriendsPalette.MutedInk;

            // Menu panel
            var menuPanel = CreateUIPanel(bg, "MenuPanel",
                new Vector2(0.55f, 0.05f), new Vector2(0.95f, 0.35f),
                new Color(FieldFriendsPalette.CreamMist.r,
                          FieldFriendsPalette.CreamMist.g,
                          FieldFriendsPalette.CreamMist.b, 0.95f));
            menuPanel.SetActive(false);

            var moveButton = CreateButton(menuPanel, "MoveBtn", "MOVE",
                new Vector2(0.5f, 0.75f), 6);
            var waitButton = CreateButton(menuPanel, "WaitBtn", "WAIT",
                new Vector2(0.5f, 0.5f), 6);
            var backButton = CreateButton(menuPanel, "BackBtn", "BACK",
                new Vector2(0.5f, 0.25f), 6);

            SetField(battleUI, "playerHPBar", playerHPBar);
            SetField(battleUI, "enemyHPBar", enemyHPBar);
            SetField(battleUI, "playerSprite", playerSprite);
            SetField(battleUI, "enemySprite", enemySprite);
            SetField(battleUI, "textBox", textBox);
            SetField(battleUI, "menuPanel", menuPanel);
            SetField(battleUI, "moveButton", moveButton);
            SetField(battleUI, "waitButton", waitButton);
            SetField(battleUI, "backButton", backButton);
            SetField(battleUI, "battleManager", battleManager);

            // Also wire BattleManager -> BattleUI
            SetField(battleManager, "battleUI", battleUI);
        }

        void BuildPauseMenuUI(GameObject parent, PauseMenu pauseMenu,
            FriendsUI friendsUI, MapUI mapUI)
        {
            // Menu panel
            var menuPanel = CreateUIPanel(parent, "MenuPanel",
                new Vector2(0.6f, 0.2f), new Vector2(0.95f, 0.8f),
                FieldFriendsPalette.CreamMist);
            menuPanel.SetActive(false);

            // 4 labeled options
            var friendsBtn = CreateButton(menuPanel, "FriendsBtn", "FRIENDS",
                new Vector2(0.5f, 0.85f), 6);
            var mapBtn = CreateButton(menuPanel, "MapBtn", "MAP",
                new Vector2(0.5f, 0.65f), 6);
            var waitBtn = CreateButton(menuPanel, "WaitBtn", "WAIT",
                new Vector2(0.5f, 0.45f), 6);
            var saveBtn = CreateButton(menuPanel, "SaveBtn", "SAVE",
                new Vector2(0.5f, 0.25f), 6);

            // Labels
            var friendsLabel = friendsBtn.GetComponentInChildren<Text>();
            var mapLabel = mapBtn.GetComponentInChildren<Text>();
            var waitLabel = waitBtn.GetComponentInChildren<Text>();
            var saveLabel = saveBtn.GetComponentInChildren<Text>();

            // Friends sub-panel (full overlay)
            var friendsPanel = CreateUIPanel(parent, "FriendsPanel",
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f),
                FieldFriendsPalette.CreamMist);
            friendsPanel.SetActive(false);
            // Copy FriendsUI's parent as the panel content area
            friendsUI.transform.SetParent(friendsPanel.transform, false);

            // Map sub-panel (full overlay)
            var mapPanel = CreateUIPanel(parent, "MapPanel",
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f),
                FieldFriendsPalette.CreamMist);
            mapPanel.SetActive(false);
            mapUI.transform.SetParent(mapPanel.transform, false);

            SetField(pauseMenu, "menuPanel", menuPanel);
            SetField(pauseMenu, "friendsPanel", friendsPanel);
            SetField(pauseMenu, "mapPanel", mapPanel);
            SetField(pauseMenu, "friendsButton", friendsBtn);
            SetField(pauseMenu, "mapButton", mapBtn);
            SetField(pauseMenu, "waitButton", waitBtn);
            SetField(pauseMenu, "saveButton", saveBtn);
            SetField(pauseMenu, "friendsLabel", friendsLabel);
            SetField(pauseMenu, "mapLabel", mapLabel);
            SetField(pauseMenu, "waitLabel", waitLabel);
            SetField(pauseMenu, "saveLabel", saveLabel);
            SetField(pauseMenu, "friendsUI", friendsUI);
            SetField(pauseMenu, "mapUI", mapUI);
        }

        void BuildFriendsUI(GameObject parent, FriendsUI friendsUI)
        {
            var slotPanels = new GameObject[3];
            var nameLabels = new Text[3];
            var hpBars = new Slider[3];
            var typeLabels = new Text[3];
            var statusLabels = new Text[3];

            for (int i = 0; i < 3; i++)
            {
                float yBase = 0.8f - i * 0.28f;

                var slot = CreateUIPanel(parent, $"Slot{i}",
                    new Vector2(0.05f, yBase - 0.22f),
                    new Vector2(0.95f, yBase),
                    FieldFriendsPalette.PastelMint);
                slotPanels[i] = slot;

                nameLabels[i] = CreateText(slot, "Name", "",
                    new Vector2(0.25f, 0.75f), 6);
                nameLabels[i].alignment = TextAnchor.MiddleLeft;

                hpBars[i] = CreateSlider(slot, "HP",
                    new Vector2(0.5f, 0.45f), new Vector2(60, 6),
                    FieldFriendsPalette.CreamMist, FieldFriendsPalette.PaleTeal);

                typeLabels[i] = CreateText(slot, "Type", "",
                    new Vector2(0.75f, 0.75f), 5);

                statusLabels[i] = CreateText(slot, "Status", "",
                    new Vector2(0.5f, 0.15f), 5);
            }

            SetField(friendsUI, "slotPanels", slotPanels);
            SetField(friendsUI, "nameLabels", nameLabels);
            SetField(friendsUI, "hpBars", hpBars);
            SetField(friendsUI, "typeLabels", typeLabels);
            SetField(friendsUI, "statusLabels", statusLabels);
        }

        void BuildMapUI(GameObject parent, MapUI mapUI)
        {
            var areaNodes = new RectTransform[7];
            var areaLabels = new Text[7];
            var nodeImages = new Image[7];

            // Layout positions for 7 areas in a vertical path
            Vector2[] positions = new Vector2[]
            {
                new Vector2(0.3f, 0.85f),  // WillowEnd
                new Vector2(0.2f, 0.7f),   // SouthField
                new Vector2(0.5f, 0.7f),   // CreekPath
                new Vector2(0.35f, 0.55f),  // HillRoad
                new Vector2(0.35f, 0.4f),   // Stonebridge
                new Vector2(0.5f, 0.25f),   // NorthMeadow
                new Vector2(0.5f, 0.1f),    // QuietGrove
            };

            for (int i = 0; i < 7; i++)
            {
                var nodeObj = new GameObject($"Node{i}");
                nodeObj.transform.SetParent(parent.transform, false);

                var nodeImg = nodeObj.AddComponent<Image>();
                nodeImg.color = FieldFriendsPalette.PastelMint;
                var nodeRect = nodeImg.rectTransform;
                nodeRect.anchorMin = positions[i];
                nodeRect.anchorMax = positions[i];
                nodeRect.sizeDelta = new Vector2(10, 10);
                nodeRect.anchoredPosition = Vector2.zero;

                areaNodes[i] = nodeRect;
                nodeImages[i] = nodeImg;

                var areaName = AreaDatabase.Get((AreaID)i).Name;
                var label = CreateText(nodeObj, "Label", areaName,
                    new Vector2(0.5f, -1f), 4);
                label.color = FieldFriendsPalette.MutedInk;
                areaLabels[i] = label;
            }

            // Player marker
            var markerObj = new GameObject("PlayerMarker");
            markerObj.transform.SetParent(parent.transform, false);
            var marker = markerObj.AddComponent<Image>();
            marker.color = FieldFriendsPalette.MutedInk;
            var markerRect = marker.rectTransform;
            markerRect.sizeDelta = new Vector2(6, 6);

            SetField(mapUI, "areaNodes", areaNodes);
            SetField(mapUI, "areaLabels", areaLabels);
            SetField(mapUI, "nodeImages", nodeImages);
            SetField(mapUI, "playerMarker", marker);
        }

        // --- Helpers ---

        static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
        }

        static GameObject CreateChild(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        static GameObject CreateUIPanel(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            var img = obj.AddComponent<Image>();
            img.color = color;
            var rect = img.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return obj;
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
            text.color = FieldFriendsPalette.MutedInk;
            var rect = text.rectTransform;
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.sizeDelta = new Vector2(150, 20);
            rect.anchoredPosition = Vector2.zero;
            return text;
        }

        static Slider CreateSlider(GameObject parent, string name,
            Vector2 anchorPos, Vector2 size, Color bgColor, Color fillColor)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(obj.transform, false);
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = bgColor;
            var bgRect = bgImg.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(obj.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            // Fill
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillArea.transform, false);
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = fillColor;
            var fillRect = fillImg.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var slider = obj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = bgImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
            slider.interactable = false;

            // Remove handle
            slider.handleRect = null;

            return slider;
        }

        static Image CreateImage(GameObject parent, string name,
            Vector2 anchorPos, Vector2 size, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            var img = obj.AddComponent<Image>();
            img.color = color;
            var rect = img.rectTransform;
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return img;
        }

        static Button CreateButton(GameObject parent, string name, string label,
            Vector2 anchorPos, int fontSize)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            var img = obj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // transparent background
            var rect = img.rectTransform;
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.sizeDelta = new Vector2(60, 14);
            rect.anchoredPosition = Vector2.zero;

            var text = CreateText(obj, "Label", label, new Vector2(0.5f, 0.5f), fontSize);
            text.color = FieldFriendsPalette.LavenderBlue;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            var button = obj.AddComponent<Button>();
            button.targetGraphic = img;

            return button;
        }
    }
}
