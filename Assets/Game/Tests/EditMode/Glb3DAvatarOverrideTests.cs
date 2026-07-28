using NUnit.Framework;
using Game.Player;

public class Glb3DAvatarOverrideTests
{
    [Test]
    public void BuildGlbUrl_MatchesCharactersStaticModel3dConvention()
    {
        var url = Glb3DAvatarOverride.BuildGlbUrl("https://factory.tehfaktoree.com", "zeke");
        Assert.AreEqual("https://factory.tehfaktoree.com/characters-static/zeke/model3d/zeke-rigged-textured.glb", url);
    }

    [Test]
    public void BuildGlbUrl_TrimsTrailingSlash_OnBaseUrl()
    {
        var url = Glb3DAvatarOverride.BuildGlbUrl("https://factory.tehfaktoree.com/", "zeke");
        Assert.AreEqual("https://factory.tehfaktoree.com/characters-static/zeke/model3d/zeke-rigged-textured.glb", url);
    }

    [Test]
    public void ComputeAvatarLocalScale_MatchesTargetHeight_ForATwoUnitTallMesh()
    {
        // lobby2d_local's real calibration: avatarScale=5 (a 2D sprite's target height, since sprites
        // are exactly 1 unit tall natively) against zeke's GLB, which is 2.0 units tall bind-pose
        // (confirmed in Blender) -- the GLB avatar must reach the SAME 5-unit target height, not 10.
        var scale = Glb3DAvatarOverride.ComputeAvatarLocalScale(5f, 2f);
        Assert.AreEqual(2.5f, scale, 0.0001f);
    }

    [Test]
    public void ComputeAvatarLocalScale_OneUnitTallMesh_MatchesAvatarScaleDirectly()
    {
        var scale = Glb3DAvatarOverride.ComputeAvatarLocalScale(5f, 1f);
        Assert.AreEqual(5f, scale, 0.0001f);
    }

    [Test]
    public void ComputeAvatarLocalScale_ZeroOrNegativeNativeHeight_FallsBackToAvatarScale()
    {
        Assert.AreEqual(5f, Glb3DAvatarOverride.ComputeAvatarLocalScale(5f, 0f), 0.0001f);
        Assert.AreEqual(5f, Glb3DAvatarOverride.ComputeAvatarLocalScale(5f, -1f), 0.0001f);
    }
}
