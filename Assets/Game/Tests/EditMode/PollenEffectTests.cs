using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// QA fix (#59), Root Cause 1: no drifting pollen effect existed anywhere in the project. Mirrors
// ElectricalSparkTests.cs's asset-level coverage for the new PollenEffectPrefab.
public class PollenEffectTests
{
    private const string MaterialPath = "Assets/Game/Materials/PollenMaterial.mat";
    private const string PrefabPath = "Assets/Game/Prefabs/PollenEffectPrefab.prefab";
    private const string TexturePath = "Assets/Game/Sprites/PollenSpore/PollenSpore.png";

    [Test]
    public void PollenMaterial_Exists()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.IsNotNull(material);
    }

    [Test]
    public void PollenSporeTexture_IsImported()
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        Assert.IsNotNull(texture, $"Expected the generated pollen spore texture at {TexturePath}.");
    }

    [Test]
    public void PollenMaterial_BaseMapIsSporeTexture()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        Assert.IsNotNull(texture, $"Expected the generated pollen spore texture at {TexturePath}.");
        Assert.AreEqual(texture, material.GetTexture("_BaseMap"),
            "PollenMaterial's _BaseMap must reference the generated spore texture -- not render as a flat tinted quad.");
    }

    [Test]
    public void PollenMaterial_UsesUrpParticleUnlitShader()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual("Universal Render Pipeline/Particles/Unlit", material.shader.name);
    }

    [Test]
    public void PollenMaterial_RendersInTransparentQueue()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual((int)UnityEngine.Rendering.RenderQueue.Transparent, material.renderQueue);
    }

    [Test]
    public void PollenEffectPrefab_Exists()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab);
    }

    [Test]
    public void PollenEffectPrefab_HasLoopingWorldSpaceParticleSystem()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var ps = prefab.GetComponent<ParticleSystem>();
        Assert.IsNotNull(ps, "PollenEffectPrefab must have a ParticleSystem.");
        Assert.IsTrue(ps.main.loop, "Pollen particle system must loop continuously for an ambient effect.");
        Assert.IsTrue(ps.main.playOnAwake, "Pollen particle system must start automatically.");
        Assert.AreEqual(ParticleSystemSimulationSpace.World, ps.main.simulationSpace);
    }

    [Test]
    public void PollenEffectPrefab_RendersAsBillboardWithPollenMaterial()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var renderer = prefab.GetComponent<ParticleSystemRenderer>();
        Assert.IsNotNull(renderer);
        Assert.AreEqual(ParticleSystemRenderMode.Billboard, renderer.renderMode);
        Assert.AreEqual(material, renderer.sharedMaterial);
    }
}
