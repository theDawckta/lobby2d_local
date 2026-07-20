using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.World;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.PersistentWorld;

public class WorldConnectControllerPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private GameObject Spawn(string name)
    {
        var go = new GameObject(name);
        _spawned.Add(go);
        return go;
    }

    // ConfigService.EnsureLoaded() inside WorldConnectController.Start() does a real
    // UnityWebRequest round trip, which takes more than one frame to complete even when it
    // fails fast (no config.json served in this test environment) -- poll with a timeout
    // instead of assuming a fixed number of frames, same pattern as AuthConnectControllerPlayTests.
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
    public IEnumerator RequireComponent_AddsWorldConnectionAutomatically()
    {
        var go = Spawn("WorldConnectController");
        go.AddComponent<WorldConnectController>();
        yield return null;

        Assert.IsNotNull(go.GetComponent<WorldConnection>());
    }

    [UnityTest]
    public IEnumerator Connection_IsWiredToTheWorldConnectionComponent()
    {
        var go = Spawn("WorldConnectController");
        var controller = go.AddComponent<WorldConnectController>();
        yield return null;

        Assert.IsNotNull(controller.Connection);
        Assert.AreSame(go.GetComponent<WorldConnection>(), controller.Connection);
    }

    [UnityTest]
    public IEnumerator Start_FindsFactoryAuthInScene_WhenNotAssigned()
    {
        var authGo = Spawn("FactoryAuth");
        var auth = authGo.AddComponent<FactoryAuth>();

        var go = Spawn("WorldConnectController");
        var controller = go.AddComponent<WorldConnectController>();
        yield return null;

        Assert.AreSame(auth, controller.Auth);
    }

    [UnityTest]
    public IEnumerator Start_DoesNotThrow_WhenNoFactoryAuthExistsAnywhere()
    {
        var go = Spawn("WorldConnectController");
        var controller = go.AddComponent<WorldConnectController>();
        yield return WaitUntilOrTimeout(() => false, 1f); // let Start() run a few frames

        Assert.IsNull(controller.Auth);
        Assert.IsFalse(controller.Connection.IsConnected);
    }

    [UnityTest]
    public IEnumerator ConnectToWorld_SetsCharacterName_OnceFactoryAuthResolves()
    {
        var authGo = Spawn("FactoryAuth");
        var auth = authGo.AddComponent<FactoryAuth>();

        var go = Spawn("WorldConnectController");
        var controller = go.AddComponent<WorldConnectController>();
        yield return null;

        auth.ApplyAuthResponse("{\"token\":\"tok-123\",\"username\":\"alice\",\"characterName\":\"Hero\",\"isGuest\":false}");
        yield return WaitUntilOrTimeout(() => controller.Connection.CharacterName != null);

        Assert.AreEqual("Hero", controller.Connection.CharacterName);
    }

    [UnityTest]
    public IEnumerator ConnectToWorld_DoesNotConnect_WhenWorldWsUrlNotConfigured()
    {
        // No config.json is served in this test environment, so ConfigService.Get("worldWsUrl")
        // falls back to "" -- the controller must skip connecting silently (no exception, no
        // attempted join with a blank URL) rather than crash, per the "game must work without a
        // backend" convention.
        var authGo = Spawn("FactoryAuth");
        var auth = authGo.AddComponent<FactoryAuth>();

        var go = Spawn("WorldConnectController");
        var controller = go.AddComponent<WorldConnectController>();
        yield return null;

        auth.ApplyAuthResponse("{\"token\":\"tok-456\",\"username\":\"bob\",\"characterName\":\"\",\"isGuest\":true}");
        // Wait for the wiring chain to actually run (proven by CharacterName getting set) before
        // asserting the negative -- otherwise a still-pending Start() coroutine would make this
        // assertion pass trivially without proving anything.
        yield return WaitUntilOrTimeout(() => controller.Connection.CharacterName != null);

        Assert.IsFalse(controller.Connection.IsConnected);
    }

    [UnityTest]
    public IEnumerator ConnectToWorld_RunsImmediately_WhenFactoryAuthAlreadyResolvedBeforeStart()
    {
        var authGo = Spawn("FactoryAuth");
        var auth = authGo.AddComponent<FactoryAuth>();
        auth.ApplyAuthResponse("{\"token\":\"tok-789\",\"username\":\"carol\",\"characterName\":\"Scout\",\"isGuest\":false}");

        var go = Spawn("WorldConnectController");
        var controller = go.AddComponent<WorldConnectController>();
        yield return WaitUntilOrTimeout(() => controller.Connection.CharacterName != null);

        Assert.AreEqual("Scout", controller.Connection.CharacterName);
    }

    [UnityTest]
    public IEnumerator WorldId_DefaultsToNonEmptyValue()
    {
        var go = Spawn("WorldConnectController");
        var controller = go.AddComponent<WorldConnectController>();
        yield return null;

        Assert.IsFalse(string.IsNullOrEmpty(controller.WorldId));
    }
}
