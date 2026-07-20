using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Auth;
using OneTimeGames.CoreSystems;

public class AuthConnectControllerPlayTests
{
    private GameObject _go;

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.Destroy(_go);
    }

    [UnityTest]
    public IEnumerator RequireComponent_AddsFactoryAuthAutomatically()
    {
        _go = new GameObject("AuthConnectController");
        _go.AddComponent<AuthConnectController>();
        yield return null;

        Assert.IsNotNull(_go.GetComponent<FactoryAuth>());
    }

    [UnityTest]
    public IEnumerator Auth_IsWiredToTheFactoryAuthComponent()
    {
        _go = new GameObject("AuthConnectController");
        var controller = _go.AddComponent<AuthConnectController>();
        yield return null;

        Assert.IsNotNull(controller.Auth);
        Assert.AreSame(_go.GetComponent<FactoryAuth>(), controller.Auth);
    }

    [UnityTest]
    public IEnumerator Start_ResolvesIdentity_FailingCleanly_WhenCharactersBaseUrlNotConfigured()
    {
        // No config.json exists in this test environment, so ConfigService.Get("charactersBaseUrl")
        // falls back to "" -- this proves the Start() -> ConfigService -> FactoryAuth.Resolve()
        // chain actually runs end to end, and that a missing backend config fails cleanly rather
        // than hanging or throwing.
        _go = new GameObject("AuthConnectController");
        var controller = _go.AddComponent<AuthConnectController>();
        yield return null;

        bool failed = false;
        string reason = null;
        controller.Auth.OnFailed += r => { failed = true; reason = r; };

        float elapsed = 0f;
        while (!failed && elapsed < 10f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(failed, "FactoryAuth.OnFailed was not fired within 10 seconds of Start()");
        Assert.AreEqual("charactersBaseUrl not set", reason);
    }

    [UnityTest]
    public IEnumerator FactoryAuth_UsernameAndToken_PopulatedAfterResolution()
    {
        _go = new GameObject("AuthConnectController");
        var controller = _go.AddComponent<AuthConnectController>();
        yield return null;

        var applied = controller.Auth.ApplyAuthResponse(
            "{\"token\":\"tok-123\",\"username\":\"alice\",\"characterName\":null,\"isGuest\":false}");

        Assert.IsTrue(applied);
        Assert.IsTrue(controller.Auth.IsResolved);
        Assert.AreEqual("tok-123", controller.Auth.Token);
        Assert.AreEqual("alice", controller.Auth.Username);
    }

    [UnityTest]
    public IEnumerator FactoryAuth_IsGuest_TrueForGuestSession()
    {
        _go = new GameObject("AuthConnectController");
        var controller = _go.AddComponent<AuthConnectController>();
        yield return null;

        controller.Auth.ApplyAuthResponse(
            "{\"token\":\"g1\",\"username\":\"guest-abc\",\"characterName\":\"\",\"isGuest\":true}");

        Assert.IsTrue(controller.Auth.IsGuest);
    }
}
