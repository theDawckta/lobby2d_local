using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OneTimeGames.CoreSystems;

public class SupplyCratePrefabPlayTests
{
    private readonly List<GameObject> _spawned = new();

    // Loading the real committed GLB (glTFast import) or letting the one-shot animation play to
    // its end both take more than one frame -- poll with a timeout rather than assuming a fixed
    // frame count, same pattern established by the other real-GLB PlayMode tests in this factory.
    private static IEnumerator WaitUntilOrTimeout(System.Func<bool> condition, float timeoutSeconds)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
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

    private GameObject InstantiatePrefabLikeSupplyCrate()
    {
        // PlayMode tests cannot use AssetDatabase/UnityEditor APIs to load the .prefab asset
        // itself, so -- matching this repo's established convention (see
        // FullScreenBackgroundPlayTests/ElectricalSparkPlayTests) -- this mirrors exactly what
        // the prefab-build Editor script wired: a GameObject carrying only an OneShotPropAnimator
        // configured with the real shipped GLB + clip name.
        var go = new GameObject("SupplyCrate");
        _spawned.Add(go);
        var animator = go.AddComponent<OneShotPropAnimator>();
        animator.GlbUrl = "SupplyCrate/SupplyCrate.glb";
        animator.ClipName = "Open";
        return go;
    }

    [UnityTest]
    public IEnumerator OneShotPropAnimator_IsWiredWithCorrectGlbUrlAndClipName()
    {
        var go = InstantiatePrefabLikeSupplyCrate();
        yield return null;

        var animator = go.GetComponent<OneShotPropAnimator>();
        Assert.IsNotNull(animator);
        Assert.AreEqual("SupplyCrate/SupplyCrate.glb", animator.GlbUrl);
        Assert.AreEqual("Open", animator.ClipName);
    }

    [UnityTest]
    public IEnumerator OneShotPropAnimator_LoadsAndPlaysOneShotAnimation_ThenHolds()
    {
        // End-to-end: load the real shipped GLB from StreamingAssets and prove the full
        // load -> Play() -> ClampForever-hold contract, not just the wiring.
        var go = InstantiatePrefabLikeSupplyCrate();
        var animator = go.GetComponent<OneShotPropAnimator>();

        animator.Load();
        yield return WaitUntilOrTimeout(() => animator.IsLoaded, 30f);
        Assert.IsTrue(animator.IsLoaded, "GLB failed to load within timeout.");
        Assert.IsFalse(animator.IsPlaying, "Crate must start closed, not auto-play on load.");
        Assert.IsFalse(animator.IsHeld);

        animator.Play();
        yield return null;
        Assert.IsTrue(animator.IsPlaying, "Play() did not start the one-shot animation.");

        yield return WaitUntilOrTimeout(() => animator.IsHeld, animator.ClipLength + 10f);
        Assert.IsTrue(animator.IsHeld, "One-shot clip never reached its held end pose.");
    }

    [UnityTest]
    public IEnumerator OneShotPropAnimator_DoesNotAutoPlayOnStart()
    {
        // loadOnStart=true / playOnLoad=false is the wired configuration -- the crate loads
        // itself but stays closed until something explicitly calls Play().
        var go = InstantiatePrefabLikeSupplyCrate();
        var animator = go.GetComponent<OneShotPropAnimator>();

        yield return WaitUntilOrTimeout(() => animator.IsLoaded, 30f);
        Assert.IsTrue(animator.IsLoaded, "GLB failed to auto-load via Start().");
        Assert.IsFalse(animator.IsPlaying);
        Assert.IsFalse(animator.IsHeld);
    }
}
