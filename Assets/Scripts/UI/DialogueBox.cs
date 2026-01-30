using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FieldFriends.Core;
using FieldFriends.Palette;

namespace FieldFriends.UI
{
    /// <summary>
    /// Simple dialogue box for NPC text.
    /// Short sentences. No exclamation points. No moral framing.
    /// One line at a time, advance with Z/Enter.
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] GameObject panel;
        [SerializeField] Text textLabel;

        bool _isShowing;
        bool _waitingForDismiss;

        void Awake()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        void Update()
        {
            if (_waitingForDismiss &&
                (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return)))
            {
                _waitingForDismiss = false;
            }
        }

        public IEnumerator ShowDialogue(string message)
        {
            _isShowing = true;

            // Lock player movement
            var player = FindFirstObjectByType<GridMovement>();
            if (player != null) player.SetMovementLocked(true);

            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
                gameManager.SetState(Data.GameState.Dialogue);

            if (panel != null) panel.SetActive(true);
            if (textLabel != null)
            {
                textLabel.text = message;
                textLabel.color = FieldFriendsPalette.MutedInk;
            }

            _waitingForDismiss = true;
            while (_waitingForDismiss)
                yield return null;

            if (panel != null) panel.SetActive(false);

            if (gameManager != null)
                gameManager.SetState(Data.GameState.Overworld);
            if (player != null) player.SetMovementLocked(false);

            _isShowing = false;
        }

        public bool IsShowing => _isShowing;
    }
}
