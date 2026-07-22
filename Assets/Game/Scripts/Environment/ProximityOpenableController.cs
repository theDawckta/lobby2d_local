using System.Collections.Generic;
using UnityEngine;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.Presence;
using OneTimeGames.CoreSystems.PersistentWorld;

namespace Game.Environment
{
    // A cave DOOR that OPENS when any player stands near it and CLOSES when nobody is, kept in sync
    // so every client renders the same state. The 2D, reversible, proximity-driven cousin of
    // SupplyCrateInteractable.
    //
    // Race-safety (the whole point): "is any player near the door" is computed from the SHARED
    // WorldPresence roster -- the local player's transform PLUS every RemotePlayer -- so every
    // connected client derives the SAME desired state from the SAME data and sends the SAME
    // idempotent intent. The world server's interaction system treats "open"/"close" as
    // no-op-if-already-there SETS (not blind flips), so simultaneous intents from many clients
    // converge instead of racing. The visual (OneShotSpriteAnimator) is driven ONLY by the
    // authoritative NetworkedEntity.ToggleChanged -- never optimistically -- so a late joiner
    // renders the correct held state from their very first snapshot. Offline (Editor / no backend)
    // it falls back to driving the animator directly so the door still works with no server.
    [RequireComponent(typeof(NetworkedEntity))]
    [RequireComponent(typeof(OneShotSpriteAnimator))]
    public class ProximityOpenableController : MonoBehaviour
    {
        [Tooltip("A player within this many world units (measured on the XZ plane) of the door OPENS it.")]
        [SerializeField] private float openRange = 4f;

        [Tooltip("Hysteresis. Once open, a player must move beyond openRange + this margin before the " +
                 "door closes, so a player hovering right at the edge doesn't make it flicker open/closed.")]
        [SerializeField] private float closeBuffer = 1.5f;

        [Tooltip("Seconds between proximity re-evaluations. The check is cheap; a small interval keeps " +
                 "the door responsive while keeping network intents sparse (we only send on a change).")]
        [SerializeField] private float pollIntervalSeconds = 0.2f;

        [Tooltip("The lobby's WorldPresence (supplies the shared player roster). Auto-found in the scene " +
                 "if left unassigned. WorldPresence lives on the moving local-player GameObject, so its " +
                 "own transform IS the local player's position.")]
        [SerializeField] private WorldPresence presence;

        private NetworkedEntity _entity;
        private OneShotSpriteAnimator _animator;
        private float _pollTimer;
        private bool? _sentOpen;   // last intent we sent to the server (null = nothing sent yet)
        private bool _isOpen;      // authoritative state -- flipped ONLY by HandleToggleChanged

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            _entity = GetComponent<NetworkedEntity>();
            _animator = GetComponent<OneShotSpriteAnimator>();
            if (presence == null) presence = FindFirstObjectByType<WorldPresence>();
        }

        private void OnEnable() { _entity.ToggleChanged += HandleToggleChanged; }
        private void OnDisable() { _entity.ToggleChanged -= HandleToggleChanged; }

        public void SetPresence(WorldPresence p) => presence = p;

        private void Update()
        {
            _pollTimer -= Time.deltaTime;
            if (_pollTimer > 0f) return;
            _pollTimer = pollIntervalSeconds;

            // Hysteresis: harder to LEAVE the trigger than to enter it. Using _isOpen (which is
            // authoritative and therefore identical on every client) for the threshold keeps the
            // desired-state computation consistent across clients, not just this one.
            float range = _isOpen ? openRange + closeBuffer : openRange;
            bool desiredOpen = AnyPlayerInRange(range);

            if (_sentOpen == desiredOpen) return;   // no change -> nothing to send
            _sentOpen = desiredOpen;

            if (_entity.IsConnected)
                _entity.SendInteract(desiredOpen ? "open" : "close");  // idempotent, server-authoritative
            else
                HandleToggleChanged(desiredOpen);   // offline (Editor / no backend): drive locally
        }

        // Any player (local + every remote) within `range` of the door on the XZ plane?
        private bool AnyPlayerInRange(float range)
        {
            if (presence == null) return false;
            var door = transform.position;

            // Local player: WorldPresence sits on the moving local-player GameObject.
            if (WithinXZ(presence.transform.position, door, range)) return true;

            foreach (var kv in presence.RemotePlayers)
            {
                var rp = kv.Value;
                if (rp != null && WithinXZ(rp.transform.position, door, range)) return true;
            }
            return false;
        }

        // Public + static so the range logic is unit-testable without a live roster/connection.
        public static bool WithinXZ(Vector3 a, Vector3 b, float range)
        {
            float dx = a.x - b.x, dz = a.z - b.z;
            return dx * dx + dz * dz <= range * range;
        }

        // Public + static: does ANY of these positions fall within range of the door? (Pure helper
        // mirroring the instance check, so proximity decisions can be exercised in EditMode.)
        public static bool AnyInRange(IEnumerable<Vector3> positions, Vector3 door, float range)
        {
            if (positions == null) return false;
            foreach (var p in positions) if (WithinXZ(p, door, range)) return true;
            return false;
        }

        // Authoritative state application. NetworkedEntity raises this only when the toggle actually
        // flips (and once with the current value on the first snapshot), so a late joiner gets the
        // right held pose. Public so tests can drive it directly without a live NetworkedEntity.
        public void HandleToggleChanged(bool on)
        {
            _isOpen = on;
            // Don't replay an animation for a pose the door is already in. OneShotSpriteAnimator's
            // Open()/Close() always play from the OPPOSITE extreme, so applying the initial "closed"
            // snapshot on spawn via Close() would visibly jump the door to the open frame and animate
            // it shut. The animator already starts on the correct held frame (Awake -> startOpen), so
            // only drive it on an actual state change.
            if (on == _animator.IsOpen) return;
            if (on) _animator.Open(); else _animator.Close();
        }
    }
}
