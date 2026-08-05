using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.PersistentWorld;
using Game.Environment;

public class SupplyCrateCoinSpillPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private SupplyCrateInteractable SpawnCrate()
    {
        var go = new GameObject("Crate");
        _spawned.Add(go);
        var collider = go.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        var animator = go.AddComponent<OneShotPropAnimator>();
        animator.GlbUrl = "SupplyCrate/SupplyCrate.glb";
        animator.ClipName = "Open";
        go.AddComponent<NetworkedEntity>();
        return go.AddComponent<SupplyCrateInteractable>();
    }

    // A minimal stand-in for CoinSpill.prefab: a VfxRecipe root with a child ParticleSystem, so
    // VfxRecipe.Play()/IsPlaying behave exactly like the real prefab without depending on the
    // asset (or its texture/material) being importable in a PlayMode test run.
    private GameObject BuildCoinSpillTestDouble()
    {
        var root = new GameObject("CoinSpillTestDouble");
        _spawned.Add(root);
        root.AddComponent<VfxRecipe>();
        var child = new GameObject("Coins");
        child.transform.SetParent(root.transform, false);
        child.AddComponent<ParticleSystem>();
        return root;
    }

    private static IEnumerator WaitUntilLoaded(OneShotPropAnimator animator, float timeoutSeconds = 30f)
    {
        var deadline = Time.realtimeSinceStartup + timeoutSeconds;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(animator.IsLoaded, "SupplyCrate GLB did not finish loading within the timeout");
    }

    // Matches only INSTANTIATED clones, not the original test-double GameObject Instantiate()
    // copies from -- both share the "CoinSpillTestDouble" name, but only a spawned clone gets the
    // "(Clone)" suffix Unity appends.
    private static List<GameObject> FindSpawnedCoinClones()
    {
        return Object.FindObjectsByType<VfxRecipe>(FindObjectsInactive.Include)
            .Where(r => r.gameObject.name == "CoinSpillTestDouble(Clone)")
            .Select(r => r.gameObject)
            .ToList();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _spawned)
        {
            if (go != null) Object.Destroy(go);
        }
        _spawned.Clear();
        foreach (var clone in FindSpawnedCoinClones())
        {
            Object.Destroy(clone);
        }
    }

    [UnityTest]
    public IEnumerator HandleCrateOpened_SpawnsCoinSpillVfxAndPlaysIt()
    {
        var crate = SpawnCrate();
        var coinPrefab = BuildCoinSpillTestDouble();
        crate.ConfigureCoinSpill(coinPrefab);

        crate.HandleCrateOpened();
        yield return null;

        var clones = FindSpawnedCoinClones();
        Assert.AreEqual(1, clones.Count, "Exactly one coin-spill VFX instance must be spawned.");

        var recipe = clones[0].GetComponent<VfxRecipe>();
        Assert.IsNotNull(recipe);
        Assert.IsTrue(recipe.IsPlaying, "VfxRecipe.Play() must have been called on the spawned instance.");
    }

    [UnityTest]
    public IEnumerator HandleCrateOpened_CalledTwice_SpawnsCoinSpillOnlyOnce()
    {
        var crate = SpawnCrate();
        var coinPrefab = BuildCoinSpillTestDouble();
        crate.ConfigureCoinSpill(coinPrefab);

        crate.HandleCrateOpened();
        crate.HandleCrateOpened();
        yield return null;

        Assert.AreEqual(1, FindSpawnedCoinClones().Count,
            "A redundant OnHold-equivalent call must never spawn the coin-spill effect twice.");
    }

    [Test]
    public void HandleCrateOpened_WithNoCoinSpillPrefabAssigned_DoesNotThrow()
    {
        var crate = SpawnCrate();
        Assert.DoesNotThrow(() => crate.HandleCrateOpened());
    }

    [UnityTest]
    public IEnumerator CrateOpening_ReachesHeldPose_SpawnsCoinSpillExactlyOnce()
    {
        // End-to-end proof of the real wiring: OneShotPropAnimator.OnHold (not OnPlay) drives the
        // spawn, via the OnEnable subscription -- not by calling HandleCrateOpened() directly.
        var crate = SpawnCrate();
        var animator = crate.GetComponent<OneShotPropAnimator>();
        animator.Load();
        yield return WaitUntilLoaded(animator);

        var coinPrefab = BuildCoinSpillTestDouble();
        crate.ConfigureCoinSpill(coinPrefab);

        Assert.AreEqual(0, FindSpawnedCoinClones().Count, "No coins before the crate opens.");

        crate.HandleToggleChanged(true); // the authoritative delta that starts the open animation

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsHeld && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(animator.IsHeld, "Precondition: the crate must reach its held-open pose.");

        yield return null; // let the OnHold UnityEvent + Instantiate() flush this frame

        Assert.AreEqual(1, FindSpawnedCoinClones().Count,
            "The coin-spill VFX must spawn exactly once, once the lid is actually open (OnHold).");
    }
}
