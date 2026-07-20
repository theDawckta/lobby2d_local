using System;
using UnityEngine;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;

namespace Game.UI
{
    // Entry screen: a welcome message, a help panel with instructions, and a Join Lobby button
    // that transitions to the game screen. Built entirely from UI Toolkit built-in
    // elements/colors -- the mockup is a plain bordered-box wireframe with no pixel artwork, so
    // no design asset is needed. gameScreen is a generic BaseScreen reference (assigned by the
    // Main Scene assembly ticket) rather than a hardcoded concrete type, since this ticket does
    // not own the game screen's implementation.
    [RequireComponent(typeof(UIDocument))]
    public class StartScreen : BaseScreen
    {
        [SerializeField] private BaseScreen gameScreen;
        [SerializeField] private string welcomeMessage = "Welcome to the Lobby!";

        [TextArea]
        [SerializeField]
        private string helpInstructions =
            "Move with WASD or the on-screen stick.\n" +
            "Talk to other players using the mic button.\n" +
            "Explore the facility and interact with objects you find.";

        public Button JoinLobbyButton { get; private set; }
        public VisualElement HelpPanel { get; private set; }
        public Label WelcomeLabel { get; private set; }
        public Label HelpLabel { get; private set; }

        public BaseScreen GameScreen
        {
            get => gameScreen;
            set => gameScreen = value;
        }

        public event Action OnJoinLobbyRequested;

        private void Start()
        {
            BuildUI();
            Show();
        }

        private void BuildUI()
        {
            var root = Root;
            if (root == null) return;

            root.Clear();
            root.style.flexGrow = 1;
            root.style.flexDirection = FlexDirection.Column;
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            root.style.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 1f);

            var welcomeBox = CreateBox(420f, 70f);
            WelcomeLabel = CreateLabel(welcomeMessage, 24, FontStyle.Bold);
            welcomeBox.Add(WelcomeLabel);
            root.Add(welcomeBox);

            HelpPanel = CreateBox(500f, 220f);
            HelpPanel.style.marginTop = 24f;
            HelpLabel = CreateLabel(helpInstructions, 16, FontStyle.Normal);
            HelpLabel.style.whiteSpace = WhiteSpace.Normal;
            HelpLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            HelpPanel.Add(HelpLabel);
            root.Add(HelpPanel);

            JoinLobbyButton = new Button(RequestJoinLobby) { text = "Join Lobby" };
            JoinLobbyButton.style.marginTop = 24f;
            JoinLobbyButton.style.width = 220f;
            JoinLobbyButton.style.height = 48f;
            JoinLobbyButton.style.fontSize = 18;
            JoinLobbyButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            JoinLobbyButton.style.color = new Color(0.15f, 0.3f, 0.15f, 1f);
            JoinLobbyButton.style.backgroundColor = new Color(0.78f, 0.89f, 0.79f, 1f);
            var buttonBorder = new Color(0.4f, 0.6f, 0.4f, 1f);
            JoinLobbyButton.style.borderTopColor = buttonBorder;
            JoinLobbyButton.style.borderBottomColor = buttonBorder;
            JoinLobbyButton.style.borderLeftColor = buttonBorder;
            JoinLobbyButton.style.borderRightColor = buttonBorder;
            JoinLobbyButton.style.borderTopWidth = 2f;
            JoinLobbyButton.style.borderBottomWidth = 2f;
            JoinLobbyButton.style.borderLeftWidth = 2f;
            JoinLobbyButton.style.borderRightWidth = 2f;
            root.Add(JoinLobbyButton);
        }

        // Invoked by the Join Lobby button; also callable directly (tests, or a future scene
        // controller) since UI Toolkit's Button.clicked event cannot be externally invoked.
        public void RequestJoinLobby()
        {
            Hide();
            if (gameScreen != null)
            {
                gameScreen.Show();
            }
            OnJoinLobbyRequested?.Invoke();
        }

        private static VisualElement CreateBox(float width, float minHeight)
        {
            var box = new VisualElement();
            box.style.width = width;
            box.style.minHeight = minHeight;
            box.style.backgroundColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            var borderColor = new Color(0.45f, 0.45f, 0.45f, 1f);
            box.style.borderTopColor = borderColor;
            box.style.borderBottomColor = borderColor;
            box.style.borderLeftColor = borderColor;
            box.style.borderRightColor = borderColor;
            box.style.borderTopWidth = 2f;
            box.style.borderBottomWidth = 2f;
            box.style.borderLeftWidth = 2f;
            box.style.borderRightWidth = 2f;
            box.style.alignItems = Align.Center;
            box.style.justifyContent = Justify.Center;
            box.style.paddingLeft = 16f;
            box.style.paddingRight = 16f;
            box.style.paddingTop = 12f;
            box.style.paddingBottom = 12f;
            return box;
        }

        private static Label CreateLabel(string text, int fontSize, FontStyle fontStyle)
        {
            var label = new Label(text);
            label.style.fontSize = fontSize;
            label.style.unityFontStyleAndWeight = fontStyle;
            label.style.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            return label;
        }
    }
}
