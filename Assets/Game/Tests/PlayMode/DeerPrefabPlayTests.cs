using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OneTimeGames.CoreSystems;

public class DeerPrefabPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private GlbCharacterAnimator SpawnConfiguredAnimator()
    {
        var go = new GameObject("Deer");
        _spawned.Add(go);
        var animator = go.AddComponent<GlbCharacterAnimator>();
        animator.GlbUrl = "Wildlife/DeerWildlife.glb";
        animator.IdleClip = "Idle";
        animator.MoveClip = "Walk";
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

    [UnityTest]
    public IEnumerator DeerGlb_LoadsWithinTimeout()
    {
        var animator = SpawnConfiguredAnimator();

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;

        Assert.IsTrue(animator.IsLoaded, "Deer GLB did not finish loading within 30 seconds");
        Assert.IsTrue(animator.HasClips, "Loaded Deer GLB has no baked animation clips");
    }

    [UnityTest]
    public IEnumerator DeerGlb_PlaysIdleClip_AfterLoad()
    {
        var animator = SpawnConfiguredAnimator();

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(animator.IsLoaded, "Deer GLB did not finish loading within 30 seconds");

        Assert.AreEqual("Idle", animator.CurrentClip);
    }

    [UnityTest]
    public IEnumerator DeerGlb_PlaysWalkClip_WhenSpeedAboveThreshold()
    {
        var animator = SpawnConfiguredAnimator();

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(animator.IsLoaded, "Deer GLB did not finish loading within 30 seconds");

        animator.SetSpeed(2f);
        yield return null;

        Assert.AreEqual("Walk", animator.CurrentClip);
    }

    [UnityTest]
    public IEnumerator DeerGlb_ReturnsToIdleClip_WhenSpeedDropsBelowThreshold()
    {
        var animator = SpawnConfiguredAnimator();

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(animator.IsLoaded, "Deer GLB did not finish loading within 30 seconds");

        animator.SetSpeed(2f);
        yield return null;
        animator.SetSpeed(0f);
        yield return null;

        Assert.AreEqual("Idle", animator.CurrentClip);
    }

    [UnityTest]
    public IEnumerator DeerGlb_ModelRoot_IsInstantiated_AfterLoad()
    {
        var animator = SpawnConfiguredAnimator();

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(animator.IsLoaded, "Deer GLB did not finish loading within 30 seconds");

        Assert.IsNotNull(animator.ModelRoot);
    }
}
