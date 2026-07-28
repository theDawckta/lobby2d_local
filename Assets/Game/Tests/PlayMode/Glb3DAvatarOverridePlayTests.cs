using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Player;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.Presence;

public class Glb3DAvatarOverridePlayTests
{
    private readonly List<GameObject> _spawned = new();

    private GameObject Spawn(string name)
    {
        var go = new GameObject(name);
        _spawned.Add(go);
        return go;
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
    public IEnumerator Awake_RegistersOverrideHook_OnWorldPresence()
    {
        var go = Spawn("Player");
        var presence = go.AddComponent<WorldPresence>();
        go.AddComponent<Glb3DAvatarOverride>();
        yield return null;

        Assert.IsNotNull(presence.avatarViewOverride);
    }

    [UnityTest]
    public IEnumerator Override_AddsGlbCharacterAnimatorAndMotion_ForConfiguredCharacter()
    {
        var go = Spawn("Player");
        var presence = go.AddComponent<WorldPresence>();
        presence.charactersBaseUrl = "https://factory.tehfaktoree.com";
        go.AddComponent<Glb3DAvatarOverride>();
        yield return null;

        var avatarRoot = Spawn("Avatar");
        var handled = presence.avatarViewOverride("zeke", avatarRoot, true);

        Assert.IsTrue(handled);
        Assert.IsNotNull(avatarRoot.GetComponent<GlbCharacterAnimator>());
        Assert.IsNotNull(avatarRoot.GetComponent<GlbAvatarMotion>());
    }

    [UnityTest]
    public IEnumerator Override_MatchesConfiguredCharacterName_CaseInsensitively()
    {
        var go = Spawn("Player");
        var presence = go.AddComponent<WorldPresence>();
        presence.charactersBaseUrl = "https://factory.tehfaktoree.com";
        go.AddComponent<Glb3DAvatarOverride>();
        yield return null;

        var avatarRoot = Spawn("Avatar");
        var handled = presence.avatarViewOverride("ZEKE", avatarRoot, true);

        Assert.IsTrue(handled);
    }

    [UnityTest]
    public IEnumerator Override_ReturnsFalse_ForUnconfiguredCharacter()
    {
        var go = Spawn("Player");
        var presence = go.AddComponent<WorldPresence>();
        presence.charactersBaseUrl = "https://factory.tehfaktoree.com";
        go.AddComponent<Glb3DAvatarOverride>();
        yield return null;

        var avatarRoot = Spawn("Avatar");
        var handled = presence.avatarViewOverride("dummy", avatarRoot, true);

        Assert.IsFalse(handled);
        Assert.IsNull(avatarRoot.GetComponent<GlbCharacterAnimator>());
    }

    [UnityTest]
    public IEnumerator Override_ReturnsFalse_WhenCharactersBaseUrlNotSet()
    {
        var go = Spawn("Player");
        var presence = go.AddComponent<WorldPresence>();
        go.AddComponent<Glb3DAvatarOverride>();
        yield return null;

        var avatarRoot = Spawn("Avatar");
        var handled = presence.avatarViewOverride("zeke", avatarRoot, true);

        Assert.IsFalse(handled);
    }
}
