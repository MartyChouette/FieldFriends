using UnityEngine;

namespace FieldFriends.Core
{
    /// <summary>
    /// Consolidated input handling. Reads raw input and exposes
    /// clean directional + action state. All game code reads from here
    /// instead of checking Input directly.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public Vector2Int Direction { get; private set; }
        public bool Confirm { get; private set; }
        public bool Cancel { get; private set; }
        public bool Pause { get; private set; }

        // True for exactly one frame
        public bool ConfirmDown { get; private set; }
        public bool CancelDown { get; private set; }
        public bool PauseDown { get; private set; }

        bool _inputEnabled = true;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (!enabled)
            {
                Direction = Vector2Int.zero;
                Confirm = false;
                Cancel = false;
                Pause = false;
                ConfirmDown = false;
                CancelDown = false;
                PauseDown = false;
            }
        }

        void Update()
        {
            if (!_inputEnabled)
            {
                ConfirmDown = false;
                CancelDown = false;
                PauseDown = false;
                return;
            }

            // Direction (grid: one axis at a time, priority to horizontal)
            int h = 0, v = 0;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) h = 1;
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) h = -1;

            if (h == 0)
            {
                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) v = 1;
                else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) v = -1;
            }

            Direction = new Vector2Int(h, v);

            // Action buttons
            Confirm = Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Return);
            Cancel = Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Escape);
            Pause = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.Tab);

            ConfirmDown = Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return);
            CancelDown = Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape);
            PauseDown = Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Tab);
        }
    }
}
