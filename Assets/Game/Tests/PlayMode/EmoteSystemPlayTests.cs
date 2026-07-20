using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Game.Emotes;
using Game.World;
using OneTimeGames.CoreSystems;

namespace Game.Tests.PlayMode
{
    public class EmoteSystemPlayTests
    {
        private GameObject _go;
        private EmoteSystem _emoteSystem;
        private PanelSettings _panelSettings;

        [SetUp]
        public void SetUp()
        {
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _go = new GameObject("emoteSystem");
            _emoteSystem = _go.AddComponent<EmoteSystem>();
            _emoteSystem.GetComponent<UIDocument>().panelSettings = _panelSettings;
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.Destroy(_go);
            if (_panelSettings != null) Object.Destroy(_panelSettings);
        }

        [UnityTest]
        public IEnumerator RequireComponent_AddsUIDocumentAutomatically()
        {
            yield return null;

            Assert.IsNotNull(_go.GetComponent<UIDocument>());
        }

        [UnityTest]
        public IEnumerator EmoteButton_IsBuilt_AndHiddenByDefault()
        {
            yield return null;

            Assert.IsNotNull(_emoteSystem.EmoteButton, "Emote button should be built on Start().");
            Assert.AreEqual(DisplayStyle.None, _emoteSystem.EmoteButton.style.display.value,
                "Emote button should be hidden until a recorded emote is confirmed available.");
            Assert.IsFalse(_emoteSystem.HasRecordedEmotes);
        }

        [UnityTest]
        public IEnumerator ApplyEmoteListResponse_ShowsButton_WhenAReadyEmoteExists()
        {
            yield return null;

            _emoteSystem.ApplyEmoteListResponse("[{\"name\":\"wave\",\"ready\":true}]");

            Assert.IsTrue(_emoteSystem.HasRecordedEmotes);
            Assert.AreEqual("wave", _emoteSystem.SelectedEmoteName);
            Assert.AreEqual(DisplayStyle.Flex, _emoteSystem.EmoteButton.style.display.value);
        }

        [UnityTest]
        public IEnumerator ApplyEmoteListResponse_KeepsButtonHidden_WhenNoEmotesAreReady()
        {
            yield return null;

            _emoteSystem.ApplyEmoteListResponse("[{\"name\":\"wave\",\"ready\":false}]");

            Assert.IsFalse(_emoteSystem.HasRecordedEmotes);
            Assert.AreEqual(DisplayStyle.None, _emoteSystem.EmoteButton.style.display.value);
        }

        [UnityTest]
        public IEnumerator ApplyEmoteListResponse_CanHideButtonAgain_AfterPreviouslyShown()
        {
            yield return null;

            _emoteSystem.ApplyEmoteListResponse("[{\"name\":\"wave\",\"ready\":true}]");
            Assert.AreEqual(DisplayStyle.Flex, _emoteSystem.EmoteButton.style.display.value);

            _emoteSystem.ApplyEmoteListResponse("[]");

            Assert.IsFalse(_emoteSystem.HasRecordedEmotes);
            Assert.AreEqual(DisplayStyle.None, _emoteSystem.EmoteButton.style.display.value);
        }

        [UnityTest]
        public IEnumerator PlayEmote_DoesNotThrow_WhenNoRecordedEmotes()
        {
            yield return null;

            Assert.DoesNotThrow(() => _emoteSystem.PlayEmote());
        }

        [UnityTest]
        public IEnumerator PlayEmote_SendsSelectedEmote_ViaWorldPresenceController()
        {
            var presenceGo = new GameObject("WorldPresenceController");
            var presenceController = presenceGo.AddComponent<WorldPresenceController>();
            _emoteSystem.PresenceController = presenceController;
            yield return null;

            _emoteSystem.ApplyEmoteListResponse("[{\"name\":\"wave\",\"ready\":true}]");

            string sentEmote = null;
            presenceController.OnLocalEmoteRequested += name => sentEmote = name;

            _emoteSystem.PlayEmote();

            Assert.AreEqual("wave", sentEmote);

            Object.Destroy(presenceGo);
        }

        [UnityTest]
        public IEnumerator PlayEmote_DoesNothing_WhenNoPresenceControllerAssigned()
        {
            yield return null;

            _emoteSystem.ApplyEmoteListResponse("[{\"name\":\"wave\",\"ready\":true}]");

            Assert.DoesNotThrow(() => _emoteSystem.PlayEmote());
        }

        [UnityTest]
        public IEnumerator Awake_FindsFactoryAuthInScene_WhenNotAssigned()
        {
            var authGo = new GameObject("FactoryAuth");
            var auth = authGo.AddComponent<FactoryAuth>();

            var go = new GameObject("emoteSystem2");
            var emoteSystem = go.AddComponent<EmoteSystem>();
            emoteSystem.GetComponent<UIDocument>().panelSettings = _panelSettings;
            yield return null;

            Assert.AreSame(auth, emoteSystem.Auth);

            Object.Destroy(go);
            Object.Destroy(authGo);
        }

        [UnityTest]
        public IEnumerator Awake_FindsWorldPresenceControllerInScene_WhenNotAssigned()
        {
            var presenceGo = new GameObject("WorldPresenceController");
            var presenceController = presenceGo.AddComponent<WorldPresenceController>();

            var go = new GameObject("emoteSystem3");
            var emoteSystem = go.AddComponent<EmoteSystem>();
            emoteSystem.GetComponent<UIDocument>().panelSettings = _panelSettings;
            yield return null;

            Assert.AreSame(presenceController, emoteSystem.PresenceController);

            Object.Destroy(go);
            Object.Destroy(presenceGo);
        }
    }
}
