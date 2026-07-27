using System;
using System.Collections;
using UnityEngine;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.Presence;
using Game.Audio;

namespace Game.World
{
    // Wires CoreSystems' WorldPresence (roster spawn/despawn, position interpolation, chat/whisper/
    // emote) to this game's runtime config + player identity, mirroring the AuthConnectController /
    // WorldConnectController pattern: charactersBaseUrl comes from ConfigService (Rule C),
    // LocalUsername comes from FactoryAuth once it resolves (recognizes the local player's own chat
    // echo). WorldPresence itself auto-finds the scene's WorldConnection, so no explicit wiring is
    // needed there. Attach to the local player GameObject -- Update() continuously pushes this
    // GameObject's own transform as the local player's position (WorldPresence throttles the actual
    // network send and no-ops while disconnected).
    [RequireComponent(typeof(WorldPresence))]
    public class WorldPresenceController : MonoBehaviour
    {
        [Tooltip("Resolved automatically from the scene if left unassigned.")]
        [SerializeField] private FactoryAuth auth;

        [Tooltip("Optional: shows the local player's own chat bubble when their message echoes back.")]
        [SerializeField] private ChatBubble localChatBubble;

        [Tooltip("Optional: the local player's speech thought-bubble. Resolved automatically from " +
                 "this GameObject if left unassigned. Its height is kept pinned above the avatar's " +
                 "actual rendered top (see UpdateBubbleHeights), same as localChatBubble.")]
        [SerializeField] private ThoughtBubble localThoughtBubble;

        [Tooltip("Extra world-space clearance above the avatar's measured top (its SpriteRenderer's " +
                 "own rendered bounds, not a guessed unit height) before the bubble sits.")]
        [SerializeField] private float bubbleHeightPadding = 0.15f;

        [Tooltip("Seconds between footstep SFX while the local player is moving.")]
        [SerializeField] private float footstepIntervalSeconds = 0.6f;

        [Tooltip("Minimum distance moved in a frame to count as walking, for footstep SFX.")]
        [SerializeField] private float footstepMoveThreshold = 0.01f;

        [Tooltip("World-space scale for player avatars. Character sprites are 1 unit tall; this " +
                 "scene's props (deer ~7 units) were assembled at a larger scale, so avatars must " +
                 "be scaled up to read as human-sized next to them.")]
        [SerializeField] private float avatarScale = 5f;

        [Tooltip("Character shown for the local player when no real one is resolved yet (Editor with " +
                 "no backend, or before the server roster arrives). 'dummy' is the universal guest.")]
        [SerializeField] private string fallbackCharacterName = "dummy";

        [Tooltip("Sprite host used in the Editor when no config.json is served, so the fallback avatar " +
                 "loads real sprites instead of a white quad. This only fetches public sprite PNGs -- " +
                 "it does NOT connect the Editor to the live world server.")]
        [SerializeField] private string editorCharactersBaseUrl = "https://factory.tehfaktoree.com";

        [Tooltip("Animation played while a player is moving. 'walking' suits a calm lobby better than 'run'.")]
        [SerializeField] private string moveAnimation = "walking";

        [Tooltip("Animation played while a player stands still.")]
        [SerializeField] private string idleAnimation = "idle";

        private Vector3 _lastPosition;
        private bool _hasLastPosition;
        private float _footstepTimer;

        public WorldPresence Presence { get; private set; }

        public FactoryAuth Auth
        {
            get => auth;
            set => auth = value;
        }

        public ChatBubble LocalChatBubble
        {
            get => localChatBubble;
            set => localChatBubble = value;
        }

        public ThoughtBubble LocalThoughtBubble
        {
            get => localThoughtBubble;
            set => localThoughtBubble = value;
        }

        // Fired when this game object sends an emote -- the server broadcasts emotes to everyone
        // EXCEPT the sender, so the local player must play their own emote locally on this hook.
        public event Action<string> OnLocalEmoteRequested;

        private void Awake()
        {
            Presence = GetComponent<WorldPresence>();
            if (auth == null) auth = FindFirstObjectByType<FactoryAuth>();
            if (localThoughtBubble == null) localThoughtBubble = GetComponent<ThoughtBubble>();

            // WorldPresence spawns avatars for REMOTE players only; opt in to rendering the local
            // player's own billboard too (this component lives on the moving LocalPlayer GameObject,
            // which is exactly what renderLocalAvatar requires) so the player can see themselves.
            Presence.renderLocalAvatar = true;
            Presence.avatarScale = avatarScale;
            // NOTE: localFallbackCharacterName is set in Start(), AFTER charactersBaseUrl is resolved --
            // setting it here would let WorldPresence.Update() eager-spawn the fallback avatar before
            // the sprite host is known, leaving it stuck as a white quad.
        }

        Transform _measuredAvatarTransform;
        SpriteRenderer _avatarRenderer;

        // Pins the local chat/thought bubbles just above the avatar's ACTUAL rendered top edge,
        // measured from its SpriteRenderer's own world-space bounds -- not a guessed "characters are
        // N units tall" constant times avatarScale. That guessed-constant approach was tried first and
        // landed the bubble either at the character's thighs (guessed too low) or far above its head
        // (guessed too high, ~3x avatarScale=5 units), because CoreSystems' ChatBubble/ThoughtBubble
        // default height assumes a baseline sprite scale this scene doesn't reliably match. Measuring
        // the real bounds is correct regardless of avatarScale, character, or sprite import settings.
        //
        // Both bubbles live directly on this (unscaled) root GameObject rather than on the local
        // avatar child WorldPresence spawns (LocalAvatarTransform), so they can't just inherit its
        // scale the way a remote player's bubble inherits WorldPresence.SpawnRemotePlayer's scale --
        // this has to actively track the avatar's measured size every frame instead.
        void UpdateBubbleHeights()
        {
            var avatarTransform = Presence.LocalAvatarTransform;
            if (avatarTransform == null) return;

            if (avatarTransform != _measuredAvatarTransform)
            {
                _measuredAvatarTransform = avatarTransform;
                _avatarRenderer = avatarTransform.GetComponent<SpriteRenderer>();
            }
            if (_avatarRenderer == null || _avatarRenderer.sprite == null) return;

            var offset = (_avatarRenderer.bounds.max.y - transform.position.y) + bubbleHeightPadding;
            localChatBubble?.SetHeightOffset(offset);
            localThoughtBubble?.SetHeightOffset(offset);
        }

        private void OnEnable()
        {
            Presence.OnChatMessageReceived += HandleChatMessageReceived;
        }

        private void OnDisable()
        {
            Presence.OnChatMessageReceived -= HandleChatMessageReceived;
        }

        private IEnumerator Start()
        {
            if (ConfigService.Instance != null)
            {
                yield return ConfigService.Instance.EnsureLoaded();
                Presence.charactersBaseUrl = ConfigService.Instance.Get("charactersBaseUrl");
            }

            // In the Editor there is no config.json, so charactersBaseUrl is empty and the fallback
            // avatar would be a white quad. Point at the live static sprite host so dummy renders
            // with real sprites. Editor-only + only when unset -- production always uses config.json.
            if (Application.isEditor && string.IsNullOrEmpty(Presence.charactersBaseUrl))
                Presence.charactersBaseUrl = editorCharactersBaseUrl;

            // Choose the walk/idle animations before arming the avatar spawn below (WorldPresence reads
            // these when it loads the sprite sets). A calm lobby walks rather than runs.
            Presence.remoteAnimation = moveAnimation;
            Presence.idleAnimation = idleAnimation;

            // Now that the sprite host + animations are resolved, arm the fallback avatar. WorldPresence.Update()
            // spawns it immediately (dummy), then upgrades to the real character on the server roster.
            Presence.localFallbackCharacterName = fallbackCharacterName;

            if (auth == null) yield break;

            if (auth.IsResolved) ApplyIdentity(auth);
            else auth.OnResolved += ApplyIdentity;
        }

        private void Update()
        {
            Presence.UpdateLocalTransform(transform.position, transform.eulerAngles.y);
            UpdateFootstepAudio();
            UpdateBubbleHeights();
        }

        // Plays a footstep SFX at a fixed cadence while the local player is actually moving --
        // driven by this GameObject's own transform delta since there is no dedicated player
        // movement controller yet to hook into (this Update() already runs every frame for
        // position sync, so it is the only reliable "is the local player moving" signal today).
        private void UpdateFootstepAudio()
        {
            if (!_hasLastPosition)
            {
                _lastPosition = transform.position;
                _hasLastPosition = true;
                return;
            }

            var moved = Vector3.Distance(transform.position, _lastPosition) > footstepMoveThreshold;
            _lastPosition = transform.position;

            if (!moved)
            {
                _footstepTimer = 0f;
                return;
            }

            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer > 0f) return;

            AudioManager.Instance?.PlaySFX("FootstepMetal");
            _footstepTimer = footstepIntervalSeconds;
        }

        public void SendChat(string text) => Presence.SendChat(text);

        public void SendWhisper(string toUsername, string text) => Presence.SendWhisper(toUsername, text);

        public void SendEmote(string animationName)
        {
            Presence.SendEmote(animationName);
            OnLocalEmoteRequested?.Invoke(animationName);
        }

        private void ApplyIdentity(FactoryAuth resolvedAuth)
        {
            auth.OnResolved -= ApplyIdentity;
            Presence.LocalUsername = resolvedAuth.Username;
        }

        // sender == null is the local player's OWN echo (chat broadcasts to everyone incl. the
        // sender) -- show it on the local bubble, same pattern CoreSystems documents for SpeechChat.
        // Public (not private) so it can be exercised directly in tests without a live connection --
        // WorldPresence.OnChatMessageReceived can only be raised by WorldPresence itself.
        public void HandleChatMessageReceived(RemotePlayer sender, string text)
        {
            if (sender != null || localChatBubble == null) return;
            localChatBubble.Show(text, Presence.chatBubbleDurationSeconds);
        }
    }
}
