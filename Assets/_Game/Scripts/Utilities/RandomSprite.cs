using UnityEngine;

namespace PrismPanic.Utilities
{
    /// <summary>
    /// Picks a random sprite from an array each time the object is enabled.
    /// Attach to floor tile prefab with a SpriteRenderer.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class RandomSprite : MonoBehaviour
    {
        [SerializeField] private Sprite[] _sprites;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (_sprites != null && _sprites.Length > 0)
            {
                _spriteRenderer.sprite = _sprites[Random.Range(0, _sprites.Length)];
            }
        }
    }
}
