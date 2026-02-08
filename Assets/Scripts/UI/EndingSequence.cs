using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FieldFriends.Core;
using FieldFriends.Party;
using FieldFriends.Palette;

namespace FieldFriends.UI
{
    /// <summary>
    /// Quiet ending when the player reaches Quiet Grove and speaks
    /// to the NPC there. No fanfare. Just acknowledgement.
    ///
    /// Shows each party member's name, then a final message.
    /// Fades to cream. Returns to title.
    /// </summary>
    public class EndingSequence : MonoBehaviour
    {
        public static void TriggerIfReady()
        {
            // Called when entering Quiet Grove
            // The ending is just being there. No trigger needed.
            // The NPC dialogue ("You can stay if you want.") is the ending.
        }

        public static Coroutine PlayEnding(MonoBehaviour host)
        {
            return host.StartCoroutine(EndingRoutine());
        }

        static IEnumerator EndingRoutine()
        {
            var input = InputManager.Instance;
            if (input != null) input.SetEnabled(false);

            yield return new WaitForSeconds(1.5f);

            // Fade out
            if (ScreenTransition.Instance != null)
                yield return ScreenTransition.Instance.FadeOut();

            // Create ending canvas
            var canvasObj = new GameObject("EndingCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(160, 144);

            // Background
            var bgObj = new GameObject("BG");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bg = bgObj.AddComponent<Image>();
            bg.color = FieldFriendsPalette.CreamMist;
            var bgRect = bg.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Show each party member's name
            var partyManager = Object.FindFirstObjectByType<PartyManager>();
            if (partyManager != null)
            {
                float yPos = 0.7f;
                foreach (var creature in partyManager.Party)
                {
                    var nameText = CreateEndingText(canvasObj, creature.Name,
                        new Vector2(0.5f, yPos), 8);
                    nameText.color = new Color(
                        FieldFriendsPalette.MutedInk.r,
                        FieldFriendsPalette.MutedInk.g,
                        FieldFriendsPalette.MutedInk.b,
                        0f);

                    // Fade in the name
                    float fadeTime = 0f;
                    while (fadeTime < 1f)
                    {
                        fadeTime += Time.deltaTime;
                        nameText.color = new Color(
                            nameText.color.r, nameText.color.g,
                            nameText.color.b, Mathf.Clamp01(fadeTime));
                        yield return null;
                    }

                    yield return new WaitForSeconds(1.2f);
                    yPos -= 0.15f;
                }
            }

            yield return new WaitForSeconds(1f);

            // Final message
            var finalText = CreateEndingText(canvasObj, "Thank you for walking.",
                new Vector2(0.5f, 0.2f), 6);
            finalText.color = new Color(
                FieldFriendsPalette.LavenderBlue.r,
                FieldFriendsPalette.LavenderBlue.g,
                FieldFriendsPalette.LavenderBlue.b,
                0f);

            float ft = 0f;
            while (ft < 1.5f)
            {
                ft += Time.deltaTime;
                finalText.color = new Color(
                    finalText.color.r, finalText.color.g,
                    finalText.color.b, Mathf.Clamp01(ft / 1.5f));
                yield return null;
            }

            yield return new WaitForSeconds(3f);

            // Wait for input
            while (!Input.GetKeyDown(KeyCode.Z) && !Input.GetKeyDown(KeyCode.Return))
                yield return null;

            Object.Destroy(canvasObj);

            if (input != null) input.SetEnabled(true);

            // Return to title
            var titleScreen = Object.FindFirstObjectByType<TitleScreen>();
            if (titleScreen != null)
                titleScreen.Show();
        }

        static Text CreateEndingText(GameObject parent, string content,
            Vector2 anchor, int fontSize)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent.transform, false);
            var text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            var rect = text.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(150, 20);
            rect.anchoredPosition = Vector2.zero;

            return text;
        }
    }
}
