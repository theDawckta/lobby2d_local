using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Wildlife;

public class WildlifeMovementTests
{
    private readonly List<GameObject> _spawned = new();

    private Transform SpawnTransform(string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        _spawned.Add(go);
        return go.transform;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _spawned)
        {
            if (go != null) Object.DestroyImmediate(go);
        }
        _spawned.Clear();
    }

    [Test]
    public void PickWanderTarget_ReturnsPointWithinAreaBounds()
    {
        var center = new Vector3(10f, 0f, -5f);
        var size = new Vector3(8f, 0f, 12f);

        for (var i = 0; i < 50; i++)
        {
            var target = WildlifeMovement.PickWanderTarget(center, size);
            Assert.LessOrEqual(Mathf.Abs(target.x - center.x), size.x * 0.5f + 0.0001f);
            Assert.LessOrEqual(Mathf.Abs(target.z - center.z), size.z * 0.5f + 0.0001f);
        }
    }

    [Test]
    public void HasReachedTarget_TrueWhenWithinArrivalDistance()
    {
        var position = new Vector3(0f, 0f, 0f);
        var target = new Vector3(0.1f, 5f, 0.1f); // y difference must be ignored (flat check)
        Assert.IsTrue(WildlifeMovement.HasReachedTarget(position, target, 0.3f));
    }

    [Test]
    public void HasReachedTarget_FalseWhenBeyondArrivalDistance()
    {
        var position = Vector3.zero;
        var target = new Vector3(5f, 0f, 0f);
        Assert.IsFalse(WildlifeMovement.HasReachedTarget(position, target, 0.3f));
    }

    [Test]
    public void ClampToArea_ClampsPositionInsideBounds()
    {
        var center = Vector3.zero;
        var size = new Vector3(10f, 0f, 10f);
        var outside = new Vector3(100f, 3f, -100f);

        var clamped = WildlifeMovement.ClampToArea(outside, center, size);

        Assert.AreEqual(5f, clamped.x, 0.0001f);
        Assert.AreEqual(-5f, clamped.z, 0.0001f);
        Assert.AreEqual(3f, clamped.y, 0.0001f); // y is passed through untouched
    }

    [Test]
    public void ClampToArea_LeavesInBoundsPositionUnchanged()
    {
        var center = Vector3.zero;
        var size = new Vector3(10f, 0f, 10f);
        var inside = new Vector3(2f, 0f, -3f);

        var clamped = WildlifeMovement.ClampToArea(inside, center, size);

        Assert.AreEqual(inside, clamped);
    }

    [Test]
    public void ShouldFlee_TrueWhenPlayerWithinDistance()
    {
        var self = Vector3.zero;
        var player = new Vector3(2f, 0f, 0f);
        Assert.IsTrue(WildlifeMovement.ShouldFlee(self, player, 5f));
    }

    [Test]
    public void ShouldFlee_FalseWhenPlayerBeyondDistance()
    {
        var self = Vector3.zero;
        var player = new Vector3(10f, 0f, 0f);
        Assert.IsFalse(WildlifeMovement.ShouldFlee(self, player, 5f));
    }

    [Test]
    public void ComputeFleeTarget_PointsAwayFromPlayer()
    {
        var self = new Vector3(0f, 0f, 0f);
        var player = new Vector3(1f, 0f, 0f);

        var fleeTarget = WildlifeMovement.ComputeFleeTarget(self, player, 8f);

        // Fleeing from a player to the +X side should move the target toward -X.
        Assert.Less(fleeTarget.x, self.x);
        Assert.AreEqual(8f, Vector3.Distance(self, fleeTarget), 0.001f);
    }

    [Test]
    public void ComputeFleeTarget_PicksDefaultDirection_WhenPlayerExactlyOnTop()
    {
        var self = new Vector3(3f, 0f, 3f);
        var player = new Vector3(3f, 0f, 3f);

        var fleeTarget = WildlifeMovement.ComputeFleeTarget(self, player, 8f);

        Assert.AreEqual(8f, Vector3.Distance(self, fleeTarget), 0.001f);
    }

    [Test]
    public void FindNearestPlayer_ReturnsClosestTransform()
    {
        var self = Vector3.zero;
        var near = SpawnTransform("Near", new Vector3(2f, 0f, 0f));
        var far = SpawnTransform("Far", new Vector3(10f, 0f, 0f));

        var nearest = WildlifeMovement.FindNearestPlayer(self, new List<Transform> { far, near }, out var distance);

        Assert.AreSame(near, nearest);
        Assert.AreEqual(2f, distance, 0.001f);
    }

    [Test]
    public void FindNearestPlayer_ReturnsNull_WhenListEmpty()
    {
        var nearest = WildlifeMovement.FindNearestPlayer(Vector3.zero, new List<Transform>(), out var distance);

        Assert.IsNull(nearest);
        Assert.AreEqual(float.MaxValue, distance);
    }

    [Test]
    public void FindNearestPlayer_ReturnsNull_WhenListIsNull()
    {
        var nearest = WildlifeMovement.FindNearestPlayer(Vector3.zero, null, out _);
        Assert.IsNull(nearest);
    }

    [Test]
    public void FindNearestPlayer_SkipsNullEntries()
    {
        var only = SpawnTransform("Only", new Vector3(3f, 0f, 0f));

        var nearest = WildlifeMovement.FindNearestPlayer(Vector3.zero, new List<Transform> { null, only }, out _);

        Assert.AreSame(only, nearest);
    }
}
