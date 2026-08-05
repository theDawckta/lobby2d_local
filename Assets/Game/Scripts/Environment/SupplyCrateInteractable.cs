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

        [Tooltip("VfxRecipe prefab instantiated once the lid finishes opening (OnHold, not OnPlay).")]
        [SerializeField] private GameObject coinSpillPrefab;

        [Tooltip("Where the coin-spill VFX spawns. Defaults to this crate's own transform if unset.")]
        [SerializeField] private Transform coinSpawnPoint;

        private NetworkedEntity _entity;
        private OneShotPropAnimator _animator;
        private bool _hasSentToggle;
        private bool _hasSpawnedCoins;

        public bool IsOpen { get; private set; }

        // Test-only wiring hook (mirrors ProximityOpenableController.SetPresence) so PlayMode
        // tests can exercise the coin-spill spawn path without a prefab asset reference.
        public void ConfigureCoinSpill(GameObject prefab, Transform spawnPoint = null)
        {
            coinSpillPrefab = prefab;
            coinSpawnPoint = spawnPoint;
        }

        private void Awake()
        {
            _entity = GetComponent<NetworkedEntity>();
            _animator = GetComponent<OneShotPropAnimator>();
        }

        private void OnEnable()
        {
            _entity.ToggleChanged += HandleToggleChanged;
            _animator.OnHold.AddListener(HandleCrateOpened);
        }

        private void OnDisable()
        {
            _entity.ToggleChanged -= HandleToggleChanged;
            _animator.OnHold.RemoveListener(HandleCrateOpened);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsOpen || _hasSentToggle) return;
            if (!other.CompareTag(triggerTag)) return;

            _hasSentToggle = true;
            if (_entity.IsConnected)
                _entity.Toggle();          // online: server-authoritative -- opens when the delta returns
            else
                HandleToggleChanged(true);  // offline (Editor / no backend): open optimistically & locally
        }

        // Public (not private) so it can be exercised directly in tests without a live
        // NetworkedEntity/WorldConnection -- mirrors WorldPresenceController.HandleChatMessageReceived.
        public void HandleToggleChanged(bool on)
        {
            if (!on || IsOpen) return;
            IsOpen = true;
            _animator.Play();
        }

        // OneShotPropAnimator.OnHold fires once the lid animation actually reaches its held-open
        // pose, not when it starts swinging -- so coins spill out of a visibly-open crate. Every
        // connected client calls Play() at most once (guarded above), so this fires at most once
        // per client too; _hasSpawnedCoins is a defensive second guard so a redundant OnHold call
        // (e.g. multiple players triggering the crate at once) never spawns the effect twice.
        public void HandleCrateOpened()
        {
            if (_hasSpawnedCoins) return;
            _hasSpawnedCoins = true;
            SpawnCoinSpill();
        }

        private void SpawnCoinSpill()
        {
            if (coinSpillPrefab == null) return;
            var spawnPoint = coinSpawnPoint != null ? coinSpawnPoint : transform;
            var instance = Instantiate(coinSpillPrefab, spawnPoint.position, spawnPoint.rotation);
            var recipe = instance.GetComponent<VfxRecipe>();
            if (recipe != null) recipe.Play();
        }
    }
}
