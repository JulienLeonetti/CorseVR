using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CorseVR
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class DesktopFirstPersonExplorer : MonoBehaviour
    {
        [SerializeField] private Transform viewCamera;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
        [SerializeField, Min(0.01f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Range(1f, 89f)] private float pitchLimit = 80f;
        [SerializeField] private float gravity = -20f;

        private CharacterController controller;
        private SentierCorridor corridor;
        private Keyboard cachedKeyboard;
        private string cachedLayout;
        private KeyControl forwardKey, leftKey, backKey, rightKey;
        private float pitch;
        private float verticalSpeed;
        private bool skipMouseDelta;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            corridor = GetComponent<SentierCorridor>();
            if (viewCamera == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                if (childCamera != null) viewCamera = childCamera.transform;
            }
            if (viewCamera == null)
            {
                Debug.LogError("DesktopFirstPersonExplorer requires a child camera.", this);
                enabled = false;
                return;
            }
            pitch = Mathf.DeltaAngle(0f, viewCamera.localEulerAngles.x);
        }

        private void Start()
        {
            if (Application.isFocused) CaptureMouse();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            bool escaped = keyboard != null && keyboard.escapeKey.isPressed;
            if (escaped)
                ReleaseMouse();
            else if (Application.isFocused && Cursor.lockState != CursorLockMode.Locked
                     && mouse != null && mouse.leftButton.wasPressedThisFrame)
                CaptureMouse();

            bool exploring = Application.isFocused && Cursor.lockState == CursorLockMode.Locked;
            if (exploring && mouse != null)
            {
                if (!skipMouseDelta) ApplyLook(mouse.delta.ReadValue());
                skipMouseDelta = false;
            }
            MovePlayer(exploring ? ReadMovement(keyboard) : Vector2.zero, Time.deltaTime);
        }

        private Vector2 ReadMovement(Keyboard keyboard)
        {
            if (keyboard == null) return Vector2.zero;
            if (cachedKeyboard != keyboard || cachedLayout != keyboard.keyboardLayout)
            {
                cachedKeyboard = keyboard;
                cachedLayout = keyboard.keyboardLayout;
                // Resolve letters by the current layout: AZERTY Z/Q occupy physical W/A.
                forwardKey = keyboard.FindKeyOnCurrentKeyboardLayout("z") ?? keyboard.zKey;
                leftKey = keyboard.FindKeyOnCurrentKeyboardLayout("q") ?? keyboard.qKey;
                backKey = keyboard.FindKeyOnCurrentKeyboardLayout("s") ?? keyboard.sKey;
                rightKey = keyboard.FindKeyOnCurrentKeyboardLayout("d") ?? keyboard.dKey;
            }

            float x = (rightKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                    - (leftKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float y = (forwardKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                    - (backKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }

        private void ApplyLook(Vector2 delta)
        {
            transform.Rotate(0f, delta.x * mouseSensitivity, 0f, Space.Self);
            pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, -pitchLimit, pitchLimit);
            viewCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void MovePlayer(Vector2 input, float deltaTime)
        {
            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed = Mathf.Max(verticalSpeed + gravity * deltaTime, -40f);
            Vector3 horizontal = (transform.right * input.x + transform.forward * input.y) * moveSpeed;
            Vector3 displacement = (horizontal + Vector3.up * verticalSpeed) * deltaTime;
            if (corridor != null) displacement = corridor.ConstrainPosition(transform.position + displacement) - transform.position;
            CollisionFlags flags = controller.Move(displacement);
            if ((flags & CollisionFlags.Below) != 0) verticalSpeed = -2f;
            if ((flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
        }

        public void ResetAfterTeleport()
        {
            verticalSpeed = -2f;
            skipMouseDelta = true;
        }
        private void CaptureMouse()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            skipMouseDelta = true;
        }

        private void ReleaseMouse()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            skipMouseDelta = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) ReleaseMouse();
        }

        private void OnDisable()
        {
            ReleaseMouse();
        }
    }
}
