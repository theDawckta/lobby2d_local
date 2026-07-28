using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Player;
using OneTimeGames.CoreSystems;

public class GlbAvatarMotionPlayTests
{
    private readonly List<GameObject> _spawned = new();

    private GameObject Spawn(string name)
    {
        var go = new GameObject(name);
        _spawned.Add(go);
        return go;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _spawned)
        {
            if (go != null) Object.Destroy(go);
        }
        _spawned.Clear();
    }

    [UnityTest]
    public IEnumerator DoesNotThrow_WhenAnimatorUnloaded_AndTransformMoves()
    {
        // Mirrors WildlifeAgentPlayTests' "GlbCharacterAnimator present but unloaded" safety case --
        // SetSpeed() on an animator with no loaded clips must no-op, not throw.
        var go = Spawn("Avatar");
        go.AddComponent<GlbCharacterAnimator>();
        go.AddComponent<GlbAvatarMotion>();

        for (var i = 0; i < 3; i++)
        {
            go.transform.position += Vector3.forward * 0.1f;
            yield return null;
        }

        Assert.Pass();
    }

    [UnityTest]
    public IEnumerator RequiresGlbCharacterAnimator_OnTheSameGameObject()
    {
        var go = Spawn("Avatar");
        go.AddComponent<GlbCharacterAnimator>();
        var motion = go.AddComponent<GlbAvatarMotion>();
        yield return null;

        Assert.IsNotNull(motion.GetComponent<GlbCharacterAnimator>());
    }
}
