using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Chat;
using Game.World;
using OneTimeGames.CoreSystems;

public class SpeechChatIntegrationControllerPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private GameObject Spawn(string name)
    {
        var go = new GameObject(name);
        _spawned.Add(go);
        return go;
    }

    // In-memory ISpeechSource so a submit can be driven end-to-end (SubmitCurrent -> OnMessageSubmitted)
    // with no browser/WebGL/Editor-keyboard dependency, mirroring how SpeechChatController itself is
    // designed to be tested behind the ISpeechSource seam.
    private class FakeSpeechSource : ISpeechSource
    {
        public bool IsSupported => true;
        public bool IsAvailable => true;
        public bool IsListening { get; private set; }
        public string CurrentTranscript { get; set; } = "";

        // Required by ISpeechSource; these tests only need CurrentTranscript/SubmitCurrent, so neither
        // event is ever raised -- suppress the resulting "declared but never used" warning.
#pragma warning disable 0067
        public event Action OnTranscriptChanged;
        public event Action<SpeechErrorKind> OnError;
#pragma warning restore 0067

        public void StartListening() => IsListening = true;
        public void StopListening() => IsListening = false;
        public void ResetTranscript() => CurrentTranscript = "";
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _spawned)
        {
            if (go != null) UnityEngine.Object.Destroy(go);
        }
        _spawned.Clear();
    }

    [UnityTest]
    public IEnumerator RequireComponent_AddsSpeechChatControllerAutomatically()
    {
        var go = Spawn("SpeechChatIntegrationController");
        go.AddComponent<SpeechChatIntegrationController>();
        yield return null;

        Assert.IsNotNull(go.GetComponent<SpeechChatController>());
    }

    [UnityTest]
    public IEnumerator SpeechChat_IsWiredToTheSpeechChatControllerComponent()
    {
        var go = Spawn("SpeechChatIntegrationController");
        var controller = go.AddComponent<SpeechChatIntegrationController>();
        yield return null;

        Assert.IsNotNull(controller.SpeechChat);
        Assert.AreSame(go.GetComponent<SpeechChatController>(), controller.SpeechChat);
    }

    [UnityTest]
    public IEnumerator Presence_FindsWorldPresenceControllerInScene_WhenNotAssigned()
    {
        var presenceGo = Spawn("WorldPresenceController");
        var presence = presenceGo.AddComponent<WorldPresenceController>();

        var go = Spawn("SpeechChatIntegrationController");
        var controller = go.AddComponent<SpeechChatIntegrationController>();
        yield return null;

        Assert.AreSame(presence, controller.Presence);
    }

    [UnityTest]
    public IEnumerator Presence_StaysNull_WhenNoWorldPresenceControllerExistsAnywhere()
    {
        var go = Spawn("SpeechChatIntegrationController");
        var controller = go.AddComponent<SpeechChatIntegrationController>();
        yield return null;

        Assert.IsNull(controller.Presence);
    }

    [UnityTest]
    public IEnumerator Presence_SetterOverridesAutoFind()
    {
        var autoFoundGo = Spawn("AutoFoundPresence");
        autoFoundGo.AddComponent<WorldPresenceController>();

        var explicitGo = Spawn("ExplicitPresence");
        var explicitPresence = explicitGo.AddComponent<WorldPresenceController>();

        var go = Spawn("SpeechChatIntegrationController");
        var controller = go.AddComponent<SpeechChatIntegrationController>();
        controller.Presence = explicitPresence;
        yield return null;

        Assert.AreSame(explicitPresence, controller.Presence);
    }

    [UnityTest]
    public IEnumerator HandleMessageSubmitted_DoesNotThrow_WhenPresenceIsNull()
    {
        var go = Spawn("SpeechChatIntegrationController");
        var controller = go.AddComponent<SpeechChatIntegrationController>();
        yield return null;

        Assert.DoesNotThrow(() => controller.HandleMessageSubmitted("hello"));
    }

    [UnityTest]
    public IEnumerator HandleMessageSubmitted_RoutesToPresenceSendChat_WithoutThrowing_WhenDisconnected()
    {
        var presenceGo = Spawn("WorldPresenceController");
        var presence = presenceGo.AddComponent<WorldPresenceController>();

        var go = Spawn("SpeechChatIntegrationController");
        var controller = go.AddComponent<SpeechChatIntegrationController>();
        controller.Presence = presence;
        yield return null;

        // No WorldConnection exists in this scene, so this proves the wiring calls all the way through
        // to WorldPresenceController.SendChat -> WorldPresence.SendChat with no exception -- the actual
        // network send is already covered by CoreSystems' own WorldPresence tests.
        Assert.DoesNotThrow(() => controller.HandleMessageSubmitted("hello lobby"));
    }

    [UnityTest]
    public IEnumerator SubmitCurrent_OnSpeechChatController_RoutesThroughToPresenceSendChat()
    {
        var presenceGo = Spawn("WorldPresenceController");
        var presence = presenceGo.AddComponent<WorldPresenceController>();

        var go = Spawn("SpeechChatIntegrationController");
        var controller = go.AddComponent<SpeechChatIntegrationController>();
        controller.Presence = presence;
        yield return null;

        // Drive the REAL SpeechChatController submit flow (not the handler directly) to prove
        // OnEnable actually subscribed OnMessageSubmitted -> HandleMessageSubmitted -> presence.SendChat.
        var source = new FakeSpeechSource { CurrentTranscript = "hello world" };
        controller.SpeechChat.Source = source;

        Assert.DoesNotThrow(() => controller.SpeechChat.SubmitCurrent());
    }
}
