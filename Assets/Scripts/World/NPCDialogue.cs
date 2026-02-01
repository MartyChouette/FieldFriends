using UnityEngine;
using FieldFriends.UI;

namespace FieldFriends.World
{
    /// <summary>
    /// NPC with a single line of dialogue, triggered by interaction.
    /// All 6 NPC lines for the entire game:
    /// - Willow End (pond): "The water's been calm lately."
    /// - Willow End (house): "You heading out again?"
    /// - South Field: "It's easy to lose track of time here."
    /// - Stonebridge: "This bridge has always been here."
    /// - North Meadow: "Things grow better when you leave them alone."
    /// - Quiet Grove: "You can stay if you want."
    /// </summary>
    public class NPCDialogue : MonoBehaviour
    {
        [SerializeField]
        [TextArea(1, 3)]
        string dialogue = "...";

        bool _inRange;

        /// <summary>
        /// Set dialogue at runtime (used by AreaSceneSetup).
        /// </summary>
        public void SetDialogue(string text)
        {
            dialogue = text;
        }

        void Update()
        {
            if (!_inRange) return;
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
            {
                TriggerDialogue();
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

        void TriggerDialogue()
        {
            DialogueBox dialogueBox = FindFirstObjectByType<DialogueBox>();
            if (dialogueBox != null && !dialogueBox.IsShowing)
            {
                StartCoroutine(dialogueBox.ShowDialogue(dialogue));
            }
        }
    }
}
