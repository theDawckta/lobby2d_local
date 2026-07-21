using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Verifies the base floor tile (OvergrownFacilityFloorPanel, issue #15) is imported and actually
// applied to the Floor geometry in Main.unity (issue #16). This guards the false-positive-"done"
// gap that let #16 close while the generated tile stayed orphaned in the Design staging area and
// the Floor kept an untextured placeholder material -- mirrors WallTextureTests.
public class FloorTextureTests
{
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";
    private const string TexturePath = "Assets/Game/Sprites/OvergrownFacilityFloorPanel/OvergrownFacilityFloorPanel.png";
    private const string MaterialPath = "Assets/Game/Materials/FloorPlaceholder.mat";

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

    private GameObject FindFloor()
    {
        return _scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Floor");
    }

    [Test]
    public void FloorTexture_IsImported_AsRepeatingTexture2D()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        Assert.IsNotNull(importer, $"Expected a texture importer at {TexturePath}.");
        Assert.AreEqual(TextureImporterType.Default, importer.textureType,
            "Floor texture should import as a plain Texture2D (material base map), not a Sprite.");
        Assert.AreEqual(TextureWrapMode.Repeat, importer.wrapMode,
            "Floor texture wrap mode must be Repeat so it tiles seamlessly across the floor.");
    }

    [Test]
    public void FloorMaterial_UsesUrpLitShader_WithFloorTextureAsBaseMap()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);

        Assert.IsNotNull(material, $"Expected a material at {MaterialPath}.");
        Assert.IsNotNull(texture, $"Expected a texture at {TexturePath}.");
        Assert.AreEqual("Universal Render Pipeline/Lit", material.shader.name,
            "Floor material must use a URP-compatible shader.");
        Assert.AreEqual(texture, material.GetTexture("_BaseMap"),
            "Floor material's _BaseMap must reference the generated floor tile -- not a flat placeholder color.");
    }

    [Test]
    public void FloorMaterial_TilesTheTexture_AcrossTheFloorSurface()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var scale = material.GetTextureScale("_BaseMap");

        Assert.Greater(scale.x, 1f, "Base map tiling scale.x must exceed 1 so the tile repeats instead of stretching across the floor.");
        Assert.Greater(scale.y, 1f, "Base map tiling scale.y must exceed 1 so the tile repeats instead of stretching across the floor.");
    }

    [Test]
    public void Floor_UsesTheTexturedFloorMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var floor = FindFloor();
        Assert.IsNotNull(floor, "Main.unity must contain the Floor GameObject.");
        Assert.AreEqual(material, floor.GetComponent<MeshRenderer>().sharedMaterial,
            "The Floor must use the textured floor material.");
    }

    [Test]
    public void Floor_CoversTheWalkableArea()
    {
        var floor = FindFloor();
        // The lobby room walls sit at x/z = +/-10 and wildlife wander within +/-10, so the floor
        // must span at least 20 world units on each axis or characters walk off it into the void.
        // Measure the actual rendered world footprint (mesh-size agnostic) rather than assuming a
        // particular built-in mesh -- the previous placeholder was a tiny patch that a hardcoded
        // "10 * scale" check would have wrongly passed.
        var size = floor.GetComponent<MeshRenderer>().bounds.size;
        Assert.GreaterOrEqual(size.x, 20f - 0.01f, "Floor must span at least the 20-unit room width so characters have ground to stand on.");
        Assert.GreaterOrEqual(size.z, 20f - 0.01f, "Floor must span at least the 20-unit room depth so characters have ground to stand on.");
    }
}
