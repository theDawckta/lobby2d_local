using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Game.UI;
using Game.Audio;

namespace Game.Tests.PlayMode
{
    public class GameScreenPlayTests
    {
        private GameObject _go;
        private GameScreen _screen;
        private PanelSettings _panelSettings;
        private float _originalVolume;

        [SetUp]
        public void SetUp()
        {
            _originalVolume = AudioListener.volume;
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();

            _go = new GameObject("gameScreen");
            _screen = _go.AddComponent<GameScreen>();
            _screen.GetComponent<UIDocument>().panelSettings = _panelSettings;
        }

        [TearDown]
        public void TearDown()
        {
            AudioListener.volume = _originalVolume;
            if (_go != null) Object.Destroy(_go);
            if (_panelSettings != null) Object.Destroy(_panelSettings);
        }

        [UnityTest]
        public IEnumerator GameScreen_IsHidden_OnLoad()
        {
            yield return null;

            Assert.IsFalse(_screen.IsVisible, "Game screen should be hidden until the player joins the lobby.");
        }

        [UnityTest]
        public IEnumerator GameScreen_Show_MakesItVisible_AndServesAsHudRoot()
        {
            yield return null;

            _screen.Show();

            Assert.IsTrue(_screen.IsVisible, "Game screen should become visible when shown (player joins lobby).");
            Assert.IsNotNull(_screen.MuteButton, "Mute button should be built as a child of the game screen root.");
        }

        [UnityTest]
        public IEnumerator MuteButton_IsBuilt_WithExpectedLabel()
        {
            yield return null;

            Assert.IsNotNull(_screen.MuteButton, "Mute button should be built.");
            Assert.AreEqual("Mute", _screen.MuteButton.text);
            Assert.IsFalse(_screen.IsMuted, "Game should start unmuted.");
        }

        [UnityTest]
        public IEnumerator ToggleMute_MutesAudio_UpdatesLabel_AndFiresEvent()
        {
            yield return null;

            bool? eventValue = null;
            _screen.OnMuteToggled += muted => eventValue = muted;

            _screen.ToggleMute();

            Assert.IsTrue(_screen.IsMuted);
            Assert.AreEqual(0f, AudioListener.volume);
            Assert.AreEqual("Unmute", _screen.MuteButton.text);
            Assert.IsTrue(eventValue.HasValue && eventValue.Value);
        }

        [UnityTest]
        public IEnumerator ToggleMute_Twice_RestoresAudio_AndLabel()
        {
            yield return null;

            _screen.ToggleMute();
            _screen.ToggleMute();

            Assert.IsFalse(_screen.IsMuted);
            Assert.AreEqual(1f, AudioListener.volume);
            Assert.AreEqual("Mute", _screen.MuteButton.text);
        }

        [UnityTest]
        public IEnumerator ToggleMute_MutesMusic_ViaAudioManager()
        {
            yield return null;
            AudioManager.Instance.SetMusicMuted(false);

            _screen.ToggleMute();

            Assert.IsTrue(AudioManager.Instance.IsMusicMuted);
        }

        [UnityTest]
        public IEnumerator ToggleMute_Twice_UnmutesMusic_ViaAudioManager()
        {
            yield return null;
            AudioManager.Instance.SetMusicMuted(false);

            _screen.ToggleMute();
            _screen.ToggleMute();

            Assert.IsFalse(AudioManager.Instance.IsMusicMuted);
        }

        [UnityTest]
        public IEnumerator ToggleMute_PlaysUiSfx()
        {
            yield return null;

            string played = null;
            void Handler(string name) => played = name;
            AudioManager.Instance.OnSfxPlayed += Handler;

            _screen.ToggleMute();

            AudioManager.Instance.OnSfxPlayed -= Handler;
            Assert.AreEqual("MuteToggleOn", played);
        }
    }
}
