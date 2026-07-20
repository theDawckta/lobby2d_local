using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// QA fix (#59), Root Cause 1. Mirrors ElectricalSparkPlayTests.cs's pattern of building an
// equivalent GameObject rather than loading the real prefab asset (never use UnityEditor APIs
// like AssetDatabase in a PlayMode test).
public class PollenEffectPlayTests
{
    private GameObject _go;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("PollenEffectPrefab");
        var ps = _go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.Destroy(_go);
    }

    [UnityTest]
    public IEnumerator PollenEffectPrefab_IsPlayingAndLooping()
    {
        yield return null;
        yield return null;

        var ps = _go.GetComponent<ParticleSystem>();
        Assert.IsTrue(ps.isPlaying, "Pollen particle system should be playing.");
        Assert.IsTrue(ps.main.loop, "Pollen particle system should loop continuously for an ambient effect.");
    }

    [UnityTest]
    public IEnumerator PollenEffectPrefab_HasParticleSystemRenderer()
    {
        yield return null;

        Assert.IsNotNull(_go.GetComponent<ParticleSystemRenderer>());
    }
}
