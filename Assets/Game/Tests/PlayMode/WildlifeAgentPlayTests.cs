using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Wildlife;
using OneTimeGames.CoreSystems;

public class WildlifeAgentPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private GameObject Spawn(string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.position = position;
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
    public IEnumerator Initialize_StartsInWanderingState()
    {
        var go = Spawn("Deer", Vector3.zero);
        var agent = go.AddComponent<WildlifeAgent>();
        agent.Initialize(Vector3.zero, new Vector3(10f, 0f, 10f), new List<Transform>());
        yield return null;

        Assert.AreEqual(WildlifeState.Wandering, agent.CurrentState);
    }

    [UnityTest]
    public IEnumerator Agent_MovesOverTime_WhileWandering()
    {
        var go = Spawn("Rabbit", Vector3.zero);
        var agent = go.AddComponent<WildlifeAgent>();
        agent.Initialize(Vector3.zero, new Vector3(20f, 0f, 20f), new List<Transform>());
        var startPosition = go.transform.position;

        // Give it several frames to move toward its wander target.
        for (var i = 0; i < 30; i++) yield return null;

        Assert.AreNotEqual(startPosition, go.transform.position);
    }

    [UnityTest]
    public IEnumerator Agent_StaysWithinAreaBounds_WhileWandering()
    {
        var center = Vector3.zero;
        var size = new Vector3(6f, 0f, 6f);
        var go = Spawn("Bird", Vector3.zero);
        var agent = go.AddComponent<WildlifeAgent>();
        agent.Initialize(center, size, new List<Transform>());

        for (var i = 0; i < 60; i++)
        {
            yield return null;
            Assert.LessOrEqual(Mathf.Abs(go.transform.position.x), size.x * 0.5f + 0.01f);
            Assert.LessOrEqual(Mathf.Abs(go.transform.position.z), size.z * 0.5f + 0.01f);
        }
    }

    [UnityTest]
    public IEnumerator Agent_EntersFleeingState_WhenPlayerRegisteredNearby()
    {
        var go = Spawn("Deer", Vector3.zero);
        var agent = go.AddComponent<WildlifeAgent>();
        var playerGo = Spawn("Player", new Vector3(1f, 0f, 0f));
        var players = new List<Transform> { playerGo.transform };

        agent.Initialize(Vector3.zero, new Vector3(30f, 0f, 30f), players);
        yield return null;

        Assert.AreEqual(WildlifeState.Fleeing, agent.CurrentState);
    }

    [UnityTest]
    public IEnumerator Agent_MovesAwayFromPlayer_WhileFleeing()
    {
        var go = Spawn("Rabbit", Vector3.zero);
        var agent = go.AddComponent<WildlifeAgent>();
        var playerGo = Spawn("Player", new Vector3(1f, 0f, 0f));
        var players = new List<Transform> { playerGo.transform };

        agent.Initialize(Vector3.zero, new Vector3(40f, 0f, 40f), players);

        var startDistance = Vector3.Distance(go.transform.position, playerGo.transform.position);
        for (var i = 0; i < 20; i++) yield return null;
        var laterDistance = Vector3.Distance(go.transform.position, playerGo.transform.position);

        Assert.Greater(laterDistance, startDistance);
    }

    [UnityTest]
    public IEnumerator Agent_ReturnsToWandering_WhenPlayerMovesAway()
    {
        var go = Spawn("Deer", Vector3.zero);
        var agent = go.AddComponent<WildlifeAgent>();
        var playerGo = Spawn("Player", new Vector3(1f, 0f, 0f));
        var players = new List<Transform> { playerGo.transform };

        agent.Initialize(Vector3.zero, new Vector3(30f, 0f, 30f), players);
        yield return null;
        Assert.AreEqual(WildlifeState.Fleeing, agent.CurrentState);

        playerGo.transform.position = new Vector3(1000f, 0f, 1000f);
        yield return null;

        Assert.AreEqual(WildlifeState.Wandering, agent.CurrentState);
    }

    [UnityTest]
    public IEnumerator Agent_DoesNotThrow_WhenGlbCharacterAnimatorPresentButUnloaded()
    {
        var go = Spawn("Deer", Vector3.zero);
        go.AddComponent<GlbCharacterAnimator>();
        var agent = go.AddComponent<WildlifeAgent>();
        agent.Initialize(Vector3.zero, new Vector3(20f, 0f, 20f), new List<Transform>());

        for (var i = 0; i < 5; i++) yield return null;

        Assert.Pass(); // reaching here with no exception proves SetSpeed() on an unloaded animator is safe
    }

    [UnityTest]
    public IEnumerator Agent_DoesNothing_BeforeInitialize()
    {
        var go = Spawn("Deer", new Vector3(5f, 0f, 5f));
        go.AddComponent<WildlifeAgent>();
        yield return null;
        yield return null;

        Assert.AreEqual(new Vector3(5f, 0f, 5f), go.transform.position);
    }
}
