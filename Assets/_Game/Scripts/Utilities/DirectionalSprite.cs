using UnityEngine;

namespace PrismPanic.Utilities
{
    /// <summary>
    /// 8-directional sprite system. Left directions auto-flip from Right sprites.
    /// Each direction can have a different number of animation frames.
    /// Player: assign only Front, FrontRight, Right, BackRight, Back (left = flipped right).
    /// Angel: same principle, 1 frame per direction.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DirectionalSprite : MonoBehaviour
    {
        [Header("5-Direction Sprites (Left = Flipped Right)")]
        [Tooltip("Kameraya bakıyor (aşağı yürüme)")]
        [SerializeField] private Sprite[] _front;
        [Tooltip("Sağ-öne bakıyor")]
        [SerializeField] private Sprite[] _frontRight;
        [Tooltip("Sağa bakıyor")]
        [SerializeField] private Sprite[] _right;
        [Tooltip("Sağ-arkaya bakıyor")]
        [SerializeField] private Sprite[] _backRight;
        [Tooltip("Kameradan uzağa bakıyor (yukarı yürüme)")]
        [SerializeField] private Sprite[] _back;

        [Header("Animation")]
        [SerializeField] private float _frameRate = 8f;
        [SerializeField] private bool _animateOnlyWhenMoving = true;

        [Header("Billboard")]
        [SerializeField] private bool _billboardToCamera = true;

        private SpriteRenderer _spriteRenderer;
        private Camera _mainCamera;
        private Transform _parentTransform;

        // 0=front, 1=frontRight, 2=right, 3=backRight, 4=back, 5=backLeft(flip), 6=left(flip), 7=frontLeft(flip)
        private int _currentDirection;
        private int _currentFrame;
        private float _frameTimer;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _mainCamera = Camera.main;
            _parentTransform = transform.parent;
        }

        private void Start()
        {
            if (_parentTransform != null)
                _lastPosition = _parentTransform.position;
        }

        private void LateUpdate()
        {
            if (_mainCamera == null || _parentTransform == null) return;

            // --- Direction ---
            Vector3 camForward = _mainCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 parentForward = _parentTransform.forward;
            parentForward.y = 0f;
            parentForward.Normalize();

            float angle = Vector3.SignedAngle(camForward, parentForward, Vector3.up);
            if (angle < 0f) angle += 360f;

            float shifted = (angle + 22.5f) % 360f;
            int sliceIndex = (int)(shifted / 45f);

            // Remap raw slice to our direction enum
            // slice 0 (0°)   = back
            // slice 1 (45°)  = backRight
            // slice 2 (90°)  = right
            // slice 3 (135°) = frontRight
            // slice 4 (180°) = front
            // slice 5 (225°) = frontLeft  → use frontRight + flip
            // slice 6 (270°) = left       → use right + flip
            // slice 7 (315°) = backLeft   → use backRight + flip
            int[] remapTable = { 4, 3, 2, 1, 0, 7, 6, 5 };
            _currentDirection = remapTable[sliceIndex % 8];

            // Get the correct sprite array and determine flip
            Sprite[] dirSprites = GetSpritesForDirection(_currentDirection, out bool shouldFlip);
            if (dirSprites == null || dirSprites.Length == 0) return;

            // Flip X for left-side directions
            _spriteRenderer.flipX = shouldFlip;

            // --- Animation ---
            bool isMoving = true;
            if (_animateOnlyWhenMoving)
            {
                Vector3 delta = _parentTransform.position - _lastPosition;
                delta.y = 0f;
                isMoving = delta.sqrMagnitude > 0.0001f;
                _lastPosition = _parentTransform.position;
            }

            if (isMoving && dirSprites.Length > 1)
            {
                _frameTimer += Time.deltaTime * _frameRate;
                if (_frameTimer >= 1f)
                {
                    _frameTimer -= 1f;
                    _currentFrame = (_currentFrame + 1) % dirSprites.Length;
                }
            }
            else if (!isMoving)
            {
                _currentFrame = 0;
                _frameTimer = 0f;
            }

            if (_currentFrame >= dirSprites.Length)
                _currentFrame = 0;

            _spriteRenderer.sprite = dirSprites[_currentFrame];

            // --- Billboard ---
            if (_billboardToCamera)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }
        }

        [Header("Evil Sprites (Optional)")]
        [Tooltip("Kameraya bakıyor (aşağı yürüme)")]
        [SerializeField] private Sprite[] _evilFront;
        [Tooltip("Sağ-öne bakıyor")]
        [SerializeField] private Sprite[] _evilFrontRight;
        [Tooltip("Sağa bakıyor")]
        [SerializeField] private Sprite[] _evilRight;
        [Tooltip("Sağ-arkaya bakıyor")]
        [SerializeField] private Sprite[] _evilBackRight;
        [Tooltip("Kameradan uzağa bakıyor (yukarı yürüme)")]
        [SerializeField] private Sprite[] _evilBack;

        private bool _isEvil = false;

        public void SetEvilState(bool isEvil)
        {
            _isEvil = isEvil;
        }

        /// <summary>
        /// Returns the sprite array for a given direction index.
        /// Left-side directions (5,6,7) return the mirrored right-side sprites + flip=true.
        /// </summary>
        private Sprite[] GetSpritesForDirection(int dir, out bool flip)
        {
            flip = false;

            Sprite[] front = _isEvil && _evilFront != null && _evilFront.Length > 0 ? _evilFront : _front;
            Sprite[] frontRight = _isEvil && _evilFrontRight != null && _evilFrontRight.Length > 0 ? _evilFrontRight : _frontRight;
            Sprite[] right = _isEvil && _evilRight != null && _evilRight.Length > 0 ? _evilRight : _right;
            Sprite[] backRight = _isEvil && _evilBackRight != null && _evilBackRight.Length > 0 ? _evilBackRight : _backRight;
            Sprite[] back = _isEvil && _evilBack != null && _evilBack.Length > 0 ? _evilBack : _back;

            switch (dir)
            {
                case 0: return front;
                case 1: return frontRight;
                case 2: return right;
                case 3: return backRight;
                case 4: return back;

                // Left-side: use right counterpart + flip
                case 5: // backLeft → backRight flipped
                    flip = true;
                    return backRight;
                case 6: // left → right flipped
                    flip = true;
                    return right;
                case 7: // frontLeft → frontRight flipped
                    flip = true;
                    return frontRight;

                default: return front;
            }
        }
    }
}
