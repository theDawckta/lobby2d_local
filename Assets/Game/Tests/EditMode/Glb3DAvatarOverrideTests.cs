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
}
