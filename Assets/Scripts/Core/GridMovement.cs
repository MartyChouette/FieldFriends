using UnityEngine;
using System;

namespace FieldFriends.Core
{
    /// <summary>
    /// Grid-step walking on an 8x8 tile grid.
    /// Movement is discrete: the player snaps tile-to-tile.
    /// Walking is the point -- no fast travel, no run button.
    /// </summary>
    public class GridMovement : MonoBehaviour
    {
        public static event Action<Vector2Int> OnStepTaken;

        [Header("Grid Settings")]
        [SerializeField] float tileSize = 8f;
        [SerializeField] float moveSpeed = 4f; // tiles per second

        Vector2Int _gridPos;
        Vector3 _targetWorldPos;
        bool _isMoving;
        Vector2Int _facingDir;

        public Vector2Int GridPosition => _gridPos;
        public Vector2Int FacingDirection => _facingDir;
        public bool IsMoving => _isMoving;

        void Start()
        {
            // Snap to nearest grid position on start
            _gridPos = WorldToGrid(transform.position);
            _targetWorldPos = GridToWorld(_gridPos);
            transform.position = _targetWorldPos;
            _facingDir = Vector2Int.down;
        }

        void Update()
        {
            if (_isMoving)
            {
                float step = moveSpeed * tileSize * Time.deltaTime;
                transform.position = Vector3.MoveTowards(
                    transform.position, _targetWorldPos, step);

                if (Vector3.Distance(transform.position, _targetWorldPos) < 0.01f)
                {
                    transform.position = _targetWorldPos;
                    _isMoving = false;
                    OnStepTaken?.Invoke(_gridPos);
                }
                return;
            }

            Vector2Int input = ReadInput();
            if (input != Vector2Int.zero)
            {
                _facingDir = input;
                TryMove(input);
            }
        }

        Vector2Int ReadInput()
        {
            // Cardinal only, no diagonals
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                return Vector2Int.up;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                return Vector2Int.down;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                return Vector2Int.left;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                return Vector2Int.right;
            return Vector2Int.zero;
        }

        void TryMove(Vector2Int direction)
        {
            Vector2Int target = _gridPos + direction;

            if (!IsWalkable(target))
                return;

            _gridPos = target;
            _targetWorldPos = GridToWorld(_gridPos);
            _isMoving = true;
        }

        bool IsWalkable(Vector2Int gridPos)
        {
            // Raycast on the Obstacles layer to check walkability
            Vector3 worldPos = GridToWorld(gridPos);
            Collider2D hit = Physics2D.OverlapPoint(worldPos, LayerMask.GetMask("Obstacles"));
            return hit == null;
        }

        public Vector3 GridToWorld(Vector2Int grid)
        {
            // Pixel-perfect positioning: center of tile
            return new Vector3(
                grid.x * tileSize + tileSize * 0.5f,
                grid.y * tileSize + tileSize * 0.5f,
                0f
            );
        }

        Vector2Int WorldToGrid(Vector3 world)
        {
            return new Vector2Int(
                Mathf.FloorToInt(world.x / tileSize),
                Mathf.FloorToInt(world.y / tileSize)
            );
        }

        /// <summary>
        /// Teleport to a grid position (used for area transitions).
        /// </summary>
        public void SetGridPosition(Vector2Int pos)
        {
            _gridPos = pos;
            _targetWorldPos = GridToWorld(pos);
            transform.position = _targetWorldPos;
            _isMoving = false;
        }

        /// <summary>
        /// Lock/unlock movement (for battles, dialogue, menus).
        /// </summary>
        public void SetMovementLocked(bool locked)
        {
            enabled = !locked;
            if (locked)
                _isMoving = false;
        }
    }
}
