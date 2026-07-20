using System.Collections.Generic;
using UnityEngine;
using OneTimeGames.CoreSystems;

namespace Game.Wildlife
{
    public enum WildlifeState
    {
        Wandering,
        Fleeing
    }

    // Drives one spawned wildlife instance: wanders to random points within its home area, and
    // flees directly away from the nearest registered player when it comes within fleeDistance.
    // GlbCharacterAnimator is optional (GetComponent, not RequireComponent) -- a wildlife prefab
    // built by a future "Apply <creature>" design ticket carries one, but this component works
    // with a plain placeholder GameObject too.
    public class WildlifeAgent : MonoBehaviour
    {
        [SerializeField] private float wanderSpeed = 1f;
        [SerializeField] private float fleeSpeed = 4f;
        [SerializeField] private float fleeDistance = 5f;
        [SerializeField] private float arrivalDistance = 0.3f;
        [SerializeField] private float fleeTargetDistance = 8f;
        [SerializeField] private float wanderIntervalSeconds = 4f;

        private Vector3 _areaCenter;
        private Vector3 _areaSize;
        private Vector3 _target;
        private float _nextWanderTime;
        private IReadOnlyList<Transform> _players;
        private GlbCharacterAnimator _animator;
        private bool _initialized;

        public WildlifeState CurrentState { get; private set; } = WildlifeState.Wandering;
        public Vector3 CurrentTarget => _target;

        public void Initialize(Vector3 areaCenter, Vector3 areaSize, IReadOnlyList<Transform> players)
        {
            _areaCenter = areaCenter;
            _areaSize = areaSize;
            _players = players;
            _animator = GetComponent<GlbCharacterAnimator>();
            _target = WildlifeMovement.PickWanderTarget(_areaCenter, _areaSize);
            _nextWanderTime = Time.time + wanderIntervalSeconds;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            var nearestPlayer = WildlifeMovement.FindNearestPlayer(transform.position, _players, out _);

            if (nearestPlayer != null && WildlifeMovement.ShouldFlee(transform.position, nearestPlayer.position, fleeDistance))
            {
                if (CurrentState != WildlifeState.Fleeing)
                {
                    CurrentState = WildlifeState.Fleeing;
                    _target = WildlifeMovement.ComputeFleeTarget(transform.position, nearestPlayer.position, fleeTargetDistance);
                }
                MoveToward(_target, fleeSpeed);
                return;
            }

            if (CurrentState == WildlifeState.Fleeing) CurrentState = WildlifeState.Wandering;

            if (WildlifeMovement.HasReachedTarget(transform.position, _target, arrivalDistance) || Time.time >= _nextWanderTime)
            {
                _target = WildlifeMovement.PickWanderTarget(_areaCenter, _areaSize);
                _nextWanderTime = Time.time + wanderIntervalSeconds;
            }
            MoveToward(_target, wanderSpeed);
        }

        private void MoveToward(Vector3 target, float speed)
        {
            var direction = target - transform.position;
            direction.y = 0f;
            var distance = direction.magnitude;

            if (distance > 0.0001f)
            {
                var moveDistance = Mathf.Min(speed * Time.deltaTime, distance);
                transform.position += direction.normalized * moveDistance;
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }

            transform.position = WildlifeMovement.ClampToArea(transform.position, _areaCenter, _areaSize);

            if (_animator != null) _animator.SetSpeed(speed);
        }
    }
}
