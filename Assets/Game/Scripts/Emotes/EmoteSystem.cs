using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;
using Game.World;
using Game.Audio;

namespace Game.Emotes
{
    // Checks whether the local player's character has any recorded custom emotes (GET
    // {charactersBaseUrl}/api/characters/{characterName}/emotes -- the same public listing
    // endpoint the profile page uses) and, if so, shows a HUD button that sends one via
    // WorldPresenceController.SendEmote. Actually PLAYING the emote animation is already fully
    // wired downstream: WorldPresence.HandleEmote calls RemotePlayer.PlayEmote for everyone else in
    // the lobby, and WorldPresenceController.OnLocalEmoteRequested exists precisely for the
    // sender's own local echo (the server excludes the sender from its broadcast) -- this
    // component only decides emote AVAILABILITY and triggers the send.
    // No mockup shows an emote control (GameScreen.png has only the mute button), so this is a
    // standalone, added-only component (its own UIDocument) rather than an edit to GameScreen --
    // per the prefab-split convention, a future Main Scene ticket positions/wires it into the scene.
    [RequireComponent(typeof(UIDocument))]
    public class EmoteSystem : MonoBehaviour
    {
        [Tooltip("Resolved automatically from the scene if left unassigned.")]
        [SerializeField] private FactoryAuth auth;

        [Tooltip("Resolved automatically from the scene if left unassigned.")]
        [SerializeField] private WorldPresenceController presenceController;

        public Button EmoteButton { get; private set; }
        public string SelectedEmoteName { get; private set; }
        public bool HasRecordedEmotes => !string.IsNullOrEmpty(SelectedEmoteName);

        public FactoryAuth Auth
        {
            get => auth;
            set => auth = value;
        }

        public WorldPresenceController PresenceController
        {
            get => presenceController;
            set => presenceController = value;
        }

        private string _charactersBaseUrl = "";

        private void Awake()
        {
            if (auth == null) auth = FindFirstObjectByType<FactoryAuth>();
            if (presenceController == null) presenceController = FindFirstObjectByType<WorldPresenceController>();
        }

        private void Start()
        {
            BuildUI();
            StartCoroutine(InitializeRoutine());
        }

        private void BuildUI()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            if (root == null) return;

            root.Clear();
            root.style.flexGrow = 1;
            // Must not swallow pointer events meant for world/movement interaction -- only the
            // button itself (default PickingMode.Position) should be pickable.
            root.pickingMode = PickingMode.Ignore;

            EmoteButton = new Button(PlayEmote) { text = "Emote" };
            EmoteButton.style.position = Position.Absolute;
            EmoteButton.style.bottom = 16f;
            EmoteButton.style.right = 16f;
            EmoteButton.style.width = 90f;
            EmoteButton.style.height = 36f;
            EmoteButton.style.fontSize = 14;
            EmoteButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            EmoteButton.style.color = new Color(0.15f, 0.3f, 0.15f, 1f);
            EmoteButton.style.backgroundColor = new Color(0.78f, 0.89f, 0.79f, 1f);
            var buttonBorder = new Color(0.4f, 0.6f, 0.4f, 1f);
            EmoteButton.style.borderTopColor = buttonBorder;
            EmoteButton.style.borderBottomColor = buttonBorder;
            EmoteButton.style.borderLeftColor = buttonBorder;
            EmoteButton.style.borderRightColor = buttonBorder;
            EmoteButton.style.borderTopWidth = 2f;
            EmoteButton.style.borderBottomWidth = 2f;
            EmoteButton.style.borderLeftWidth = 2f;
            EmoteButton.style.borderRightWidth = 2f;
            // Hidden until a recorded, ready-to-play emote is confirmed for this character
            // (acceptance: the button is visible only if the player has recorded emotes).
            EmoteButton.style.display = DisplayStyle.None;
            root.Add(EmoteButton);
        }

        private IEnumerator InitializeRoutine()
        {
            if (ConfigService.Instance != null)
            {
                yield return ConfigService.Instance.EnsureLoaded();
                _charactersBaseUrl = ConfigService.Instance.Get("charactersBaseUrl");
            }

            if (auth == null) yield break;

            if (auth.IsResolved) yield return FetchAvailableEmotes(auth.CharacterName);
            else auth.OnResolved += HandleAuthResolved;
        }

        private void HandleAuthResolved(FactoryAuth resolvedAuth)
        {
            auth.OnResolved -= HandleAuthResolved;
            StartCoroutine(FetchAvailableEmotes(resolvedAuth.CharacterName));
        }

        // Rule C / "game must work without a backend": if charactersBaseUrl never loaded (e.g.
        // no config.json served, or the account has no character), silently stay hidden rather
        // than requesting a blank/invalid URL.
        private IEnumerator FetchAvailableEmotes(string characterName)
        {
            if (string.IsNullOrEmpty(_charactersBaseUrl) || string.IsNullOrEmpty(characterName)) yield break;

            var url = _charactersBaseUrl.TrimEnd('/') + "/api/characters/" +
                      UnityWebRequest.EscapeURL(characterName) + "/emotes";
            using var req = UnityWebRequest.Get(url);
            req.timeout = 5;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;

            ApplyEmoteListResponse(req.downloadHandler.text);
        }

        // Parses the /emotes endpoint's raw JSON array response and updates the selected emote +
        // button visibility. Public (not private) so it can be exercised directly in tests without
        // a live network call -- mirrors FactoryAuth.ApplyAuthResponse's testing seam.
        public void ApplyEmoteListResponse(string json)
        {
            SelectedEmoteName = SelectFirstReadyEmote(json);
            if (EmoteButton != null)
                EmoteButton.style.display = HasRecordedEmotes ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Pure parsing logic: returns the first ready-to-play recorded emote's name from the
        // endpoint's raw JSON array response, or null if there are none / the JSON is malformed.
        // Static so it is unit-testable in EditMode with no Unity lifecycle involved.
        public static string SelectFirstReadyEmote(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            EmoteListResponse response;
            try
            {
                // JsonUtility can't parse a top-level JSON array directly -- wrap it in an object.
                response = JsonUtility.FromJson<EmoteListResponse>("{\"items\":" + json + "}");
            }
            catch (Exception)
            {
                return null;
            }

            if (response?.items == null) return null;

            foreach (var entry in response.items)
            {
                if (entry != null && entry.ready && !string.IsNullOrEmpty(entry.name)) return entry.name;
            }

            return null;
        }

        // Invoked by the emote button; also callable directly (tests) since UI Toolkit's
        // Button.clicked event cannot be externally invoked. Sending is delegated to
        // WorldPresenceController, which broadcasts to every other player in the lobby AND raises
        // OnLocalEmoteRequested for the sender's own local echo (see its own notes).
        public void PlayEmote()
        {
            if (!HasRecordedEmotes || presenceController == null) return;
            presenceController.SendEmote(SelectedEmoteName);
            AudioManager.Instance?.PlaySFX("EmotePlay");
        }

        [Serializable]
        private class EmoteEntry
        {
            public string name;
            public bool ready;
        }

        [Serializable]
        private class EmoteListResponse
        {
            public EmoteEntry[] items;
        }
    }
}
