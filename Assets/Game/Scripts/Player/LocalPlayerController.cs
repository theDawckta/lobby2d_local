using UnityEngine;
using UnityEngine.InputSystem;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.Presence;

namespace Game.Player
{
    // Drives the local player's own movement from keyboard (desktop) and a CoreSystems
    // VirtualAnalogStick (touch -- and, per TouchControls' own pointer-event design, also the
    // mouse when the stick is visible, e.g. in the Editor) and pushes the resulting position/yaw
    // to WorldPresence each frame so other players see it. Ground plane is X/Z, Y is held fixed.
    //
    // touchStick and worldPresence are resolved automatically from the scene if left unassigned,
    // matching the pattern already established by WorldConnectController/WorldPresenceController --
    // scene wiring (which stick, which PanelSettings) belongs to the Main Scene assembly ticket,
    // not this one.
    public class LocalPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;

        [Tooltip("Degrees/sec this transform turns toward the input-facing direction. The 2D billboard " +
                 "sprite ignores this (it always re-faces the camera and picks its sprite frame from " +
                 "movement velocity, not this transform's rotation) -- it only matters for a 3D avatar " +
                 "(e.g. GlbCharacterAnimator) parented under this transform, which would otherwise " +
                 "inherit an instant snap-to-facing every time input direction changes.")]
        [SerializeField] private float turnSpeedDegrees = 540f;

        [Tooltip("Resolved automatically from the scene if left unassigned.")]
        [SerializeField] private VirtualAnalogStick touchStick;

        [Tooltip("Resolved automatically from this GameObject/the scene if left unassigned.")]
        [SerializeField] private WorldPresence worldPresence;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        public float TurnSpeedDegrees
        {
            get => turnSpeedDegrees;
            set => turnSpeedDegrees = value;
        }

        public VirtualAnalogStick TouchStick
        {
            get => touchStick;
            set => touchStick = value;
        }

        public WorldPresence Presence
        {
            get => worldPresence;
            set => worldPresence = value;
        }

        public Vector3 Velocity { get; private set; }

        private void Awake()
        {
            if (touchStick == null) touchStick = FindFirstObjectByType<VirtualAnalogStick>();
            if (worldPresence == null) worldPresence = GetComponent<WorldPresence>();
            if (worldPresence == null) worldPresence = FindFirstObjectByType<WorldPresence>();
        }

        private void Start()
        {
            // Register with the wildlife manager so animals (2D + 3D) actually flee from the player.
            // Nothing else called RegisterPlayer, so the flee list was always empty.
            var wildlife = FindFirstObjectByType<Game.Wildlife.WildlifeManager>();
            if (wildlife != null) wildlife.RegisterPlayer(transform);
        }

        private void Update()
        {
            Move(ReadInput(), Time.deltaTime);
        }

        // Reads the combined keyboard + touch-stick input for this frame. Public so a test (or a
        // future input-remapping feature) can exercise it directly without simulating real devices.
        public Vector2 ReadInput()
        {
            var keyboardAxis = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                keyboardAxis = LocalPlayerMovement.ComputeKeyboardAxis(
                    keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
                    keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed,
                    keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed,
                    keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed);
            }

            var touchAxis = touchStick != null ? touchStick.Value : Vector2.zero;
            return LocalPlayerMovement.CombineInput(keyboardAxis, touchAxis);
        }

        // The testable core: applies moveInput to this transform's position/yaw and syncs the
        // result to WorldPresence. Also callable directly (tests, or a future non-input-driven
        // mover) since Update()'s real device reads can't be simulated without live hardware.
        public void Move(Vector2 moveInput, float deltaTime)
        {
            Velocity = LocalPlayerMovement.ComputeVelocity(moveInput, moveSpeed);
            transform.position = LocalPlayerMovement.ComputeNextPosition(transform.position, Velocity, deltaTime);

            // The exact instantaneous target yaw -- sent to the network unchanged from before, so
            // remote-view responsiveness/position-sync timing isn't affected by the visual smoothing
            // below.
            var targetYaw = LocalPlayerMovement.ComputeYaw(moveInput, transform.eulerAngles.y);
            if (worldPresence != null) worldPresence.UpdateLocalTransform(transform.position, targetYaw);

            // This transform's own rotation turns toward targetYaw at a finite rate instead of
            // snapping to it every frame. See turnSpeedDegrees' tooltip for why this only matters
            // once a 3D (non-billboard) avatar exists.
            var smoothedYaw = LocalPlayerMovement.ComputeSmoothedYaw(transform.eulerAngles.y, targetYaw, turnSpeedDegrees, deltaTime);
            transform.rotation = Quaternion.Euler(0f, smoothedYaw, 0f);
        }
    }
}
