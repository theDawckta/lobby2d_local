using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OneTimeGames.CoreSystems;

public class SupplyCratePrefabPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private OneShotPropAnimator SpawnConfiguredAnimator()
    {
        var go = new GameObject("SupplyCrate");
        _spawned.Add(go);
        var animator = go.AddComponent<OneShotPropAnimator>();
        animator.GlbUrl = "SupplyCrate/SupplyCrate.glb";
        animator.ClipName = "Open";
        return animator;
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

    private static IEnumerator WaitUntilLoaded(OneShotPropAnimator animator, float timeoutSeconds = 30f)
    {
        var deadline = Time.realtimeSinceStartup + timeoutSeconds;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(animator.IsLoaded, "SupplyCrate GLB did not finish loading within the timeout");
    }

    [UnityTest]
    public IEnumerator SupplyCrateGlb_LoadsWithinTimeout()
    {
        var animator = SpawnConfiguredAnimator();
        animator.Load();

        yield return WaitUntilLoaded(animator);
    }

    [UnityTest]
    public IEnumerator SupplyCrateGlb_StaysClosed_UntilPlayIsCalled()
    {
        var animator = SpawnConfiguredAnimator();
        animator.Load();

        yield return WaitUntilLoaded(animator);

        Assert.IsFalse(animator.IsPlaying, "Crate must not auto-play on load");
        Assert.IsFalse(animator.IsHeld, "Crate must not be open before Play() is called");
    }

    [UnityTest]
    public IEnumerator SupplyCrateGlb_OpensAndHolds_AfterPlay()
    {
        var animator = SpawnConfiguredAnimator();
        animator.Load();

        yield return WaitUntilLoaded(animator);

        animator.Play();
        yield return null;

        Assert.IsTrue(animator.IsPlaying, "Crate should be playing its one-shot open animation right after Play()");

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsHeld && Time.realtimeSinceStartup < deadline) yield return null;

        Assert.IsTrue(animator.IsHeld, "Crate should be holding its final open pose after the clip finishes");
        Assert.IsFalse(animator.IsPlaying, "Crate should no longer report playing once it is held open");
    }
}
