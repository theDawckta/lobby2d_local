using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using OneTimeGames.CoreSystems;

public class SupplyCratePrefabEditTests
{
    private const string PrefabPath = "Assets/Game/Prefabs/SupplyCratePrefab.prefab";

    [Test]
    public void SupplyCratePrefab_Exists()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab, $"Expected a prefab asset at {PrefabPath}");
    }

    [Test]
    public void SupplyCratePrefab_HasOneShotPropAnimatorWiredCorrectly()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var animator = prefab.GetComponent<OneShotPropAnimator>();

        Assert.IsNotNull(animator, "SupplyCrate prefab must carry a OneShotPropAnimator");
        Assert.AreEqual("SupplyCrate/SupplyCrate.glb", animator.GlbUrl);
        Assert.AreEqual("Open", animator.ClipName);
    }

    [Test]
    public void SupplyCrateGlb_ShippedInStreamingAssets()
    {
        var fullPath = Path.Combine(Application.dataPath, "StreamingAssets", "SupplyCrate", "SupplyCrate.glb");
        Assert.IsTrue(File.Exists(fullPath), $"Expected the crate GLB at {fullPath}");
    }
}
