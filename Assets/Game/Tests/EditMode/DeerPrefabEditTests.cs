using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using OneTimeGames.CoreSystems;

public class DeerPrefabEditTests
{
    private const string PrefabPath = "Assets/Game/Prefabs/Deer.prefab";

    [Test]
    public void DeerPrefab_Exists()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab, $"Expected a prefab asset at {PrefabPath}");
    }

    [Test]
    public void DeerPrefab_HasGlbCharacterAnimatorWiredCorrectly()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var animator = prefab.GetComponent<GlbCharacterAnimator>();

        Assert.IsNotNull(animator, "Deer prefab must carry a GlbCharacterAnimator");
        Assert.AreEqual("Wildlife/DeerWildlife.glb", animator.GlbUrl);
        Assert.AreEqual("Idle", animator.IdleClip);
        Assert.AreEqual("Walk", animator.MoveClip);
    }

    [Test]
    public void DeerWildlifeGlb_ShippedInStreamingAssets()
    {
        var fullPath = Path.Combine(Application.dataPath, "StreamingAssets", "Wildlife", "DeerWildlife.glb");
        Assert.IsTrue(File.Exists(fullPath), $"Expected the deer GLB at {fullPath}");
    }
}
