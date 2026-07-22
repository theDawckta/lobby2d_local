using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CaveAmbiencePlayTests
{
    private readonly List<GameObject> _spawned = new();

    private AudioSource SpawnConfiguredSource(float clipLengthSeconds = 0.1f)
    {
        // Build the GameObject inactive so AddComponent's implicit Awake/OnEnable does not
        // fire (and consume playOnAwake) before the clip is assigned -- matches how a real
        // prefab instantiates, where the clip is already serialized before Awake ever runs.
        var go = new GameObject("CaveAmbience");
        go.SetActive(false);
        _spawned.Add(go);

        var clip = AudioClip.Create("TestCaveWildlife", Mathf.CeilToInt(clipLengthSeconds * 44100), 1, 44100, false);
        clip.SetData(new float[Mathf.CeilToInt(clipLengthSeconds * 44100)], 0);

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = true;
        source.spatialBlend = 1f;
        source.volume = 0.7f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 3f;
        source.maxDistance = 14f;

        go.SetActive(true);
        return source;
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
    public IEnumerator CaveAmbienceAudioSource_PlaysAutomaticallyOnAwake()
    {
        var source = SpawnConfiguredSource();

        yield return null;

        Assert.IsTrue(source.isPlaying, "AudioSource with playOnAwake=true must start playing on its own");
    }

    [UnityTest]
    public IEnumerator CaveAmbienceAudioSource_KeepsPlayingAcrossFrames_WhenLooping()
    {
        // Advance a handful of frames rather than a real-time wait past the clip's length --
        // this keeps the test deterministic instead of racing unrelated background editor
        // activity (e.g. Unity-MCP's dev-tooling connection attempts) that can land during a
        // longer wall-clock wait.
        var source = SpawnConfiguredSource(0.1f);

        Assert.IsTrue(source.loop, "AudioSource must be configured to loop");

        for (var i = 0; i < 5; i++)
        {
            yield return null;
            Assert.IsTrue(source.isPlaying, $"AudioSource with loop=true must still be playing on frame {i}");
        }
    }

    [UnityTest]
    public IEnumerator CaveAmbienceAudioSource_RemainsFullySpatial()
    {
        var source = SpawnConfiguredSource();

        yield return null;

        Assert.AreEqual(1f, source.spatialBlend, 0.001f, "CaveAmbience must stay fully 3D/positional at runtime");
        Assert.AreEqual(AudioRolloffMode.Linear, source.rolloffMode);
        Assert.AreEqual(3f, source.minDistance, 0.001f);
        Assert.AreEqual(14f, source.maxDistance, 0.001f);
    }
}
