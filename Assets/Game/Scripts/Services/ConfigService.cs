using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigService : MonoBehaviour
{
    public static ConfigService Instance { get; private set; }

    private readonly Dictionary<string, string> _values = new();
    private bool _loaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("[ConfigService]");
        go.AddComponent<ConfigService>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator EnsureLoaded()
    {
        if (_loaded) yield break;
        // In WebGL, relative URLs resolve against the player context, not the page URL. Use absoluteURL.
        var pageUrl = Application.absoluteURL;
        var baseUrl = string.IsNullOrEmpty(pageUrl) ? "" : pageUrl.Substring(0, pageUrl.LastIndexOf('/') + 1);
        using var req = UnityWebRequest.Get(baseUrl + "config.json");
        req.timeout = 3;
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            var text = req.downloadHandler.text.Trim('{', '}', ' ', '\n');
            foreach (var pair in text.Split(','))
            {
                var kv = pair.Split(new char[] { ':' }, 2);
                // Trim() strips ALL surrounding whitespace (incl. the newlines/indentation
                // of pretty-printed multi-line config.json) BEFORE stripping quotes. Without
                // the leading Trim(), every key except the first keeps a leading "\n  " and
                // never matches a Get() lookup -- silently dropping every config value but one.
                if (kv.Length == 2) _values[kv[0].Trim().Trim('"')] = kv[1].Trim().Trim('"');
            }
        }
        _loaded = true;
    }

    public string Get(string key, string fallback = "") =>
        _values.TryGetValue(key, out var v) ? v : fallback;
}
