using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FieldFriends.Core;
using FieldFriends.Save;
using FieldFriends.Palette;

namespace FieldFriends.UI
{
    /// <summary>
    /// Title screen. Shows game title, options: NEW / CONTINUE.
    /// CONTINUE only visible if a save file exists.
    /// Calm, no animation beyond a gentle text blink.
    /// </summary>
    public class TitleScreen : MonoBehaviour
    {
        GameObject _panel;
        Text _titleText;
        Text _newGameText;
        Text _continueText;
        Text _promptText;

        int _selection;
        bool _hasSave;
        bool _active;

        public void Show()
        {
            if (_panel == null)
                BuildUI();

            var saveManager = FindFirstObjectByType<SaveManager>();
            _hasSave = saveManager != null && saveManager.HasSave();

            _selection = _hasSave ? 1 : 0; // default to Continue if save exists
            _continueText.gameObject.SetActive(_hasSave);

            _panel.SetActive(true);
            _active = true;

            UpdateHighlight();

            // Disable gameplay input
            if (InputManager.Instance != null)
                InputManager.Instance.SetEnabled(true);

            // Start fade in
            if (ScreenTransition.Instance != null)
                ScreenTransition.Instance.FadeIn();
        }

        void BuildUI()
        {
            _panel = new GameObject("TitlePanel");
            _panel.transform.SetParent(transform, false);

            // Background
            var bg = _panel.AddComponent<Image>();
            bg.color = FieldFriendsPalette.CreamMist;
            var bgRect = bg.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Title
            _titleText = CreateText(_panel, "TitleText",
                "Field Friends",
                new Vector2(0.5f, 0.65f),
                12);
            _titleText.color = FieldFriendsPalette.MutedInk;

            // Subtitle
            var subtitle = CreateText(_panel, "Subtitle",
                "Bramble Vale",
                new Vector2(0.5f, 0.55f),
                7);
            subtitle.color = FieldFriendsPalette.LavenderBlue;

            // New Game
            _newGameText = CreateText(_panel, "NewGame",
                "NEW",
                new Vector2(0.5f, 0.35f),
                8);

            // Continue
            _continueText = CreateText(_panel, "Continue",
                "CONTINUE",
                new Vector2(0.5f, 0.25f),
                8);

            // Prompt
            _promptText = CreateText(_panel, "Prompt",
                "",
                new Vector2(0.5f, 0.12f),
                5);
            _promptText.color = FieldFriendsPalette.LavenderBlue;

            StartCoroutine(BlinkPrompt());
        }

        Text CreateText(GameObject parent, string name, string content, Vector2 anchorPos, int fontSize)
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

        IEnumerator BlinkPrompt()
        {
            while (true)
            {
                if (_promptText != null)
                {
                    _promptText.text = "press Z";
                    yield return new WaitForSeconds(0.8f);
                    _promptText.text = "";
                    yield return new WaitForSeconds(0.4f);
                }
                else
                {
                    yield return null;
                }
            }
        }

        void Update()
        {
            if (!_active || _panel == null || !_panel.activeSelf) return;

            // Navigation
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (_selection > 0)
                {
                    _selection--;
                    UpdateHighlight();
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                int max = _hasSave ? 1 : 0;
                if (_selection < max)
                {
                    _selection++;
                    UpdateHighlight();
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
                }
            }

            // Confirm
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                StartCoroutine(ConfirmSelection());
            }
        }

        void UpdateHighlight()
        {
            _newGameText.color = _selection == 0
                ? FieldFriendsPalette.MutedInk
                : FieldFriendsPalette.LavenderBlue;

            if (_continueText.gameObject.activeSelf)
            {
                _continueText.color = _selection == 1
                    ? FieldFriendsPalette.MutedInk
                    : FieldFriendsPalette.LavenderBlue;
            }
        }

        IEnumerator ConfirmSelection()
        {
            _active = false;

            // Fade out
            if (ScreenTransition.Instance != null)
                yield return ScreenTransition.Instance.FadeOut();

            var gameManager = FindFirstObjectByType<GameManager>();

            if (_selection == 0)
            {
                // New Game
                if (gameManager != null)
                    gameManager.StartNewGame();
            }
            else
            {
                // Continue
                if (gameManager != null)
                    gameManager.LoadGame();
            }

            _panel.SetActive(false);

            // Fade in to the game
            if (ScreenTransition.Instance != null)
                yield return ScreenTransition.Instance.FadeIn();
        }
    }
}
