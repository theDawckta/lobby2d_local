using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Player;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.Presence;

public class LocalPlayerControllerPlayTests
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
    public IEnumerator Move_UpdatesPosition_AlongInputDirection()
    {
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        controller.MoveSpeed = 2f;
        yield return null;

        controller.Move(new Vector2(0f, 1f), 1f);

        Assert.AreEqual(new Vector3(0f, 0f, 2f), go.transform.position);
    }

    [UnityTest]
    public IEnumerator Move_NoInput_DoesNotChangePosition()
    {
        var go = Spawn("LocalPlayer");
        go.transform.position = new Vector3(3f, 0f, 4f);
        var controller = go.AddComponent<LocalPlayerController>();
        yield return null;

        controller.Move(Vector2.zero, 1f);

        Assert.AreEqual(new Vector3(3f, 0f, 4f), go.transform.position);
    }

    [UnityTest]
    public IEnumerator Move_TurnsPartwayTowardMovementDirection_WithinOneFrame()
    {
        // Turning is rate-limited (turnSpeedDegrees), not instant -- a 3D avatar parented under this
        // transform must not snap to face a new direction the moment input changes. Default 540
        // deg/sec * 0.1s = 54-degree budget, short of the 90-degree target.
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        yield return null;

        controller.Move(new Vector2(1f, 0f), 0.1f);

        Assert.AreEqual(54f, go.transform.eulerAngles.y, 0.01f);
        Assert.Less(go.transform.eulerAngles.y, 90f);
    }

    [UnityTest]
    public IEnumerator Move_ReachesMovementDirection_AfterEnoughTime()
    {
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        yield return null;

        for (var i = 0; i < 5; i++) controller.Move(new Vector2(1f, 0f), 0.1f);

        Assert.AreEqual(90f, go.transform.eulerAngles.y, 0.01f);
    }

    [UnityTest]
    public IEnumerator Move_NoInput_HoldsCurrentYaw_OnceConverged()
    {
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        yield return null;

        for (var i = 0; i < 5; i++) controller.Move(new Vector2(1f, 0f), 0.1f); // converges to 90
        controller.Move(Vector2.zero, 0.1f);

        Assert.AreEqual(90f, go.transform.eulerAngles.y, 0.01f);
    }

    [UnityTest]
    public IEnumerator Move_SetsVelocity_FromInputAndMoveSpeed()
    {
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        controller.MoveSpeed = 3f;
        yield return null;

        controller.Move(new Vector2(1f, 0f), 1f);

        Assert.AreEqual(new Vector3(3f, 0f, 0f), controller.Velocity);
    }

    [UnityTest]
    public IEnumerator Move_DoesNotThrow_WhenWorldPresenceNotAssigned()
    {
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        yield return null;

        Assert.IsNull(controller.Presence);
        Assert.DoesNotThrow(() => controller.Move(new Vector2(0f, 1f), 0.1f));
    }

    [UnityTest]
    public IEnumerator Move_DoesNotThrow_WhenWorldPresenceAssignedButDisconnected()
    {
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        var presence = go.AddComponent<WorldPresence>();
        controller.Presence = presence;
        yield return null;

        Assert.DoesNotThrow(() => controller.Move(new Vector2(0f, 1f), 0.1f));
    }

    [UnityTest]
    public IEnumerator Awake_FindsWorldPresenceOnSameGameObject_WhenNotAssigned()
    {
        var go = Spawn("LocalPlayer");
        var presence = go.AddComponent<WorldPresence>();
        var controller = go.AddComponent<LocalPlayerController>();
        yield return null;

        Assert.AreSame(presence, controller.Presence);
    }

    [UnityTest]
    public IEnumerator Awake_FindsVirtualAnalogStickInScene_WhenNotAssigned()
    {
        // No MobileControlsOverlay in this scene, so the stick's own Start() will disable itself
        // once it runs -- but LocalPlayerController resolves it in Awake(), which runs
        // synchronously and immediately (before any Start()), so the stick is still discoverable.
        var stickGo = Spawn("Stick");
        var stick = stickGo.AddComponent<VirtualAnalogStick>();

        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();

        Assert.AreSame(stick, controller.TouchStick);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ReadInput_DoesNotThrow_WhenNoTouchStickExists()
    {
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        yield return null;

        Assert.DoesNotThrow(() => controller.ReadInput());
    }

    [UnityTest]
    public IEnumerator DefaultTurnSpeedDegrees_IsPositive()
    {
        var go = Spawn("LocalPlayer");
        var controller = go.AddComponent<LocalPlayerController>();
        yield return null;

        Assert.Greater(controller.TurnSpeedDegrees, 0f);
    }

    [UnityTest]
    public IEnumerator Update_RunsEachFrame_WithoutThrowing()
    {
        var go = Spawn("LocalPlayer");
        go.AddComponent<LocalPlayerController>();

        yield return null;
        yield return null;
        yield return null;

        Assert.Pass();
    }
}
