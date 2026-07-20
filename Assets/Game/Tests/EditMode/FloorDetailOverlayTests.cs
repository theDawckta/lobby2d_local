using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Verifies the Floor Detail Overlay (issue #18) is imported and wired into the real committed
// Main.unity scene: layered above the tiled floor, below characters/props, with the floor
// showing through its alpha.
public class FloorDetailOverlayTests
{
    private const string TexturePath = "Assets/Game/Sprites/MossAndCrackOverlay/MossAndCrackOverlay.png";
    private const string SidecarPath = "Assets/Game/Sprites/MossAndCrackOverlay/MossAndCrackOverlay-sprites.json";
    private const string MaterialPath = "Assets/Game/Materials/FloorDetailOverlayMaterial.mat";
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";

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

    private GameObject FindOverlay()
    {
        return _scene.GetRootGameObjects().FirstOrDefault(g => g.name == "FloorDetailOverlay");
    }

    private GameObject FindFloor()
    {
        return _scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Floor");
    }

    [Test]
    public void OverlayTexture_ExistsInSpritesFolder()
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        Assert.IsNotNull(texture);
        Assert.AreEqual(2048, texture.width);
        Assert.AreEqual(2048, texture.height);
    }

    [Test]
    public void OverlaySidecar_ExistsInSpritesFolder()
    {
        var sidecar = AssetDatabase.LoadAssetAtPath<TextAsset>(SidecarPath);
        Assert.IsNotNull(sidecar);
        Assert.IsTrue(sidecar.text.Contains("\"transparent\": true"));
    }

    [Test]
    public void OverlayMaterial_Exists()
    {
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Material>(MaterialPath));
    }

    [Test]
    public void OverlayMaterial_UsesUrpLitShader()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual("Universal Render Pipeline/Lit", material.shader.name);
    }

    [Test]
    public void OverlayMaterial_IsTransparentSurface()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual(1f, material.GetFloat("_Surface"), "Material must use Surface Type = Transparent so the floor shows through its alpha.");
        Assert.AreEqual(0f, material.GetFloat("_ZWrite"), "Transparent overlay must not write depth.");
        Assert.AreEqual("Transparent", material.GetTag("RenderType", false));
    }

    [Test]
    public void OverlayMaterial_BaseMapIsOverlayTexture()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        Assert.AreEqual(texture, material.GetTexture("_BaseMap"));
    }

    [Test]
    public void Scene_ContainsFloorDetailOverlayGameObject()
    {
        var overlay = FindOverlay();
        Assert.IsNotNull(overlay, "Main.unity must contain a FloorDetailOverlay GameObject.");
        Assert.IsNotNull(overlay.GetComponent<MeshRenderer>());
        Assert.IsNotNull(overlay.GetComponent<MeshFilter>());
    }

    [Test]
    public void OverlayGameObject_UsesOverlayMaterial()
    {
        var overlay = FindOverlay();
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual(material, overlay.GetComponent<MeshRenderer>().sharedMaterial);
    }

    [Test]
    public void OverlayGameObject_MatchesFloorFootprint()
    {
        var overlay = FindOverlay();
        var floor = FindFloor();
        Assert.IsNotNull(floor, "Scene must still contain the Floor GameObject.");

        Assert.AreEqual(floor.GetComponent<MeshFilter>().sharedMesh, overlay.GetComponent<MeshFilter>().sharedMesh,
            "Overlay should reuse the floor's mesh so it covers exactly the same footprint.");
        Assert.AreEqual(floor.transform.localScale, overlay.transform.localScale,
            "Overlay must match the floor's scale so it fully covers the floor area.");
        Assert.AreEqual(floor.transform.position.x, overlay.transform.position.x, 0.001f);
        Assert.AreEqual(floor.transform.position.z, overlay.transform.position.z, 0.001f);
    }

    [Test]
    public void OverlayGameObject_IsAboveFloorAndBelowCharacterHeight()
    {
        var overlay = FindOverlay();
        var floor = FindFloor();

        Assert.Greater(overlay.transform.position.y, floor.transform.position.y,
            "Overlay must render above the floor.");
        Assert.Less(overlay.transform.position.y, 0.5f,
            "Overlay must stay a thin ground decal so it does not occlude character sprites standing on the floor.");
    }

    [Test]
    public void OverlayGameObject_DoesNotCastOrReceiveShadows()
    {
        var overlay = FindOverlay();
        var renderer = overlay.GetComponent<MeshRenderer>();
        Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, renderer.shadowCastingMode);
        Assert.IsFalse(renderer.receiveShadows);
    }

    [Test]
    public void OverlayGameObject_HasNoCollider()
    {
        var overlay = FindOverlay();
        Assert.IsNull(overlay.GetComponent<Collider>(), "A flat ground decal must not block player movement or other physics.");
    }
}
