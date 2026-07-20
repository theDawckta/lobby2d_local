using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using OneTimeGames.CoreSystems;

public class BirdPrefabEditTests
{
    private const string PrefabPath = "Assets/Game/Prefabs/Bird.prefab";

    [Test]
    public void BirdPrefab_Exists()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab, $"Expected a prefab asset at {PrefabPath}");
    }

    [Test]
    public void BirdPrefab_HasGlbCharacterAnimatorWiredCorrectly()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var animator = prefab.GetComponent<GlbCharacterAnimator>();

        Assert.IsNotNull(animator, "Bird prefab must carry a GlbCharacterAnimator");
        Assert.AreEqual("Wildlife/BirdWildlife.glb", animator.GlbUrl);
        Assert.AreEqual("Idle", animator.IdleClip);
        Assert.AreEqual("Walk", animator.MoveClip);
    }

    [Test]
    public void BirdWildlifeGlb_ShippedInStreamingAssets()
    {
        var fullPath = Path.Combine(Application.dataPath, "StreamingAssets", "Wildlife", "BirdWildlife.glb");
        Assert.IsTrue(File.Exists(fullPath), $"Expected the bird GLB at {fullPath}");
    }
}
