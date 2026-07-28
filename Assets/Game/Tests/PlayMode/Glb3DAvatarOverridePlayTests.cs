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
    public IEnumerator Override_RescalesAvatarRoot_ToCompensateForTheGlbsOwnHeight()
    {
        // Regression test for the "cop mesh is way too big" bug: WorldPresence sets avatarRoot's
        // localScale to Vector3.one * avatarScale BEFORE calling the override (calibrated for 1-unit-
        // tall 2D sprites) -- the override must correct it for the GLB's real (2-unit) bind-pose
        // height, not leave the raw avatarScale in place.
        var go = Spawn("Player");
        var presence = go.AddComponent<WorldPresence>();
        presence.charactersBaseUrl = "https://factory.tehfaktoree.com";
        presence.avatarScale = 5f;
        go.AddComponent<Glb3DAvatarOverride>();
        yield return null;

        var avatarRoot = Spawn("Avatar");
        avatarRoot.transform.localScale = Vector3.one * presence.avatarScale; // mirrors WorldPresence's own pre-step
        presence.avatarViewOverride("zeke", avatarRoot, true);

        Assert.AreEqual(2.5f, avatarRoot.transform.localScale.x, 0.0001f);
        Assert.AreEqual(2.5f, avatarRoot.transform.localScale.y, 0.0001f);
        Assert.AreEqual(2.5f, avatarRoot.transform.localScale.z, 0.0001f);
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
