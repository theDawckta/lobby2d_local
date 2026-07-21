using System.Collections.Generic;
using UnityEngine;
using OneTimeGames.CoreSystems;
using Game.Audio;

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

        // Explicit, per-agent SFX name (never derived from the prefab/GameObject's name -- see
        // CLAUDE.md's naming-convention rule) to play once when this agent starts fleeing. Left
        // empty by default; WildlifeManager assigns it per spawn entry from its own configured data.
        [SerializeField] private string fleeSfxName = "";

        [Tooltip("If > 0, this creature FLIES: it is lifted to this world Y and wanders at that height " +
                 "instead of on the floor (e.g. a bird). Movement stays horizontal, so it holds the height.")]
        [SerializeField] private float flyHeight = 0f;

        [Tooltip("How fast the creature can turn (degrees/sec). It steers its heading toward its target " +
                 "at this rate and moves ALONG that heading, so it follows a curved path instead of " +
                 "snapping to face a new direction instantly. Applies to both 2D and 3D creatures.")]
        [SerializeField] private float turnSpeedDegreesPerSecond = 140f;

        private Vector3 _heading = Vector3.forward;   // current travel direction, steered gradually

        private Vector3 _areaCenter;
        private Vector3 _areaSize;
        private Vector3 _target;
        private float _nextWanderTime;
        private IReadOnlyList<Transform> _players;
        private GlbCharacterAnimator _animator;
        private bool _initialized;

        public WildlifeState CurrentState { get; private set; } = WildlifeState.Wandering;
        public Vector3 CurrentTarget => _target;

        public string FleeSfxName
        {
            get => fleeSfxName;
            set => fleeSfxName = value;
        }

        public void Initialize(Vector3 areaCenter, Vector3 areaSize, IReadOnlyList<Transform> players)
        {
            _areaCenter = areaCenter;
            _areaSize = areaSize;
            _players = players;
            _animator = GetComponent<GlbCharacterAnimator>();
            // Flyers (birds) operate at flyHeight: lift once here. MoveToward zeroes the vertical
            // component, so the creature holds this Y for the rest of its life.
            if (flyHeight > 0f)
            {
                var pos = transform.position; pos.y = flyHeight; transform.position = pos;
                _areaCenter.y = flyHeight;
            }
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
                    if (!string.IsNullOrEmpty(fleeSfxName)) AudioManager.Instance?.PlaySFX(fleeSfxName);
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
            var desired = target - transform.position;
            desired.y = 0f;

            if (desired.sqrMagnitude > 0.0001f)
            {
                // Steer the current heading TOWARD the target at a limited turn rate, then move ALONG
                // that heading -- so the creature banks through a curve instead of snapping to face the
                // new target instantly. The gradually-changing heading is what makes both the 3D model
                // rotation and the 2D sprite direction (derived from velocity) change smoothly.
                var desiredDir = desired.normalized;
                if (_heading.sqrMagnitude < 0.0001f) _heading = desiredDir;
                _heading = Vector3.RotateTowards(_heading, desiredDir,
                    Mathf.Deg2Rad * turnSpeedDegreesPerSecond * Time.deltaTime, 0f).normalized;
                transform.position += _heading * (speed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(_heading, Vector3.up);
            }

            transform.position = WildlifeMovement.ClampToArea(transform.position, _areaCenter, _areaSize);

            if (_animator != null) _animator.SetSpeed(speed);
        }
    }
}
