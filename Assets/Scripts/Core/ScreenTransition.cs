using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FieldFriends.Core
{
    /// <summary>
    /// Screen fade in/out using a full-screen UI Image.
    /// No flashy effects -- just a calm, even fade to/from the
    /// palette's Muted Ink color.
    /// </summary>
    public class ScreenTransition : MonoBehaviour
    {
        public static ScreenTransition Instance { get; private set; }

        Image _fadeImage;
        Canvas _canvas;
        bool _transitioning;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateFadeOverlay();
        }

        void CreateFadeOverlay()
        {
            // Create a canvas for the fade overlay
            var canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999;
            canvasObj.AddComponent<CanvasScaler>();

            // Create the fade image
            var imgObj = new GameObject("FadeImage");
            imgObj.transform.SetParent(canvasObj.transform, false);
            _fadeImage = imgObj.AddComponent<Image>();
            _fadeImage.color = new Color(
                Palette.FieldFriendsPalette.MutedInk.r,
                Palette.FieldFriendsPalette.MutedInk.g,
                Palette.FieldFriendsPalette.MutedInk.b,
                0f
            );

            // Fill the screen
            var rect = _fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _fadeImage.raycastTarget = false;
        }

        public bool IsTransitioning => _transitioning;

        /// <summary>
        /// Fade to black (Muted Ink), execute an action, then fade back in.
        /// </summary>
        public Coroutine DoTransition(System.Action midAction = null)
        {
            if (_transitioning) return null;
            return StartCoroutine(TransitionRoutine(midAction));
        }

        /// <summary>
        /// Just fade out (to opaque).
        /// </summary>
        public Coroutine FadeOut()
        {
            if (_transitioning) return null;
            return StartCoroutine(FadeRoutine(0f, 1f));
        }

        /// <summary>
        /// Just fade in (to transparent).
        /// </summary>
        public Coroutine FadeIn()
        {
            if (_transitioning) return null;
            return StartCoroutine(FadeRoutine(1f, 0f));
        }

        IEnumerator TransitionRoutine(System.Action midAction)
        {
            _transitioning = true;

            // Fade out
            yield return FadeRoutine(0f, 1f);

            // Execute mid-transition action
            midAction?.Invoke();

            // Brief hold
            yield return new WaitForSeconds(0.1f);

            // Fade in
            yield return FadeRoutine(1f, 0f);

            _transitioning = false;
        }

        IEnumerator FadeRoutine(float fromAlpha, float toAlpha)
        {
            _transitioning = true;
            float duration = GameConstants.FadeDuration;
            float elapsed = 0f;

            var color = _fadeImage.color;
            color.a = fromAlpha;
            _fadeImage.color = color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                _fadeImage.color = color;
                yield return null;
            }

            color.a = toAlpha;
            _fadeImage.color = color;

            if (toAlpha == 0f)
                _transitioning = false;
        }
    }
}
