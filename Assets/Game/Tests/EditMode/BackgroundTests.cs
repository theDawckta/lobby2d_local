using Game.Environment;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BackgroundTests
{
    private const string TexturePath = "Assets/Game/Sprites/OvergrownFacilityBackground/OvergrownFacilityBackground.png";
    private const string SidecarPath = "Assets/Game/Sprites/OvergrownFacilityBackground/OvergrownFacilityBackground-sprites.json";
    private const string MaterialPath = "Assets/Game/Materials/BackgroundMaterial.mat";
    private const string PrefabPath = "Assets/Game/Prefabs/BackgroundPrefab.prefab";
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";

    [Test]
    public void BackgroundTexture_ExistsInSpritesFolder()
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        Assert.IsNotNull(texture);
    }

    [Test]
    public void BackgroundSidecar_ExistsInSpritesFolder()
    {
        var sidecar = AssetDatabase.LoadAssetAtPath<TextAsset>(SidecarPath);
        Assert.IsNotNull(sidecar);
    }

    [Test]
    public void BackgroundMaterial_Exists()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.IsNotNull(material);
    }

    [Test]
    public void BackgroundMaterial_UsesUrpLitShader()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual("Universal Render Pipeline/Lit", material.shader.name);
    }

    [Test]
    public void BackgroundMaterial_BaseMapIsBackgroundTexture()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        Assert.AreEqual(texture, material.GetTexture("_BaseMap"));
    }

    [Test]
    public void BackgroundPrefab_Exists()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab);
    }

    [Test]
    public void BackgroundPrefab_HasMeshRenderer()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab.GetComponent<MeshRenderer>());
    }

    [Test]
    public void BackgroundPrefab_MeshRendererUsesBackgroundMaterial()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual(material, prefab.GetComponent<MeshRenderer>().sharedMaterial);
    }

    [Test]
    public void BackgroundPrefab_HasFullScreenBackgroundComponent()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab.GetComponent<FullScreenBackground>());
    }

    [Test]
    public void BackgroundPrefab_HasNoMeshCollider()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNull(prefab.GetComponent<MeshCollider>());
    }

    [Test]
    public void MainScene_IsNotModifiedByThisTicket()
    {
        var sceneText = System.IO.File.ReadAllText(ScenePath);
        Assert.IsFalse(sceneText.Contains("BackgroundPrefab"));
        Assert.IsFalse(sceneText.Contains("FullScreenBackground"));
    }
}
