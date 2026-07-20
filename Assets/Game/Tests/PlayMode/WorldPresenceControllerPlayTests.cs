using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Game.World;
using Game.Audio;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.Presence;

public class WorldPresenceControllerPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private GameObject Spawn(string name)
    {
        var go = new GameObject(name);
        _spawned.Add(go);
        return go;
    }

    // ConfigService.EnsureLoaded() inside Start() does a real UnityWebRequest round trip, which
    // takes more than one frame to complete even when it fails fast (no config.json served in this
    // test environment) -- poll with a timeout instead of assuming a fixed number of frames, same
    // pattern as AuthConnectControllerPlayTests / WorldConnectControllerPlayTests.
    private static IEnumerator WaitUntilOrTimeout(System.Func<bool> condition, float timeoutSeconds = 10f)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeoutSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _spawned)
        {
            if (go != null) Object.Destroy(go);
        }
        _spawned.Clear();
    }

    [UnityTest]
    public IEnumerator RequireComponent_AddsWorldPresenceAutomatically()
    {
        var go = Spawn("WorldPresenceController");
        go.AddComponent<WorldPresenceController>();
        yield return null;

        Assert.IsNotNull(go.GetComponent<WorldPresence>());
    }

    [UnityTest]
    public IEnumerator Presence_IsWiredToTheWorldPresenceComponent()
    {
        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return null;

        Assert.IsNotNull(controller.Presence);
        Assert.AreSame(go.GetComponent<WorldPresence>(), controller.Presence);
    }

    [UnityTest]
    public IEnumerator Start_FindsFactoryAuthInScene_WhenNotAssigned()
    {
        var authGo = Spawn("FactoryAuth");
        var auth = authGo.AddComponent<FactoryAuth>();

        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return null;

        Assert.AreSame(auth, controller.Auth);
    }

    [UnityTest]
    public IEnumerator Start_DoesNotThrow_WhenNoFactoryAuthExistsAnywhere()
    {
        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return WaitUntilOrTimeout(() => false, 1f); // let Start() run a few frames

        Assert.IsNull(controller.Auth);
    }

    [UnityTest]
    public IEnumerator LocalUsername_IsSet_OnceFactoryAuthResolves()
    {
        var authGo = Spawn("FactoryAuth");
        var auth = authGo.AddComponent<FactoryAuth>();

        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return null;

        auth.ApplyAuthResponse("{\"token\":\"tok-123\",\"username\":\"alice\",\"characterName\":\"Hero\",\"isGuest\":false}");
        yield return WaitUntilOrTimeout(() => controller.Presence.LocalUsername != null);

        Assert.AreEqual("alice", controller.Presence.LocalUsername);
    }

    [UnityTest]
    public IEnumerator LocalUsername_IsSetImmediately_WhenFactoryAuthAlreadyResolvedBeforeStart()
    {
        var authGo = Spawn("FactoryAuth");
        var auth = authGo.AddComponent<FactoryAuth>();
        auth.ApplyAuthResponse("{\"token\":\"tok-789\",\"username\":\"carol\",\"characterName\":\"Scout\",\"isGuest\":false}");

        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return WaitUntilOrTimeout(() => controller.Presence.LocalUsername != null);

        Assert.AreEqual("carol", controller.Presence.LocalUsername);
    }

    [UnityTest]
    public IEnumerator Update_PushesLocalTransform_WithoutThrowing_WhilePlayerMoves()
    {
        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return null;

        go.transform.position = new Vector3(1f, 0f, 2f);
        yield return null;
        go.transform.position = new Vector3(3f, 0f, 5f);
        yield return null;

        // No WorldConnection exists in this scene, so Presence.Update() sees IsConnected == false and
        // no network send happens -- this proves the wiring runs every frame with no exception, which
        // is all that's verifiable without a live server (the send-throttling/network behavior itself
        // is already covered by CoreSystems' own WorldPresence tests).
        Assert.IsNotNull(controller.Presence);
    }

    [UnityTest]
    public IEnumerator Update_PlaysFootstepSfx_WhilePlayerIsMoving()
    {
        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return null; // first frame only establishes the last-known position

        string played = null;
        void Handler(string name) => played = name;
        AudioManager.Instance.OnSfxPlayed += Handler;

        for (var i = 0; i < 5; i++)
        {
            go.transform.position += new Vector3(0.1f, 0f, 0f);
            yield return null;
        }

        AudioManager.Instance.OnSfxPlayed -= Handler;
        Assert.AreEqual("FootstepMetal", played);
    }

    [UnityTest]
    public IEnumerator Update_DoesNotPlayFootstepSfx_WhilePlayerIsStationary()
    {
        var go = Spawn("WorldPresenceController");
        go.AddComponent<WorldPresenceController>();
        yield return null;

        var fired = false;
        void Handler(string name) => fired = true;
        AudioManager.Instance.OnSfxPlayed += Handler;

        for (var i = 0; i < 5; i++) yield return null;

        AudioManager.Instance.OnSfxPlayed -= Handler;
        Assert.IsFalse(fired);
    }

    [UnityTest]
    public IEnumerator SendChat_DoesNotThrow_WhenDisconnected()
    {
        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return null;

        Assert.DoesNotThrow(() => controller.SendChat("hello"));
    }

    [UnityTest]
    public IEnumerator SendWhisper_DoesNotThrow_WhenDisconnected()
    {
        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return null;

        Assert.DoesNotThrow(() => controller.SendWhisper("bob", "psst"));
    }

    [UnityTest]
    public IEnumerator SendEmote_FiresLocalEmoteEvent()
    {
        var go = Spawn("WorldPresenceController");
        var controller = go.AddComponent<WorldPresenceController>();
        yield return null;

        string played = null;
        controller.OnLocalEmoteRequested += name => played = name;
        controller.SendEmote("wave");

        Assert.AreEqual("wave", played);
    }

    [UnityTest]
    public IEnumerator HandleChatMessageReceived_ShowsLocalBubble_OnOwnEcho()
    {
        var go = Spawn("WorldPresenceController");
        var bubbleGo = Spawn("LocalChatBubble");
        var bubble = bubbleGo.AddComponent<ChatBubble>();

        var controller = go.AddComponent<WorldPresenceController>();
        controller.LocalChatBubble = bubble;
        yield return null;

        // WorldPresence.OnChatMessageReceived can only be raised by WorldPresence itself (requires a
        // live connection), so exercise the same handler a real own-echo would invoke directly --
        // sender == null is the local player's own message.
        controller.HandleChatMessageReceived(null, "hello world");
        yield return null;

        var label = bubble.GetComponentInChildren<Text>(true);
        Assert.IsNotNull(label);
        Assert.AreEqual("hello world", label.text);
        Assert.IsTrue(label.transform.parent.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator HandleChatMessageReceived_DoesNotShowLocalBubble_ForRemoteSender()
    {
        var go = Spawn("WorldPresenceController");
        var bubbleGo = Spawn("LocalChatBubble");
        var bubble = bubbleGo.AddComponent<ChatBubble>();
        var remoteGo = Spawn("RemotePlayer");
        var remote = remoteGo.AddComponent<RemotePlayer>();

        var controller = go.AddComponent<WorldPresenceController>();
        controller.LocalChatBubble = bubble;
        yield return null;

        controller.HandleChatMessageReceived(remote, "not mine");
        yield return null;

        var label = bubble.GetComponentInChildren<Text>(true);
        Assert.IsNotNull(label);
        Assert.IsFalse(label.transform.parent.gameObject.activeSelf);
    }
}
