using UnityEngine;
using OneTimeGames.CoreSystems;
using Game.World;

namespace Game.Chat
{
    // Wires CoreSystems' SpeechChatController (mic toggle -> local thought bubble -> submitted text)
    // to this game's WorldPresenceController, per the CoreSystems-documented pattern:
    //   speechChatController.OnMessageSubmitted += worldPresence.SendChat;
    // The other half of that pattern -- showing the local player's own talking bubble on submit via
    // the null-sender echo -- is already wired by WorldPresenceController.HandleChatMessageReceived
    // (see #6); this controller adds only the missing SpeechChat -> WorldPresence.SendChat leg.
    // Attach to the local player GameObject alongside WorldPresenceController. RequireComponent
    // auto-adds SpeechChatController, which itself auto-adds its own required UIDocument tap-catcher.
    [RequireComponent(typeof(SpeechChatController))]
    public class SpeechChatIntegrationController : MonoBehaviour
    {
        [Tooltip("Resolved automatically from the scene if left unassigned.")]
        [SerializeField] private WorldPresenceController presence;

        public SpeechChatController SpeechChat { get; private set; }

        public WorldPresenceController Presence
        {
            get => presence;
            set => presence = value;
        }

        private void Awake()
        {
            SpeechChat = GetComponent<SpeechChatController>();
            if (presence == null) presence = FindFirstObjectByType<WorldPresenceController>();
        }

        private void OnEnable()
        {
            SpeechChat.OnMessageSubmitted += HandleMessageSubmitted;
        }

        private void OnDisable()
        {
            SpeechChat.OnMessageSubmitted -= HandleMessageSubmitted;
        }

        // Public (not private) so it can be exercised directly in tests without driving SpeechChatController's
        // full mic/transcript flow -- mirrors WorldPresenceController.HandleChatMessageReceived.
        public void HandleMessageSubmitted(string text)
        {
            if (presence == null) return;
            presence.SendChat(text);
        }
    }
}
