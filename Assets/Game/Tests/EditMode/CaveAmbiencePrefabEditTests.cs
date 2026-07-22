using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CaveAmbiencePrefabEditTests
{
    private const string PrefabPath = "Assets/Game/Prefabs/CaveDoorPrefab.prefab";
    private const string ClipPath = "Assets/Game/Audio/Ambient/CaveWildlife.wav";

    private static AudioSource FindCaveAmbienceSource(GameObject prefab)
    {
        var child = prefab.transform.Find("CaveAmbience");
        Assert.IsNotNull(child, "Expected a child GameObject named 'CaveAmbience' on the CaveDoor prefab");
        var source = child.GetComponent<AudioSource>();
        Assert.IsNotNull(source, "CaveAmbience child must carry an AudioSource component");
        return source;
    }

    [Test]
    public void CaveWildlifeClip_IsImported_WithValidGuid()
    {
        var guid = AssetDatabase.AssetPathToGUID(ClipPath);
        Assert.IsFalse(string.IsNullOrEmpty(guid), $"Expected {ClipPath} to have a valid GUID (be imported)");

        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
        Assert.IsNotNull(clip, $"Expected an AudioClip asset at {ClipPath}");
    }

    [Test]
    public void CaveDoorPrefab_HasCaveAmbienceChild_WithAudioSource()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab, $"Expected a prefab asset at {PrefabPath}");

        var source = FindCaveAmbienceSource(prefab);
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);

        Assert.AreEqual(clip, source.clip, "AudioSource.clip must reference the imported CaveWildlife AudioClip");
    }

    [Test]
    public void CaveAmbienceAudioSource_IsConfiguredForLoopingSpatialPlayback()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var source = FindCaveAmbienceSource(prefab);

        Assert.IsTrue(source.loop, "CaveAmbience AudioSource must loop");
        Assert.IsTrue(source.playOnAwake, "CaveAmbience AudioSource must play on awake");
        Assert.AreEqual(1f, source.spatialBlend, 0.001f, "CaveAmbience AudioSource must be fully 3D (spatialBlend=1)");
        Assert.AreEqual(0.7f, source.volume, 0.001f);
        Assert.AreEqual(AudioRolloffMode.Linear, source.rolloffMode);
        Assert.AreEqual(3f, source.minDistance, 0.001f);
        Assert.AreEqual(14f, source.maxDistance, 0.001f);
    }

    [Test]
    public void CaveAmbienceChild_SitsAtCaveDoorLocalOrigin()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var child = prefab.transform.Find("CaveAmbience");

        Assert.AreEqual(Vector3.zero, child.localPosition, "CaveAmbience should sit at the CaveDoor's local origin");
    }
}
