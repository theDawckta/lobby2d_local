using System.Linq;
using NUnit.Framework;
using OneTimeGames.CoreSystems;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class ElectricalSparkTests
{
    private const string TexturePath = "Assets/Game/Sprites/ElectricalSpark/ElectricalSpark.png";
    private const string SidecarPath = "Assets/Game/Sprites/ElectricalSpark/ElectricalSpark-sprites.json";
    private const string MaterialPath = "Assets/Game/Materials/ElectricalSparkMaterial.mat";
    private const string ControllerPath = "Assets/Game/Animations/ElectricalSparkAnimator.controller";
    private const string PrefabPath = "Assets/Game/Prefabs/ElectricalSparkPrefab.prefab";
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";
    private const int ExpectedFrameCount = 13;

    [Test]
    public void ElectricalSparkTexture_ExistsInSpritesFolder()
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        Assert.IsNotNull(texture);
    }

    [Test]
    public void ElectricalSparkSidecar_ExistsInSpritesFolder()
    {
        var sidecar = AssetDatabase.LoadAssetAtPath<TextAsset>(SidecarPath);
        Assert.IsNotNull(sidecar);
    }

    [Test]
    public void ElectricalSparkTexture_SlicedIntoExpectedFrameCount()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(TexturePath).OfType<Sprite>().ToArray();
        Assert.AreEqual(ExpectedFrameCount, sprites.Length);
    }

    [Test]
    public void ElectricalSparkMaterial_Exists()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.IsNotNull(material);
    }

    [Test]
    public void ElectricalSparkMaterial_UsesUrpTransparentSpriteShader()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual("Universal Render Pipeline/2D/Sprite-Unlit-Default", material.shader.name);
    }

    [Test]
    public void ElectricalSparkMaterial_RendersInTransparentQueue()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.AreEqual((int)UnityEngine.Rendering.RenderQueue.Transparent, material.renderQueue);
    }

    [Test]
    public void ElectricalSparkController_Exists()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        Assert.IsNotNull(controller);
    }

    [Test]
    public void ElectricalSparkController_DefaultStateUsesLoopingClip()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        var defaultState = controller.layers[0].stateMachine.defaultState;
        Assert.IsNotNull(defaultState);

        var clip = defaultState.motion as AnimationClip;
        Assert.IsNotNull(clip);
        Assert.IsTrue(AnimationUtility.GetAnimationClipSettings(clip).loopTime);
    }

    [Test]
    public void ElectricalSparkController_ClipHasNPlusOneKeyframesForSeamlessLoop()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        var clip = controller.layers[0].stateMachine.defaultState.motion as AnimationClip;

        var binding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
        var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);

        Assert.AreEqual(ExpectedFrameCount + 1, keyframes.Length);
        Assert.AreEqual(keyframes[0].value, keyframes[ExpectedFrameCount].value);
    }

    [Test]
    public void ElectricalSparkPrefab_Exists()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab);
    }

    [Test]
    public void ElectricalSparkPrefab_HasSpriteRendererWithTransparentMaterial()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        var sr = prefab.GetComponent<SpriteRenderer>();
        Assert.IsNotNull(sr);
        Assert.AreEqual(material, sr.sharedMaterial);
        Assert.IsNotNull(sr.sprite);
    }

    [Test]
    public void ElectricalSparkPrefab_HasAnimatorWithSparkController()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        var animator = prefab.GetComponent<Animator>();
        Assert.IsNotNull(animator);
        Assert.AreEqual(controller, animator.runtimeAnimatorController);
    }

    [Test]
    public void ElectricalSparkPrefab_HasBillboardSprite()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.IsNotNull(prefab.GetComponent<BillboardSprite>());
    }

    [Test]
    public void MainScene_IsNotModifiedByThisTicket()
    {
        var sceneText = System.IO.File.ReadAllText(ScenePath);
        Assert.IsFalse(sceneText.Contains("ElectricalSparkPrefab"));
    }
}
