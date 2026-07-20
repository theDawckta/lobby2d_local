using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Verifies the wall texture (issue #21's DerelictWallMetal asset) is imported and applied to
// the wall geometry from Main.unity (issue #37), per issue #22.
public class WallTextureTests
{
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";
    private const string TexturePath = "Assets/Game/Sprites/DerelictWallMetal/DerelictWallMetal.png";
    private const string MaterialPath = "Assets/Game/Materials/WallPlaceholder.mat";

    private Scene _scene;

    [OneTimeSetUp]
    public void OpenScene()
    {
        _scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
    }

    [OneTimeTearDown]
    public void CloseScene()
    {
        if (_scene.IsValid()) EditorSceneManager.CloseScene(_scene, true);
    }

    [Test]
    public void WallTexture_IsImported_AsRepeatingTexture2D()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        Assert.IsNotNull(importer, $"Expected a texture importer at {TexturePath}.");
        Assert.AreEqual(TextureImporterType.Default, importer.textureType,
            "Wall texture should import as a plain Texture2D (material base map), not a Sprite.");
        Assert.AreEqual(TextureWrapMode.Repeat, importer.wrapMode,
            "Wall texture wrap mode must be Repeat so it tiles across the wall surfaces.");
    }

    [Test]
    public void WallMaterial_UsesUrpLitShader_WithWallTextureAsBaseMap()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);

        Assert.IsNotNull(material, $"Expected a material at {MaterialPath}.");
        Assert.IsNotNull(texture, $"Expected a texture at {TexturePath}.");
        Assert.AreEqual("Universal Render Pipeline/Lit", material.shader.name,
            "Wall material must use a URP-compatible shader.");
        Assert.AreEqual(texture, material.GetTexture("_BaseMap"),
            "Wall material's _BaseMap must reference the wall texture.");
    }

    [Test]
    public void WallMaterial_TilesTheTexture_AcrossTheWallSurface()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var scale = material.GetTextureScale("_BaseMap");

        Assert.Greater(scale.x, 1f, "Base map tiling scale.x must exceed 1 so the texture repeats instead of stretching across the wall.");
        Assert.Greater(scale.y, 1f, "Base map tiling scale.y must exceed 1 so the texture repeats instead of stretching across the wall.");
    }

    [Test]
    public void AllFourWalls_UseTheTexturedWallMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var wallNames = new[] { "Wall_North", "Wall_South", "Wall_East", "Wall_West" };

        var renderers = _scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<MeshRenderer>(true))
            .Where(r => wallNames.Contains(r.gameObject.name))
            .ToList();

        Assert.AreEqual(4, renderers.Count, "Expected to find all four wall renderers (Wall_North/South/East/West) in Main.unity.");
        foreach (var renderer in renderers)
        {
            Assert.AreEqual(material, renderer.sharedMaterial,
                $"{renderer.gameObject.name} must use the textured WallPlaceholder material.");
        }
    }
}
