using UnityEngine;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.PersistentWorld;

namespace Game.Environment
{
    // Opens the supply crate exactly once, in a way every player in the lobby sees: any player's
    // trigger entry asks the server to flip the crate's networked "toggle" component
    // (NetworkedEntity.Toggle()); the crate only actually plays its open animation when the
    // authoritative delta comes back through NetworkedEntity.OnToggleChanged, so a first-time
    // visitor who joins after it was opened still sees it open (the joining snapshot already
    // carries toggle=true). Once open it never asks the server to toggle again -- _hasSentToggle
    // guards against re-sending while multiple players stand in the trigger, and _isOpen (driven
    // only by the authoritative event, never assumed locally) guards against ever undoing it.
    [RequireComponent(typeof(NetworkedEntity))]
    [RequireComponent(typeof(OneShotPropAnimator))]
    [RequireComponent(typeof(Collider))]
    public class SupplyCrateInteractable : MonoBehaviour
    {
        [SerializeField] private string triggerTag = "Player";

        private NetworkedEntity _entity;
        private OneShotPropAnimator _animator;
        private bool _hasSentToggle;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _entity = GetComponent<NetworkedEntity>();
            _animator = GetComponent<OneShotPropAnimator>();
        }

        private void OnEnable()
        {
            _entity.ToggleChanged += HandleToggleChanged;
        }

        private void OnDisable()
        {
            _entity.ToggleChanged -= HandleToggleChanged;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsOpen || _hasSentToggle) return;
            if (!other.CompareTag(triggerTag)) return;

            _hasSentToggle = true;
            _entity.Toggle();
        }

        // Public (not private) so it can be exercised directly in tests without a live
        // NetworkedEntity/WorldConnection -- mirrors WorldPresenceController.HandleChatMessageReceived.
        public void HandleToggleChanged(bool on)
        {
            if (!on || IsOpen) return;
            IsOpen = true;
            _animator.Play();
        }
    }
}
