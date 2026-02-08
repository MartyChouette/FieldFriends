using UnityEngine;

namespace FieldFriends.Core
{
    /// <summary>
    /// 2-frame idle animation. Subtle bob/blink/sway.
    /// Loop duration: 0.8-1.2 seconds (randomized per instance).
    /// Attach to any sprite that should idle-animate.
    /// </summary>
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] Sprite frame0;
        [SerializeField] Sprite frame1;

        SpriteRenderer _renderer;
        float _loopDuration;
        float _timer;
        bool _onFrame1;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            // Randomize loop duration within spec range
            _loopDuration = Random.Range(
                GameConstants.IdleAnimMinLoop,
                GameConstants.IdleAnimMaxLoop
            );
        }

        void Update()
        {
            if (_renderer == null || frame0 == null || frame1 == null)
                return;

            _timer += Time.deltaTime;
            if (_timer >= _loopDuration)
            {
                _timer -= _loopDuration;
                _onFrame1 = !_onFrame1;
                _renderer.sprite = _onFrame1 ? frame1 : frame0;
            }
        }

        /// <summary>
        /// Set frames at runtime (for procedurally created sprites).
        /// </summary>
        public void SetFrames(Sprite f0, Sprite f1)
        {
            frame0 = f0;
            frame1 = f1;
            if (_renderer != null)
                _renderer.sprite = f0;
        }
    }
}
