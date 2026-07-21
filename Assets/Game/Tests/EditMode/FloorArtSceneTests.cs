using System.Linq;
using NUnit.Framework;
using OneTimeGames.CoreSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Verifies the layered floor-art system (factory standard: tiled floor < patches < decals <
// characters) plus the north-wall cave entrance are wired into the committed Main.unity:
//  - moss ground-decal sprites + a GroundDecalScatter covering the floor,
//  - a MossyEarthPatch GroundPatch (9-slice, Tiled draw mode, border auto-set on import),
//  - a second scatter confined to the patch's region,
//  - the CaveEntrance structure sprite on the north wall.
// (Replaces FloorDetailOverlayTests -- the single MossAndCrackOverlay quad was retired in favor
// of engine-scattered decals.)
public class FloorArtSceneTests
{
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";
    private const string PatchPath = "Assets/Game/Sprites/MossyEarthPatch/MossyEarthPatch.png";
    private const string CavePath = "Assets/Game/Sprites/CaveEntranceMouth/CaveEntranceMouth.png";

    private static readonly string[] DecalPaths =
    {
        "Assets/Game/Sprites/MossDecalA/MossDecalA.png",
        "Assets/Game/Sprites/MossDecalB/MossDecalB.png",
        "Assets/Game/Sprites/MossDecalC/MossDecalC.png",
    };

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

    private GameObject FindRoot(string name)
    {
        return _scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
    }

    // ---------- assets ----------

    [Test]
    public void MossDecalSprites_ImportedAsSprites()
    {
        foreach (var path in DecalPaths)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Assert.IsNotNull(sprite, path + " must be imported as a Sprite.");
        }
    }

    [Test]
    public void MossyEarthPatch_ImporterSetNineSliceBorder()
    {
        // The CoreSystems NineSlicePatchImporter must have set the 9-slice border from the
        // sidecar automatically: max(8, round(0.25 * 512)) = 128 on all four sides, FullRect mesh.
        var importer = (TextureImporter)AssetImporter.GetAtPath(PatchPath);
        Assert.IsNotNull(importer);
        Assert.AreEqual(new Vector4(128, 128, 128, 128), importer.spriteBorder);
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        Assert.AreEqual(SpriteMeshType.FullRect, settings.spriteMeshType,
            "Tiled draw mode requires Full Rect sprite meshes.");
        Assert.AreEqual(128, GroundPatch.ComputeBorderPx(0.25f, 512, 512));
    }

    [Test]
    public void CaveSprite_ImportedWithBottomCenterPivot()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CavePath);
        Assert.IsNotNull(sprite, "Cave entrance must be imported as a Sprite.");
        // Bottom-center pivot (structure-sprite convention): pivot.y == 0, pivot.x == width/2.
        Assert.AreEqual(0f, sprite.pivot.y, 0.01f);
        Assert.AreEqual(sprite.rect.width * 0.5f, sprite.pivot.x, 0.01f);
    }

    // ---------- scene: decal scatter ----------

    [Test]
    public void Scene_FloorDecals_ScatterTargetsFloor()
    {
        var go = FindRoot("FloorDecals");
        Assert.IsNotNull(go, "Main.unity must contain a FloorDecals GameObject.");
        var scatter = go.GetComponent<GroundDecalScatter>();
        Assert.IsNotNull(scatter, "FloorDecals must carry a GroundDecalScatter.");

        var floor = FindRoot("Floor");
        Assert.IsNotNull(floor);
        var area = scatter.ScatterArea;
        // Scatter area must cover the 20x20 floor footprint.
        Assert.GreaterOrEqual(area.size.x, 19f);
        Assert.GreaterOrEqual(area.size.z, 19f);
    }

    [Test]
    public void Scene_MossPatchDecals_ConfinedToPatchRegion()
    {
        var go = FindRoot("MossPatchDecals");
        Assert.IsNotNull(go, "Main.unity must contain the patch-confined MossPatchDecals scatter.");
        var scatter = go.GetComponent<GroundDecalScatter>();
        Assert.IsNotNull(scatter);

        var patch = FindRoot("MossPatch");
        Assert.IsNotNull(patch);
        var patchBounds = patch.GetComponent<SpriteRenderer>().bounds;
        var area = scatter.ScatterArea;
        // The scatter's area must be the patch's region, not the whole floor.
        Assert.AreEqual(patchBounds.center.x, area.center.x, 0.1f);
        Assert.AreEqual(patchBounds.center.z, area.center.z, 0.1f);
        Assert.LessOrEqual(area.size.x, 7f);
    }

    [Test]
    public void Scene_HasNoBakedDecalChildren()
    {
        // Decals spawn at runtime (Awake, seeded) -- the committed scene must not contain
        // pre-spawned decal instances.
        foreach (var name in new[] { "FloorDecals", "MossPatchDecals" })
        {
            var go = FindRoot(name);
            Assert.IsNotNull(go);
            Assert.AreEqual(0, go.transform.childCount,
                name + " must not have baked decal children; they spawn at runtime.");
        }
    }

    // ---------- scene: patch ----------

    [Test]
    public void Scene_MossPatch_UsesTiledNineSlice()
    {
        var go = FindRoot("MossPatch");
        Assert.IsNotNull(go, "Main.unity must contain a MossPatch GameObject.");
        Assert.IsNotNull(go.GetComponent<GroundPatch>());

        var r = go.GetComponent<SpriteRenderer>();
        Assert.IsNotNull(r);
        Assert.AreEqual(SpriteDrawMode.Tiled, r.drawMode);
        Assert.AreEqual(new Vector2(6f, 4f), r.size);
        Assert.AreEqual(AssetDatabase.LoadAssetAtPath<Sprite>(PatchPath), r.sprite);
        // Lies flat just above the floor, below characters.
        Assert.Greater(go.transform.position.y, 0f);
        Assert.Less(go.transform.position.y, 0.5f);
    }

    [Test]
    public void Scene_FloorLayerOrder_PatchesBelowDecalsBelowCharacters()
    {
        // Factory floor standard: floor (opaque) < patches (-20) < decals (-10) < characters (0).
        // Ground layers must stay NEGATIVE -- sprite sorting order beats depth in the transparent
        // queue, so a positive order would paint over character sprites at the default order 0.
        var patchOrder = FindRoot("MossPatch").GetComponent<SpriteRenderer>().sortingOrder;
        Assert.AreEqual(-20, patchOrder);
        Assert.Less(patchOrder, 0);
    }

    // ---------- scene: cave entrance ----------

    [Test]
    public void Scene_CaveEntrance_OnNorthWall()
    {
        var go = FindRoot("CaveEntrance");
        Assert.IsNotNull(go, "Main.unity must contain a CaveEntrance GameObject.");
        var r = go.GetComponent<SpriteRenderer>();
        Assert.IsNotNull(r);
        Assert.AreEqual(AssetDatabase.LoadAssetAtPath<Sprite>(CavePath), r.sprite);

        // Sits just in front of the north wall (slab at z=10, camera-facing face z=9.8),
        // facing the camera (identity rotation -- a sprite quad faces -Z), base on the floor.
        Assert.Greater(go.transform.position.z, 9f);
        Assert.Less(go.transform.position.z, 10f);
        Assert.AreEqual(Quaternion.identity, go.transform.rotation);
        Assert.AreEqual(0f, go.transform.position.y, 0.01f);
        // Fits on the 20-wide x 6-tall wall.
        Assert.LessOrEqual(r.bounds.size.x, 20f);
        Assert.LessOrEqual(r.bounds.size.y, 6.05f);
        Assert.Greater(r.bounds.size.x, 4f, "Cave entrance should be a substantial opening, not a tiny decal.");
    }

    // ---------- retirement ----------

    [Test]
    public void Scene_OldFloorDetailOverlay_IsGone()
    {
        Assert.IsNull(FindRoot("FloorDetailOverlay"),
            "The single-quad MossAndCrackOverlay approach was replaced by scattered ground decals.");
    }
}
