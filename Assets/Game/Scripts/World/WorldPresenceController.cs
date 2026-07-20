using System;
using System.Collections;
using UnityEngine;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.Presence;

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

        // Fired when this game object sends an emote -- the server broadcasts emotes to everyone
        // EXCEPT the sender, so the local player must play their own emote locally on this hook.
        public event Action<string> OnLocalEmoteRequested;

        private void Awake()
        {
            Presence = GetComponent<WorldPresence>();
            if (auth == null) auth = FindFirstObjectByType<FactoryAuth>();
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

            if (auth == null) yield break;

            if (auth.IsResolved) ApplyIdentity(auth);
            else auth.OnResolved += ApplyIdentity;
        }

        private void Update()
        {
            Presence.UpdateLocalTransform(transform.position, transform.eulerAngles.y);
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
