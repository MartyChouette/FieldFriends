using UnityEngine;

namespace FieldFriends.Core
{
    /// <summary>
    /// GB-style camera: 20x18 tile viewport on an 8x8 grid.
    /// Follows the player, pixel-snapped.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform target;

        [Header("Viewport (GB-like)")]
        [SerializeField] int viewTilesX = 20;
        [SerializeField] int viewTilesY = 18;
        [SerializeField] float tileSize = 8f;

        Camera _cam;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            ConfigureCamera();
        }

        void ConfigureCamera()
        {
            if (_cam == null) return;

            // Set orthographic size to show exactly viewTilesY tiles vertically.
            // orthographicSize = half the viewport height in world units.
            _cam.orthographic = true;
            _cam.orthographicSize = (viewTilesY * tileSize) / 2f;
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 pos = target.position;
            // Pixel-snap: round to nearest pixel (1 unit = 1 pixel at tileSize 8)
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            pos.z = transform.position.z;

            transform.position = pos;
        }

        public void SetTarget(Transform t)
        {
            target = t;
        }

        /// <summary>
        /// World-space dimensions of the viewport.
        /// </summary>
        public Vector2 ViewportWorldSize =>
            new Vector2(viewTilesX * tileSize, viewTilesY * tileSize);
    }
}
