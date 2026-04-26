using UnityEngine;

namespace PrismPanic.Utilities
{
    /// <summary>
    /// A lightweight, generic sprite animator.
    /// Cycles through an array of sprites based on a frame rate.
    /// Does not require an Animator component, perfect for simple VFX like speed trails or stun stars.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleSpriteAnimator : MonoBehaviour
    {
        [Tooltip("The sprites to cycle through in order.")]
        [SerializeField] private Sprite[] _frames;

        [Tooltip("How many frames to show per second (e.g. 12 fps).")]
        [SerializeField] private float _framesPerSecond = 12f;

        [Tooltip("Should the animation loop indefinitely?")]
        [SerializeField] private bool _loop = true;

        [Tooltip("Should the GameObject disable itself after one play? (Ignored if Loop is true)")]
        [SerializeField] private bool _disableOnFinish = false;

        private SpriteRenderer _spriteRenderer;
        private float _timer;
        private int _currentFrameIndex;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            // Reset to the first frame every time this object is enabled
            _currentFrameIndex = 0;
            _timer = 0f;
            
            if (_frames != null && _frames.Length > 0)
            {
                _spriteRenderer.sprite = _frames[0];
            }
        }

        private void Update()
        {
            if (_frames == null || _frames.Length == 0) return;

            _timer += Time.deltaTime;
            float timePerFrame = 1f / _framesPerSecond;

            if (_timer >= timePerFrame)
            {
                _timer -= timePerFrame;
                _currentFrameIndex++;

                if (_currentFrameIndex >= _frames.Length)
                {
                    if (_loop)
                    {
                        _currentFrameIndex = 0; // Loop back to start
                    }
                    else
                    {
                        _currentFrameIndex = _frames.Length - 1; // Stay on last frame
                        if (_disableOnFinish)
                        {
                            gameObject.SetActive(false);
                            return;
                        }
                    }
                }

                _spriteRenderer.sprite = _frames[_currentFrameIndex];
            }
        }
    }
}
