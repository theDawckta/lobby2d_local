using UnityEngine;

namespace Game.Player
{
    // Pure, stateless movement math -- kept separate from LocalPlayerController's MonoBehaviour
    // lifecycle so it can be unit tested in EditMode without spinning up GameObjects. Mirrors the
    // WildlifeMovement convention (ground plane on X/Z, Y fixed).
    public static class LocalPlayerMovement
    {
        // Digital keyboard axis: forward/back drive Z, left/right drive X. Diagonals are
        // normalized so moving on two axes at once isn't faster than moving on one.
        public static Vector2 ComputeKeyboardAxis(bool forward, bool back, bool left, bool right)
        {
            var axis = new Vector2(
                (right ? 1f : 0f) - (left ? 1f : 0f),
                (forward ? 1f : 0f) - (back ? 1f : 0f));
            return axis.sqrMagnitude > 1f ? axis.normalized : axis;
        }

        // Combines keyboard and touch-stick input into a single move direction, clamped to a
        // magnitude of 1 so simultaneous input from both sources can't move faster than either alone.
        public static Vector2 CombineInput(Vector2 keyboardAxis, Vector2 touchAxis)
        {
            var combined = keyboardAxis + touchAxis;
            return combined.magnitude > 1f ? combined.normalized : combined;
        }

        public static Vector3 ComputeVelocity(Vector2 moveInput, float moveSpeed)
        {
            return new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        }

        public static Vector3 ComputeNextPosition(Vector3 currentPosition, Vector3 velocity, float deltaTime)
        {
            return currentPosition + velocity * deltaTime;
        }

        // Faces the direction of movement (Unity's LookRotation convention: 0 degrees = +Z, 90 = +X).
        // Holds the previous yaw while there is no movement input, so the player doesn't snap to a
        // default facing when standing still.
        public static float ComputeYaw(Vector2 moveInput, float currentYaw)
        {
            if (moveInput.sqrMagnitude < 0.0001f) return currentYaw;
            return Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
        }
    }
}
