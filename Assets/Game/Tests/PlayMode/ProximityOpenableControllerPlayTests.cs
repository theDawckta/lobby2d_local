using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.PersistentWorld;
using Game.Environment;

public class ProximityOpenableControllerPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private ProximityOpenableController SpawnDoor()
    {
        var go = new GameObject("CaveDoor");
        _spawned.Add(go);
        go.AddComponent<SpriteRenderer>();
        var animator = go.AddComponent<OneShotSpriteAnimator>();
        // Two frames so Open()/Close() have somewhere to animate to (frame 0 closed, 1 open).
        animator.SetFrames(new[] { Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f),
                                   Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f) });
        go.AddComponent<NetworkedEntity>();
        return go.AddComponent<ProximityOpenableController>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _spawned) if (go != null) Object.Destroy(go);
        _spawned.Clear();
    }

    [Test]
    public void StartsClosed()
    {
        var door = SpawnDoor();
        Assert.IsFalse(door.IsOpen, "Door must start closed.");
    }

    [UnityTest]
    public IEnumerator HandleToggleChanged_True_OpensDoorAndPlaysAnimator()
    {
        var door = SpawnDoor();
        yield return null;

        door.HandleToggleChanged(true);
        yield return null;

        Assert.IsTrue(door.IsOpen, "Door must be open after a true toggle.");
        Assert.IsTrue(door.GetComponent<OneShotSpriteAnimator>().IsOpen,
            "The animator must be driven to its open pose.");
    }

    [UnityTest]
    public IEnumerator HandleToggleChanged_ReversesOpenAndClosed()
    {
        var door = SpawnDoor();
        yield return null;

        door.HandleToggleChanged(true);
        yield return null;
        Assert.IsTrue(door.IsOpen);

        // Unlike the one-way crate, the door CLOSES again when the authoritative state flips back.
        door.HandleToggleChanged(false);
        yield return null;
        Assert.IsFalse(door.IsOpen, "Door must close when the authoritative toggle flips back to false.");
    }

    [UnityTest]
    public IEnumerator NoLiveConnection_NoPlayers_DoorStaysClosed()
    {
        // With no WorldConnection and no roster, the proximity poll finds nobody in range and the
        // door never opens itself -- proving it never fakes state without either a player or the server.
        var door = SpawnDoor();
        door.transform.position = Vector3.zero;

        yield return null;
        yield return null;

        Assert.IsFalse(door.IsOpen, "No players in range -- the door must stay closed.");
    }

    [Test]
    public void WithinXZ_IgnoresY_AndRespectsRange()
    {
        var door = Vector3.zero;
        Assert.IsTrue(ProximityOpenableController.WithinXZ(new Vector3(3f, 100f, 0f), door, 4f),
            "Within range on XZ (Y ignored) must count as near.");
        Assert.IsFalse(ProximityOpenableController.WithinXZ(new Vector3(5f, 0f, 0f), door, 4f),
            "Beyond range must not count as near.");
    }

    [Test]
    public void AnyInRange_TrueWhenAnyPositionNear()
    {
        var door = Vector3.zero;
        var far = new[] { new Vector3(10f, 0f, 0f), new Vector3(0f, 0f, 9f) };
        Assert.IsFalse(ProximityOpenableController.AnyInRange(far, door, 4f));

        var mixed = new[] { new Vector3(10f, 0f, 0f), new Vector3(1f, 0f, 1f) };
        Assert.IsTrue(ProximityOpenableController.AnyInRange(mixed, door, 4f),
            "One nearby player among far ones must still open the door.");
    }
}
