using System;
using UnityEngine;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;

namespace Game.UI
{
    // Root gameplay screen: hosts the mute button and any other HUD elements over the 3D lobby
    // world. Hidden on load (the NO BOOT-FRAME FLASH rule -- StartScreen is the intended
    // first-visible screen); shown when the player joins the lobby via StartScreen's existing
    // "Join Lobby" -> gameScreen.Show() wiring (StartScreen already holds a generic BaseScreen
    // reference for this -- see #12), so this ticket does not duplicate that transition. The
    // mockup shows only a bordered "PLAYFIELD" wireframe box (the actual 3D world rendered by
    // the scene camera, not a UI element) plus a small mute button in the top-right corner --
    // built entirely from UI Toolkit built-in elements/colors, no pixel artwork needed.
    [RequireComponent(typeof(UIDocument))]
    public class GameScreen : BaseScreen
    {
        public Button MuteButton { get; private set; }
        public bool IsMuted { get; private set; }

        public event Action<bool> OnMuteToggled;

        private void Start()
        {
            BuildUI();
            Hide();
        }

        private void BuildUI()
        {
            var root = Root;
            if (root == null) return;

            root.Clear();
            root.style.flexGrow = 1;

            // The gameplay world renders via the scene camera beneath this overlay -- the root
            // must not swallow pointer events meant for world/movement interaction. Only the
            // mute button (an explicit child, default PickingMode.Position) should be pickable.
            root.pickingMode = PickingMode.Ignore;

            MuteButton = new Button(ToggleMute) { text = "Mute" };
            MuteButton.style.position = Position.Absolute;
            MuteButton.style.top = 16f;
            MuteButton.style.right = 16f;
            MuteButton.style.width = 90f;
            MuteButton.style.height = 36f;
            MuteButton.style.fontSize = 14;
            MuteButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            MuteButton.style.color = new Color(0.15f, 0.3f, 0.15f, 1f);
            MuteButton.style.backgroundColor = new Color(0.78f, 0.89f, 0.79f, 1f);
            var buttonBorder = new Color(0.4f, 0.6f, 0.4f, 1f);
            MuteButton.style.borderTopColor = buttonBorder;
            MuteButton.style.borderBottomColor = buttonBorder;
            MuteButton.style.borderLeftColor = buttonBorder;
            MuteButton.style.borderRightColor = buttonBorder;
            MuteButton.style.borderTopWidth = 2f;
            MuteButton.style.borderBottomWidth = 2f;
            MuteButton.style.borderLeftWidth = 2f;
            MuteButton.style.borderRightWidth = 2f;
            root.Add(MuteButton);
        }

        // Invoked by the mute button; also callable directly (tests) since UI Toolkit's
        // Button.clicked event cannot be externally invoked.
        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            AudioListener.volume = IsMuted ? 0f : 1f;
            if (MuteButton != null) MuteButton.text = IsMuted ? "Unmute" : "Mute";
            OnMuteToggled?.Invoke(IsMuted);
        }
    }
}
