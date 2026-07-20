using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using OneTimeGames.CoreSystems;
using Game.Audio;
using Game.Auth;
using Game.Chat;
using Game.Emotes;
using Game.Player;
using Game.UI;
using Game.Wildlife;
using Game.World;

// Verifies the assembled Main.unity scene (issue #37) actually instantiates and wires the
// systems the issue lists. Opens the real committed scene file rather than building one in
// memory, so a regression to the scene asset itself (not just the assembly script) is caught.
public class MainSceneAssemblyTests
{
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

    private T FindInScene<T>() where T : Component
    {
        return _scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<T>(true))
            .FirstOrDefault();
    }

    [Test]
    public void MainScene_IsRegisteredInBuildSettings()
    {
        Assert.IsTrue(EditorBuildSettings.scenes.Any(s => s.path == ScenePath && s.enabled),
            "Main.unity must be an enabled scene in Build Settings.");
    }

    [Test]
    public void MainScene_HasExactlyOneCamera_ClearingTheScreen()
    {
        var cameras = _scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<Camera>(true))
            .ToList();

        Assert.AreEqual(1, cameras.Count, "Scene should have exactly one camera (idempotent assembly).");
        Assert.AreEqual(CameraClearFlags.SolidColor, cameras[0].clearFlags,
            "Camera must clear the screen (SolidColor clear flags).");
    }

    [Test]
    public void MainScene_HasFloorAndFourWalls()
    {
        var floor = _scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Floor");
        Assert.IsNotNull(floor, "Scene must contain a Floor GameObject.");
        Assert.IsNotNull(floor.GetComponent<MeshRenderer>(), "Floor must have a MeshRenderer.");

        var walls = _scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Walls");
        Assert.IsNotNull(walls, "Scene must contain a Walls parent GameObject.");
        var wallRenderers = walls.GetComponentsInChildren<MeshRenderer>(true);
        Assert.AreEqual(4, wallRenderers.Length, "Scene must contain 4 wall planes enclosing the floor.");
    }

    [Test]
    public void MainScene_HasBackgroundAndSparkEffect()
    {
        Assert.IsNotNull(FindInScene<Game.Environment.FullScreenBackground>(),
            "Scene must contain the Background prefab (FullScreenBackground).");

        var spark = _scene.GetRootGameObjects().FirstOrDefault(g => g.name == "ElectricalSparkPrefab");
        Assert.IsNotNull(spark, "Scene must contain the Spark Effect prefab instance.");
    }

    [Test]
    public void MainScene_HasWildlifeManager_WithDeerAndBirdOnly_NoRabbit()
    {
        var manager = FindInScene<WildlifeManager>();
        Assert.IsNotNull(manager, "Scene must contain a WildlifeManager.");

        var prefabNames = manager.SpawnEntries.Where(e => e?.prefab != null).Select(e => e.prefab.name).ToList();
        CollectionAssert.Contains(prefabNames, "Deer", "WildlifeManager must spawn Deer.");
        CollectionAssert.Contains(prefabNames, "Bird", "WildlifeManager must spawn Bird.");
        // Rabbit is intentionally absent -- see the issue #37 closeout comment: #29/#30's
        // generated asset and "Apply Rabbit" commit never made it into this repo.
        Assert.IsFalse(prefabNames.Any(n => n.ToLower().Contains("rabbit")),
            "No Rabbit prefab exists to spawn -- it must not be referenced.");
    }

    [Test]
    public void MainScene_WildlifeAgents_AreAmbientAndNonReactive()
    {
        var deerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Deer.prefab");
        var birdPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Bird.prefab");

        foreach (var prefab in new[] { deerPrefab, birdPrefab })
        {
            var agent = prefab.GetComponent<WildlifeAgent>();
            Assert.IsNotNull(agent, $"{prefab.name} must carry a WildlifeAgent.");
            var so = new SerializedObject(agent);
            Assert.AreEqual(0f, so.FindProperty("fleeDistance").floatValue,
                $"{prefab.name}'s WildlifeAgent must have fleeDistance = 0 (ambient, non-reactive).");
        }
    }

    [Test]
    public void MainScene_HasSupplyCrate_NetworkedAndInteractable()
    {
        var crate = _scene.GetRootGameObjects().FirstOrDefault(g => g.name == "SupplyCratePrefab");
        Assert.IsNotNull(crate, "Scene must contain the SupplyCrate prefab instance.");
        Assert.IsNotNull(crate.GetComponent<OneShotPropAnimator>(), "SupplyCrate must have OneShotPropAnimator.");
        Assert.IsNotNull(crate.GetComponent<Game.Environment.SupplyCrateInteractable>(),
            "SupplyCrate must have SupplyCrateInteractable to open once on trigger.");

        var collider = crate.GetComponent<Collider>();
        Assert.IsNotNull(collider, "SupplyCrate must have a Collider for trigger detection.");
        Assert.IsTrue(collider.isTrigger, "SupplyCrate's collider must be a trigger.");
    }

    [Test]
    public void MainScene_HasLocalPlayer_WithMovementAndPresence()
    {
        Assert.IsNotNull(FindInScene<LocalPlayerController>(), "Scene must contain a LocalPlayerController.");
        Assert.IsNotNull(FindInScene<WorldPresenceController>(), "Scene must contain a WorldPresenceController.");
        Assert.IsNotNull(FindInScene<SpeechChatIntegrationController>(),
            "Scene must contain a SpeechChatIntegrationController.");
        Assert.IsNotNull(FindInScene<ChatBubble>(), "Local player must have a ChatBubble for its own echo.");

        var player = _scene.GetRootGameObjects().FirstOrDefault(g => g.name == "LocalPlayer");
        Assert.IsNotNull(player, "LocalPlayer GameObject must exist.");
        Assert.AreEqual("Player", player.tag, "LocalPlayer must be tagged 'Player' for the crate trigger.");
        Assert.IsNotNull(player.GetComponent<Collider>(), "LocalPlayer must have a Collider for trigger detection.");
    }

    [Test]
    public void MainScene_HasWorldAndAuthConnections()
    {
        Assert.IsNotNull(FindInScene<WorldConnectController>(), "Scene must contain a WorldConnectController.");
        Assert.IsNotNull(FindInScene<FactoryAuth>(), "Scene must contain a FactoryAuth.");
        Assert.IsNotNull(FindInScene<AuthConnectController>(), "Scene must contain an AuthConnectController.");
    }

    [Test]
    public void MainScene_HasEmoteSystem()
    {
        Assert.IsNotNull(FindInScene<EmoteSystem>(), "Scene must contain an EmoteSystem.");
    }

    [Test]
    public void MainScene_StartScreenIsWiredToGameScreen()
    {
        var startScreen = FindInScene<StartScreen>();
        var gameScreen = FindInScene<GameScreen>();
        Assert.IsNotNull(startScreen, "Scene must contain a StartScreen.");
        Assert.IsNotNull(gameScreen, "Scene must contain a GameScreen.");
        Assert.AreSame(gameScreen, startScreen.GameScreen,
            "StartScreen's Join Lobby button must be wired to hide StartScreen and show GameScreen.");
    }
}
