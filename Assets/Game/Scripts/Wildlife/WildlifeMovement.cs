using System.Collections.Generic;
using UnityEngine;

namespace Game.Wildlife
{
    // Pure, stateless wander/flee movement math -- kept separate from WildlifeAgent's MonoBehaviour
    // lifecycle so it can be unit tested in EditMode without spinning up GameObjects.
    public static class WildlifeMovement
    {
        public static Vector3 PickWanderTarget(Vector3 areaCenter, Vector3 areaSize)
        {
            var half = areaSize * 0.5f;
            return areaCenter + new Vector3(
                Random.Range(-half.x, half.x),
                0f,
                Random.Range(-half.z, half.z));
        }

        public static bool HasReachedTarget(Vector3 position, Vector3 target, float arrivalDistance)
        {
            var flat = new Vector3(target.x - position.x, 0f, target.z - position.z);
            return flat.sqrMagnitude <= arrivalDistance * arrivalDistance;
        }

        public static Vector3 ClampToArea(Vector3 position, Vector3 areaCenter, Vector3 areaSize)
        {
            var half = areaSize * 0.5f;
            return new Vector3(
                Mathf.Clamp(position.x, areaCenter.x - half.x, areaCenter.x + half.x),
                position.y,
                Mathf.Clamp(position.z, areaCenter.z - half.z, areaCenter.z + half.z));
        }

        public static bool ShouldFlee(Vector3 selfPosition, Vector3 playerPosition, float fleeDistance)
        {
            var flat = new Vector3(playerPosition.x - selfPosition.x, 0f, playerPosition.z - selfPosition.z);
            return flat.sqrMagnitude <= fleeDistance * fleeDistance;
        }

        public static Vector3 ComputeFleeTarget(Vector3 selfPosition, Vector3 playerPosition, float fleeTargetDistance)
        {
            var away = selfPosition - playerPosition;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) away = Vector3.forward; // player exactly on top -- pick a direction
            return selfPosition + away.normalized * fleeTargetDistance;
        }

        public static Transform FindNearestPlayer(Vector3 selfPosition, IReadOnlyList<Transform> players, out float nearestDistance)
        {
            Transform nearest = null;
            nearestDistance = float.MaxValue;
            if (players == null) return null;

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null) continue;
                var flat = new Vector3(player.position.x - selfPosition.x, 0f, player.position.z - selfPosition.z);
                var dist = flat.magnitude;
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearest = player;
                }
            }
            return nearest;
        }
    }
}
