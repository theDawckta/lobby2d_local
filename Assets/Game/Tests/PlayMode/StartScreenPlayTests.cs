using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Game.UI;

namespace Game.Tests.PlayMode
{
    public class StartScreenPlayTests
    {
        // BaseScreen has no abstract members -- a bare subclass is enough to stand in for the
        // (not-yet-built) game screen this ticket transitions to.
        private class StubScreen : OneTimeGames.CoreSystems.BaseScreen
        {
        }

        private GameObject _go;
        private StartScreen _screen;
        private PanelSettings _panelSettings;

        [SetUp]
        public void SetUp()
        {
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();

            _go = new GameObject("startScreen");
            _screen = _go.AddComponent<StartScreen>();
            _screen.GetComponent<UIDocument>().panelSettings = _panelSettings;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.Destroy(_go);
            if (_panelSettings != null) Object.Destroy(_panelSettings);
        }

        [UnityTest]
        public IEnumerator StartScreen_IsVisibleOnLoad()
        {
            yield return null;

            Assert.IsTrue(_screen.IsVisible, "Start screen should be visible once Start() has run.");
        }

        [UnityTest]
        public IEnumerator HelpPanel_IsVisible_AndContainsInstructions()
        {
            yield return null;

            Assert.IsNotNull(_screen.HelpPanel, "Help panel should be built.");
            Assert.AreNotEqual(DisplayStyle.None, _screen.HelpPanel.style.display.value,
                "Help panel should be visible.");
            Assert.IsNotNull(_screen.HelpLabel, "Help panel should contain an instructions label.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(_screen.HelpLabel.text),
                "Help panel instructions text should not be empty.");
        }

        [UnityTest]
        public IEnumerator WelcomeLabel_ShowsConfiguredWelcomeMessage()
        {
            yield return null;

            Assert.IsNotNull(_screen.WelcomeLabel);
            Assert.AreEqual("Welcome to the Lobby!", _screen.WelcomeLabel.text);
        }

        [UnityTest]
        public IEnumerator JoinLobbyButton_IsBuilt_WithExpectedLabel()
        {
            yield return null;

            Assert.IsNotNull(_screen.JoinLobbyButton, "Join Lobby button should be built.");
            Assert.AreEqual("Join Lobby", _screen.JoinLobbyButton.text);
        }

        [UnityTest]
        public IEnumerator RequestJoinLobby_HidesStartScreen_AndShowsGameScreen_AndFiresEvent()
        {
            yield return null;

            var gameScreenGo = new GameObject("gameScreen");
            var gameScreen = gameScreenGo.AddComponent<StubScreen>();
            gameScreen.GetComponent<UIDocument>().panelSettings = _panelSettings;
            gameScreen.Hide();
            _screen.GameScreen = gameScreen;

            bool eventFired = false;
            _screen.OnJoinLobbyRequested += () => eventFired = true;

            _screen.RequestJoinLobby();

            Assert.IsFalse(_screen.IsVisible, "Start screen should hide after Join Lobby.");
            Assert.IsTrue(gameScreen.IsVisible, "Game screen should be shown after Join Lobby.");
            Assert.IsTrue(eventFired, "OnJoinLobbyRequested should fire.");

            Object.Destroy(gameScreenGo);
        }

        [UnityTest]
        public IEnumerator RequestJoinLobby_DoesNotThrow_WhenGameScreenNotAssigned()
        {
            yield return null;

            Assert.IsNull(_screen.GameScreen);
            Assert.DoesNotThrow(() => _screen.RequestJoinLobby());
            Assert.IsFalse(_screen.IsVisible);
        }
    }
}
