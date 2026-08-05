using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using OneTimeGames.CoreSystems;
using Game.Environment;

public class CoinSpillVfxEditTests
{
    private const string PrefabPath = "Assets/Game/Vfx/CoinSpill.prefab";
    private const string CratePrefabPath = "Assets/Game/Prefabs/SupplyCratePrefab.prefab";
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";

    [Test]
    public void CoinSpillPrefab_Exists()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab, $"Expected a prefab asset at {PrefabPath}");
    }

    [Test]
    public void CoinSpillPrefab_HasVfxRecipeConfiguredToAutoDestroy()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var recipe = prefab.GetComponent<VfxRecipe>();
        Assert.IsNotNull(recipe, "CoinSpill prefab must carry a VfxRecipe.");

        var so = new SerializedObject(recipe);
        Assert.IsTrue(so.FindProperty("autoDestroyOnFinish").boolValue,
            "Coin spill is a one-shot effect and must self-destroy once finished (no lingering GameObject).");
    }

    [Test]
    public void CoinSpillPrefab_HasParticleSystemWithGravityAndWorldCollision()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var ps = prefab.GetComponentInChildren<ParticleSystem>(true);
        Assert.IsNotNull(ps, "CoinSpill prefab must carry a child ParticleSystem.");

        var main = ps.main;
        Assert.IsFalse(main.loop, "Coin spill must be a one-shot burst, not a looping effect.");
        Assert.Greater(main.gravityModifier.constant, 0f, "Coins must fall under real gravity.");

        var collision = ps.collision;
        Assert.IsTrue(collision.enabled, "Coin collision must be enabled so coins bounce/settle on the floor.");
        Assert.AreEqual(ParticleSystemCollisionType.World, collision.type,
            "Coin spill must use World collision against the floor's collider, not Planes.");
    }

    [Test]
    public void CoinSpillPrefab_EmitsAsABurst_NotContinuousStream()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var ps = prefab.GetComponentInChildren<ParticleSystem>(true);
        var emission = ps.emission;

        Assert.AreEqual(0f, emission.rateOverTime.constant,
            "Coins must spawn as a burst, not a continuous stream.");
        Assert.Greater(emission.burstCount, 0, "Coin spill must have at least one burst.");
    }

    [Test]
    public void SupplyCratePrefab_HasCoinSpillWired()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CratePrefabPath);
        var interactable = prefab.GetComponent<SupplyCrateInteractable>();
        Assert.IsNotNull(interactable, "SupplyCrate prefab must carry a SupplyCrateInteractable.");

        var so = new SerializedObject(interactable);
        Assert.IsNotNull(so.FindProperty("coinSpillPrefab").objectReferenceValue,
            "SupplyCrate must have its coin-spill VFX prefab assigned.");
        Assert.IsNotNull(so.FindProperty("coinSpawnPoint").objectReferenceValue,
            "SupplyCrate must have a coin spawn point assigned.");
    }

    [Test]
    public void MainScene_FloorHasNonTriggerCollider_ForParticleCollision()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        try
        {
            var floor = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Floor");
            Assert.IsNotNull(floor, "Scene must contain a Floor GameObject.");

            var collider = floor.GetComponent<Collider>();
            Assert.IsNotNull(collider, "Floor must have a Collider for coin-spill particles to collide with.");
            Assert.IsFalse(collider.isTrigger, "Floor's collider must be a real (non-trigger) collider.");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}
