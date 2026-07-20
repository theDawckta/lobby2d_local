using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Wildlife;

public class WildlifeManagerPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private GameObject Spawn(string name)
    {
        var go = new GameObject(name);
        _spawned.Add(go);
        return go;
    }

    private GameObject MakePrefabLikeObject(string name)
    {
        // WildlifeManager.SpawnOne works on any GameObject reference (Instantiate accepts a plain
        // scene object the same way it accepts a real .prefab asset), so a bare GameObject stands
        // in for a wildlife prefab in these tests without needing real creature-3d art assets.
        // Must stay ACTIVE: Instantiate() copies the source's activeSelf onto the clone, so an
        // inactive stand-in would spawn wildlife whose Update() never runs.
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
    public IEnumerator SpawnAll_SpawnsConfiguredCountPerEntry()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();
        manager.SpawnOnStart = false; // testing the explicit SpawnAll() call in isolation
        var deerPrefab = MakePrefabLikeObject("DeerPrefab");
        var rabbitPrefab = MakePrefabLikeObject("RabbitPrefab");
        manager.SpawnEntries.Add(new WildlifeSpawnEntry { prefab = deerPrefab, count = 2 });
        manager.SpawnEntries.Add(new WildlifeSpawnEntry { prefab = rabbitPrefab, count = 3 });

        manager.SpawnAll();
        yield return null;

        Assert.AreEqual(5, manager.SpawnedWildlife.Count);
        foreach (var go in manager.SpawnedWildlife) _spawned.Add(go.gameObject);
    }

    [UnityTest]
    public IEnumerator Start_SpawnsAutomatically_WhenSpawnOnStartTrue()
    {
        var managerGo = Spawn("WildlifeManager");
        var prefab = MakePrefabLikeObject("BirdPrefab");
        var manager = managerGo.AddComponent<WildlifeManager>();
        manager.SpawnEntries.Add(new WildlifeSpawnEntry { prefab = prefab, count = 1 });

        yield return null; // let Start() run

        Assert.AreEqual(1, manager.SpawnedWildlife.Count);
        foreach (var go in manager.SpawnedWildlife) _spawned.Add(go.gameObject);
    }

    [UnityTest]
    public IEnumerator SpawnedWildlife_AreWithinConfiguredAreaBounds()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();
        manager.SpawnOnStart = false;
        manager.AreaCenter = new Vector3(5f, 0f, -5f);
        manager.AreaSize = new Vector3(4f, 0f, 4f);
        var prefab = MakePrefabLikeObject("DeerPrefab");
        manager.SpawnEntries.Add(new WildlifeSpawnEntry { prefab = prefab, count = 10 });

        manager.SpawnAll();
        yield return null;

        foreach (var agent in manager.SpawnedWildlife)
        {
            _spawned.Add(agent.gameObject);
            Assert.LessOrEqual(Mathf.Abs(agent.transform.position.x - manager.AreaCenter.x), manager.AreaSize.x * 0.5f + 0.001f);
            Assert.LessOrEqual(Mathf.Abs(agent.transform.position.z - manager.AreaCenter.z), manager.AreaSize.z * 0.5f + 0.001f);
        }
    }

    [UnityTest]
    public IEnumerator SpawnAll_AssignsFleeSfxName_FromSpawnEntry()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();
        manager.SpawnOnStart = false;
        var deerPrefab = MakePrefabLikeObject("DeerPrefab");
        manager.SpawnEntries.Add(new WildlifeSpawnEntry { prefab = deerPrefab, count = 1, fleeSfxName = "DeerGrunt" });

        manager.SpawnAll();
        yield return null;

        foreach (var go in manager.SpawnedWildlife) _spawned.Add(go.gameObject);
        Assert.AreEqual("DeerGrunt", manager.SpawnedWildlife[0].FleeSfxName);
    }

    [UnityTest]
    public IEnumerator SpawnOne_AddsWildlifeAgentComponent_WhenPrefabLacksOne()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();
        var prefab = MakePrefabLikeObject("RabbitPrefab");

        var agent = manager.SpawnOne(prefab);
        yield return null;

        Assert.IsNotNull(agent);
        _spawned.Add(agent.gameObject);
        Assert.IsNotNull(agent.GetComponent<WildlifeAgent>());
    }

    [UnityTest]
    public IEnumerator SpawnOne_ReturnsNull_WhenPrefabIsNull()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();

        var agent = manager.SpawnOne(null);
        yield return null;

        Assert.IsNull(agent);
    }

    [UnityTest]
    public IEnumerator RegisterPlayer_AddsToPlayersList()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();
        var playerGo = Spawn("Player");

        manager.RegisterPlayer(playerGo.transform);
        yield return null;

        Assert.AreEqual(1, manager.Players.Count);
        Assert.AreSame(playerGo.transform, manager.Players[0]);
    }

    [UnityTest]
    public IEnumerator RegisterPlayer_DoesNotAddDuplicate()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();
        var playerGo = Spawn("Player");

        manager.RegisterPlayer(playerGo.transform);
        manager.RegisterPlayer(playerGo.transform);
        yield return null;

        Assert.AreEqual(1, manager.Players.Count);
    }

    [UnityTest]
    public IEnumerator UnregisterPlayer_RemovesFromPlayersList()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();
        var playerGo = Spawn("Player");
        manager.RegisterPlayer(playerGo.transform);

        manager.UnregisterPlayer(playerGo.transform);
        yield return null;

        Assert.AreEqual(0, manager.Players.Count);
    }

    [UnityTest]
    public IEnumerator SpawnedAgents_FleeFromPlayerRegisteredAfterSpawn()
    {
        var managerGo = Spawn("WildlifeManager");
        var manager = managerGo.AddComponent<WildlifeManager>();
        manager.SpawnOnStart = false;
        manager.AreaCenter = Vector3.zero;
        manager.AreaSize = new Vector3(30f, 0f, 30f);
        var prefab = MakePrefabLikeObject("DeerPrefab");
        manager.SpawnEntries.Add(new WildlifeSpawnEntry { prefab = prefab, count = 1 });
        manager.SpawnAll();
        var agent = manager.SpawnedWildlife[0];
        _spawned.Add(agent.gameObject);
        agent.transform.position = Vector3.zero;

        var playerGo = Spawn("Player");
        playerGo.transform.position = new Vector3(0.5f, 0f, 0f);
        manager.RegisterPlayer(playerGo.transform);

        // The manager passes its live players list by reference into each spawned agent, so a
        // player registered AFTER spawn must still be seen by already-spawned wildlife. Poll
        // instead of asserting after a single frame -- state transitions happen in Update(),
        // whose ordering relative to this coroutine's resume isn't guaranteed on the same frame.
        var elapsedFrames = 0;
        while (agent.CurrentState != WildlifeState.Fleeing && elapsedFrames < 30)
        {
            yield return null;
            elapsedFrames++;
        }

        Assert.AreEqual(WildlifeState.Fleeing, agent.CurrentState);
    }
}
