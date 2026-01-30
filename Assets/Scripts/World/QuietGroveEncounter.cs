using UnityEngine;
using FieldFriends.Data;
using FieldFriends.Party;
using FieldFriends.UI;

namespace FieldFriends.World
{
    /// <summary>
    /// Special trigger for the single optional Still encounter in Quiet Grove.
    /// Not a random encounter -- the player approaches and interacts.
    /// </summary>
    public class QuietGroveEncounter : MonoBehaviour
    {
        [SerializeField] string approachText = "Something is here.";

        bool _triggered;
        bool _inRange;

        void Update()
        {
            if (_triggered || !_inRange) return;
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
            {
                TriggerEncounter();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _inRange = true;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _inRange = false;
        }

        void TriggerEncounter()
        {
            _triggered = true;

            // Show approach text first, then start battle with Still
            var dialogueBox = FindFirstObjectByType<DialogueBox>();
            if (dialogueBox != null)
            {
                StartCoroutine(ShowThenBattle(dialogueBox));
            }
        }

        System.Collections.IEnumerator ShowThenBattle(DialogueBox dialogueBox)
        {
            yield return dialogueBox.ShowDialogue(approachText);

            var gameManager = FindFirstObjectByType<Core.GameManager>();
            if (gameManager != null)
            {
                gameManager.StartBattle("Still");
            }
        }
    }
}
