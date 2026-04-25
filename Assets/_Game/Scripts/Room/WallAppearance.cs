using UnityEngine;

namespace PrismPanic.Room
{
    /// <summary>
    /// Duvarın haritadaki konumuna göre (kenar, köşe vb.) sprite'larını otomatik ayarlar.
    /// Prefab üzerindeki Sprite Renderer'lara referans verilmelidir.
    /// </summary>
    public class WallAppearance : MonoBehaviour
    {
        [Header("Sprite Renderers")]
        [Tooltip("Duvarın üst yüzeyi")]
        [SerializeField] private SpriteRenderer _topRenderer;
        [Tooltip("Duvarın alt yüzeyi")]
        [SerializeField] private SpriteRenderer _bottomRenderer;
        [Tooltip("Duvarın 4 yan yüzeyi")]
        [SerializeField] private SpriteRenderer[] _sideRenderers;

        [Header("Top Edge (Üst Kenar)")]
        [SerializeField] private Sprite _topEdgeTopBottomSprite;

        [Header("Bottom Edge (Alt Kenar)")]
        [SerializeField] private Sprite _bottomEdgeTopBottomSprite;

        [Header("Left & Right Edges (Sağ/Sol Kenar)")]
        [SerializeField] private Sprite _sideEdgeTopBottomSprite;

        [Header("Common Sides (Tüm Kenarların Yan Yüzleri)")]
        [SerializeField] private Sprite _commonSideSprite;

        [Header("Corners (En Köşeler)")]
        [SerializeField] private Sprite _cornerTopBottomSprite;
        [SerializeField] private Sprite _cornerSideSprite;

        [Header("Default (İç Duvarlar)")]
        [SerializeField] private Sprite _defaultTopBottomSprite;
        [SerializeField] private Sprite _defaultSideSprite;

        /// <summary>
        /// RoomConfigurator tarafından duvarın konumuna göre çağrılır.
        /// </summary>
        public void Setup(bool isTop, bool isBottom, bool isLeft, bool isRight)
        {
            bool isCorner = (isTop || isBottom) && (isLeft || isRight);

            Sprite topBottomToUse = _defaultTopBottomSprite;
            Sprite sidesToUse = _defaultSideSprite;

            if (isCorner)
            {
                topBottomToUse = _cornerTopBottomSprite;
                sidesToUse = _cornerSideSprite;
            }
            else if (isTop)
            {
                topBottomToUse = _topEdgeTopBottomSprite;
                sidesToUse = _commonSideSprite;
            }
            else if (isBottom)
            {
                topBottomToUse = _bottomEdgeTopBottomSprite;
                sidesToUse = _commonSideSprite;
            }
            else if (isLeft || isRight)
            {
                topBottomToUse = _sideEdgeTopBottomSprite;
                sidesToUse = _commonSideSprite;
            }

            // Sprite'ları uygula
            if (_topRenderer != null) _topRenderer.sprite = topBottomToUse;
            if (_bottomRenderer != null) _bottomRenderer.sprite = topBottomToUse;

            if (_sideRenderers != null)
            {
                foreach (var renderer in _sideRenderers)
                {
                    if (renderer != null) renderer.sprite = sidesToUse;
                }
            }
        }
    }
}
