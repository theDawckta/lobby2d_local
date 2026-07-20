using NUnit.Framework;
using OneTimeGames.CoreSystems;
using UnityEditor;
using UnityEngine;

public class SupplyCratePrefabTests
{
    private const string PrefabPath = "Assets/Game/Prefabs/SupplyCratePrefab.prefab";
    private const string GlbPath = "Assets/StreamingAssets/SupplyCrate/SupplyCrate.glb";

    [Test]
    public void SupplyCratePrefab_ExistsInPrefabsFolder()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab);
    }

    [Test]
    public void SupplyCrateGlb_ExistsInStreamingAssets()
    {
        var asset = AssetDatabase.LoadAssetAtPath<Object>(GlbPath);
        Assert.IsNotNull(asset);
    }

    [Test]
    public void SupplyCratePrefab_HasOneShotPropAnimatorWiredCorrectly()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var animator = prefab.GetComponent<OneShotPropAnimator>();

        Assert.IsNotNull(animator);
        Assert.AreEqual("SupplyCrate/SupplyCrate.glb", animator.GlbUrl);
        Assert.AreEqual("Open", animator.ClipName);
    }
}
