using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.PersistentWorld;
using Game.Environment;

public class SupplyCrateInteractablePlayTests
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
    public IEnumerator HandleToggleChanged_True_OpensCrateAndPlaysAnimator()
    {
        var crate = SpawnCrate();
        crate.GetComponent<OneShotPropAnimator>().Load();

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!crate.GetComponent<OneShotPropAnimator>().IsLoaded && Time.realtimeSinceStartup < deadline)
            yield return null;

        Assert.IsFalse(crate.IsOpen, "Crate must start closed.");

        crate.HandleToggleChanged(true);
        yield return null;

        Assert.IsTrue(crate.IsOpen, "Crate must be open after a true toggle.");
        Assert.IsTrue(crate.GetComponent<OneShotPropAnimator>().IsPlaying,
            "Crate's open animation must start playing.");
    }

    [Test]
    public void HandleToggleChanged_False_DoesNotOpenCrate()
    {
        var crate = SpawnCrate();

        crate.HandleToggleChanged(false);

        Assert.IsFalse(crate.IsOpen, "A false toggle must never open the crate.");
    }

    [UnityTest]
    public IEnumerator HandleToggleChanged_CalledTwice_StaysOpen_NeverRecloses()
    {
        var crate = SpawnCrate();
        crate.GetComponent<OneShotPropAnimator>().Load();

        var deadline = Time.realtimeSinceStartup + 30f;
        while (!crate.GetComponent<OneShotPropAnimator>().IsLoaded && Time.realtimeSinceStartup < deadline)
            yield return null;

        crate.HandleToggleChanged(true);
        yield return null;
        Assert.IsTrue(crate.IsOpen);

        // A second true delta (e.g. a late-joining player's initial snapshot) must not throw or
        // restart the animation from closed -- the crate opens exactly once and holds forever.
        crate.HandleToggleChanged(true);
        yield return null;

        Assert.IsTrue(crate.IsOpen, "Crate must remain open, never reset by a redundant toggle.");
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter_WithPlayerTag_DoesNotLocallyOpenCrate_WithNoLiveConnection()
    {
        // Proves SupplyCrateInteractable never optimistically opens itself -- it only reacts to
        // the authoritative NetworkedEntity.ToggleChanged event, never to the trigger directly.
        // With no WorldConnection in this test scene, NetworkedEntity.Toggle() silently no-ops,
        // so the crate must still read as closed right after the trigger fires.
        var crate = SpawnCrate();
        crate.transform.position = Vector3.zero;

        var player = new GameObject("Player");
        _spawned.Add(player);
        player.tag = "Player";
        var playerCollider = player.AddComponent<CapsuleCollider>();
        playerCollider.isTrigger = false;
        var rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        player.transform.position = new Vector3(5f, 0f, 0f);

        yield return new WaitForFixedUpdate();
        yield return null;

        player.transform.position = Vector3.zero;

        yield return new WaitForFixedUpdate();
        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsFalse(crate.IsOpen, "No live WorldConnection -- the trigger must not fake-open the crate locally.");
    }
}
