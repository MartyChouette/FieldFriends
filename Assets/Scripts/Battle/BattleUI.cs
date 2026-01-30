using UnityEngine;
using UnityEngine.UI;
using FieldFriends.Data;
using FieldFriends.Party;
using FieldFriends.Palette;

namespace FieldFriends.Battle
{
    /// <summary>
    /// Battle screen UI: GB-like layout.
    /// Enemy zone (top), party zone (mid), text box (bottom).
    /// HP bars only. No floating numbers. No flashing. No shake.
    /// One line of text per action.
    ///
    /// Menu: MOVE / WAIT / BACK
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        [Header("HP Bars")]
        [SerializeField] Slider playerHPBar;
        [SerializeField] Slider enemyHPBar;

        [Header("Sprites")]
        [SerializeField] Image playerSprite;
        [SerializeField] Image enemySprite;

        [Header("Text")]
        [SerializeField] Text textBox;

        [Header("Menu")]
        [SerializeField] GameObject menuPanel;
        [SerializeField] Button moveButton;
        [SerializeField] Button waitButton;
        [SerializeField] Button backButton;

        [Header("References")]
        [SerializeField] BattleManager battleManager;

        CreatureInstance _player;
        CreatureInstance _enemy;

        int _menuIndex;

        void Awake()
        {
            if (menuPanel != null)
                menuPanel.SetActive(false);

            // Wire up button callbacks
            if (moveButton != null)
                moveButton.onClick.AddListener(() => OnMenuSelect(BattleAction.Move));
            if (waitButton != null)
                waitButton.onClick.AddListener(() => OnMenuSelect(BattleAction.Wait));
            if (backButton != null)
                backButton.onClick.AddListener(() => OnMenuSelect(BattleAction.Back));
        }

        void Update()
        {
            if (menuPanel == null || !menuPanel.activeSelf) return;

            // Keyboard navigation for the battle menu
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _menuIndex = Mathf.Max(0, _menuIndex - 1);
                UpdateMenuHighlight();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _menuIndex = Mathf.Min(2, _menuIndex + 1);
                UpdateMenuHighlight();
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
            {
                OnMenuSelect((BattleAction)_menuIndex);
            }
        }

        public void Initialize(CreatureInstance player, CreatureInstance enemy)
        {
            _player = player;
            _enemy = enemy;
            UpdateHP();
            ShowMenu(false);
        }

        public void SetPlayerCreature(CreatureInstance player)
        {
            _player = player;
            UpdateHP();
        }

        public void UpdateHP()
        {
            if (playerHPBar != null && _player != null)
            {
                playerHPBar.maxValue = _player.MaxHP;
                playerHPBar.value = _player.CurrentHP;
            }
            if (enemyHPBar != null && _enemy != null)
            {
                enemyHPBar.maxValue = _enemy.MaxHP;
                enemyHPBar.value = _enemy.CurrentHP;
            }
        }

        public void ShowText(string message)
        {
            if (textBox != null)
                textBox.text = message;
        }

        public void ShowMenu(bool visible)
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(visible);
                if (visible)
                {
                    _menuIndex = 0;
                    UpdateMenuHighlight();
                }
            }
        }

        void UpdateMenuHighlight()
        {
            // Highlight the selected menu item with Muted Ink,
            // non-selected items use Lavender Blue.
            SetButtonColor(moveButton, _menuIndex == 0);
            SetButtonColor(waitButton, _menuIndex == 1);
            SetButtonColor(backButton, _menuIndex == 2);
        }

        void SetButtonColor(Button btn, bool selected)
        {
            if (btn == null) return;
            var text = btn.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.color = selected
                    ? FieldFriendsPalette.MutedInk
                    : FieldFriendsPalette.LavenderBlue;
            }
        }

        void OnMenuSelect(BattleAction action)
        {
            if (battleManager != null)
                battleManager.OnActionSelected(action);
        }
    }
}
