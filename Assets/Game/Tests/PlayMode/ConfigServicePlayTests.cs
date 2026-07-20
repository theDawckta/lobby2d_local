using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ConfigServicePlayTests
{
    [UnityTest]
    public IEnumerator Instance_IsAutoCreatedBeforeAnyTestRuns()
    {
        yield return null;
        Assert.IsNotNull(ConfigService.Instance);
    }

    [UnityTest]
    public IEnumerator Get_ReturnsGivenFallback_WhenKeyNotLoaded()
    {
        yield return null;
        Assert.AreEqual("fallback-value", ConfigService.Instance.Get("definitely_not_a_real_config_key", "fallback-value"));
    }

    [UnityTest]
    public IEnumerator Get_ReturnsEmptyString_WhenKeyMissingAndNoFallbackGiven()
    {
        yield return null;
        Assert.AreEqual(string.Empty, ConfigService.Instance.Get("definitely_not_a_real_config_key"));
    }

    [UnityTest]
    public IEnumerator EnsureLoaded_CompletesWithoutThrowing_WhenNoConfigJsonIsServed()
    {
        // No config.json is served in this test environment (no WebGL host, no absoluteURL) --
        // EnsureLoaded must complete cleanly rather than throwing or hanging, leaving Get() to
        // fall back as normal.
        yield return ConfigService.Instance.EnsureLoaded();
        Assert.AreEqual("fallback-value", ConfigService.Instance.Get("charactersBaseUrl", "fallback-value"));
    }

    [UnityTest]
    public IEnumerator AddingASecondComponent_DestroysTheDuplicateGameObject()
    {
        var duplicateGo = new GameObject("DuplicateConfigService");
        duplicateGo.AddComponent<ConfigService>();
        yield return null;

        Assert.IsTrue(duplicateGo == null);
    }
}
