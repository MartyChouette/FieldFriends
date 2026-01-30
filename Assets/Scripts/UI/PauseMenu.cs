using UnityEngine;
using UnityEngine.UI;
using FieldFriends.Core;
using FieldFriends.Palette;

namespace FieldFriends.UI
{
    /// <summary>
    /// Pause menu with four options: FRIENDS, MAP, WAIT, SAVE.
    /// Minimal UI noise, high readability.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject menuPanel;
        [SerializeField] GameObject friendsPanel;
        [SerializeField] GameObject mapPanel;

        [Header("Menu Buttons")]
        [SerializeField] Button friendsButton;
        [SerializeField] Button mapButton;
        [SerializeField] Button waitButton;
        [SerializeField] Button saveButton;

        [Header("Menu Labels")]
        [SerializeField] Text friendsLabel;
        [SerializeField] Text mapLabel;
        [SerializeField] Text waitLabel;
        [SerializeField] Text saveLabel;

        [Header("References")]
        [SerializeField] FriendsUI friendsUI;
        [SerializeField] MapUI mapUI;

        int _menuIndex;
        bool _isOpen;

        void Awake()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
            if (friendsPanel != null) friendsPanel.SetActive(false);
            if (mapPanel != null) mapPanel.SetActive(false);

            if (friendsButton != null) friendsButton.onClick.AddListener(OpenFriends);
            if (mapButton != null) mapButton.onClick.AddListener(OpenMap);
            if (waitButton != null) waitButton.onClick.AddListener(DoWait);
            if (saveButton != null) saveButton.onClick.AddListener(DoSave);
        }

        void Update()
        {
            // Toggle pause with Escape or X key
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            {
                if (_isOpen)
                    Close();
                else
                    Open();
                return;
            }

            if (!_isOpen || menuPanel == null || !menuPanel.activeSelf) return;

            // Keyboard navigation
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _menuIndex = Mathf.Max(0, _menuIndex - 1);
                UpdateHighlight();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _menuIndex = Mathf.Min(3, _menuIndex + 1);
                UpdateHighlight();
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
            {
                SelectCurrent();
            }
        }

        public void Open()
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null && gameManager.CurrentState != Data.GameState.Overworld)
                return;

            _isOpen = true;
            _menuIndex = 0;
            if (menuPanel != null) menuPanel.SetActive(true);
            UpdateHighlight();

            if (gameManager != null)
                gameManager.SetState(Data.GameState.Paused);

            // Lock player movement
            var player = FindFirstObjectByType<GridMovement>();
            if (player != null) player.SetMovementLocked(true);
        }

        public void Close()
        {
            _isOpen = false;
            if (menuPanel != null) menuPanel.SetActive(false);
            if (friendsPanel != null) friendsPanel.SetActive(false);
            if (mapPanel != null) mapPanel.SetActive(false);

            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
                gameManager.SetState(Data.GameState.Overworld);

            var player = FindFirstObjectByType<GridMovement>();
            if (player != null) player.SetMovementLocked(false);
        }

        void SelectCurrent()
        {
            switch (_menuIndex)
            {
                case 0: OpenFriends(); break;
                case 1: OpenMap(); break;
                case 2: DoWait(); break;
                case 3: DoSave(); break;
            }
        }

        void OpenFriends()
        {
            if (friendsPanel != null) friendsPanel.SetActive(true);
            if (menuPanel != null) menuPanel.SetActive(false);
            if (friendsUI != null) friendsUI.Refresh();
        }

        void OpenMap()
        {
            if (mapPanel != null) mapPanel.SetActive(true);
            if (menuPanel != null) menuPanel.SetActive(false);
            if (mapUI != null) mapUI.Refresh();
        }

        void DoWait()
        {
            // "You rest for a bit." -- brief pause, heals 1 HP to all
            var partyManager = FindFirstObjectByType<Party.PartyManager>();
            if (partyManager != null)
            {
                foreach (var c in partyManager.Party)
                {
                    if (!c.IsResting) c.Heal(1);
                }
            }
            Close();
        }

        void DoSave()
        {
            var saveManager = FindFirstObjectByType<Save.SaveManager>();
            if (saveManager != null)
                saveManager.ManualSave();
            Close();
        }

        void UpdateHighlight()
        {
            SetLabelColor(friendsLabel, _menuIndex == 0);
            SetLabelColor(mapLabel, _menuIndex == 1);
            SetLabelColor(waitLabel, _menuIndex == 2);
            SetLabelColor(saveLabel, _menuIndex == 3);
        }

        void SetLabelColor(Text label, bool selected)
        {
            if (label == null) return;
            label.color = selected
                ? FieldFriendsPalette.MutedInk
                : FieldFriendsPalette.LavenderBlue;
        }
    }
}
