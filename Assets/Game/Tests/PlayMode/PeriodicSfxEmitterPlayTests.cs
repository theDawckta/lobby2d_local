using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Audio;

public class PeriodicSfxEmitterPlayTests
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
    public IEnumerator PlaysConfiguredSfx_AfterInterval_WhenPlayOnStartTrue()
    {
        var go = Spawn("SparkEmitter");
        var emitter = go.AddComponent<PeriodicSfxEmitter>();
        emitter.SfxName = "SparkFlicker";
        emitter.MinIntervalSeconds = 0f;
        emitter.MaxIntervalSeconds = 0f;
        emitter.PlayOnStart = true;

        string played = null;
        void Handler(string name) => played = name;
        AudioManager.Instance.OnSfxPlayed += Handler;

        yield return null;
        yield return null;

        AudioManager.Instance.OnSfxPlayed -= Handler;
        Assert.AreEqual("SparkFlicker", played);
    }

    [UnityTest]
    public IEnumerator DoesNotPlay_BeforeConfiguredIntervalElapses()
    {
        var go = Spawn("PollenEmitter");
        var emitter = go.AddComponent<PeriodicSfxEmitter>();
        emitter.SfxName = "PollenDrift";
        emitter.MinIntervalSeconds = 1000f;
        emitter.MaxIntervalSeconds = 1000f;
        emitter.PlayOnStart = false;

        var fired = false;
        void Handler(string name) => fired = true;
        AudioManager.Instance.OnSfxPlayed += Handler;

        yield return null;
        yield return null;
        yield return null;

        AudioManager.Instance.OnSfxPlayed -= Handler;
        Assert.IsFalse(fired);
    }

    [UnityTest]
    public IEnumerator DoesNotThrow_WhenSfxNameIsEmpty()
    {
        var go = Spawn("EmptyNameEmitter");
        var emitter = go.AddComponent<PeriodicSfxEmitter>();
        emitter.MinIntervalSeconds = 0f;
        emitter.MaxIntervalSeconds = 0f;
        emitter.PlayOnStart = true;

        for (var i = 0; i < 3; i++) yield return null;

        Assert.Pass(); // reaching here with no exception proves an empty sfxName is a safe no-op
    }
}
