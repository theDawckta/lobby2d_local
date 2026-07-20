using NUnit.Framework;
using UnityEngine;
using Game.Player;

public class LocalPlayerMovementTests
{
    [Test]
    public void ComputeKeyboardAxis_NoKeysPressed_ReturnsZero()
    {
        var axis = LocalPlayerMovement.ComputeKeyboardAxis(false, false, false, false);
        Assert.AreEqual(Vector2.zero, axis);
    }

    [Test]
    public void ComputeKeyboardAxis_Forward_ReturnsPositiveY()
    {
        var axis = LocalPlayerMovement.ComputeKeyboardAxis(true, false, false, false);
        Assert.AreEqual(new Vector2(0f, 1f), axis);
    }

    [Test]
    public void ComputeKeyboardAxis_Right_ReturnsPositiveX()
    {
        var axis = LocalPlayerMovement.ComputeKeyboardAxis(false, false, false, true);
        Assert.AreEqual(new Vector2(1f, 0f), axis);
    }

    [Test]
    public void ComputeKeyboardAxis_OpposingKeys_Cancel()
    {
        var axis = LocalPlayerMovement.ComputeKeyboardAxis(true, true, true, true);
        Assert.AreEqual(Vector2.zero, axis);
    }

    [Test]
    public void ComputeKeyboardAxis_Diagonal_IsNormalized()
    {
        var axis = LocalPlayerMovement.ComputeKeyboardAxis(true, false, false, true);
        Assert.AreEqual(1f, axis.magnitude, 0.0001f);
    }

    [Test]
    public void CombineInput_AddsBothSources()
    {
        var combined = LocalPlayerMovement.CombineInput(new Vector2(0.5f, 0f), new Vector2(0f, 0.5f));
        Assert.AreEqual(new Vector2(0.5f, 0.5f), combined);
    }

    [Test]
    public void CombineInput_ClampsMagnitudeToOne()
    {
        var combined = LocalPlayerMovement.CombineInput(new Vector2(1f, 0f), new Vector2(1f, 0f));
        Assert.AreEqual(1f, combined.magnitude, 0.0001f);
        Assert.AreEqual(0f, combined.y, 0.0001f);
    }

    [Test]
    public void CombineInput_ZeroInputs_ReturnsZero()
    {
        var combined = LocalPlayerMovement.CombineInput(Vector2.zero, Vector2.zero);
        Assert.AreEqual(Vector2.zero, combined);
    }

    [Test]
    public void ComputeVelocity_ScalesInputByMoveSpeed()
    {
        var velocity = LocalPlayerMovement.ComputeVelocity(new Vector2(1f, 0f), 5f);
        Assert.AreEqual(new Vector3(5f, 0f, 0f), velocity);
    }

    [Test]
    public void ComputeVelocity_KeepsYAtZero()
    {
        var velocity = LocalPlayerMovement.ComputeVelocity(new Vector2(0.3f, 0.7f), 2f);
        Assert.AreEqual(0f, velocity.y);
    }

    [Test]
    public void ComputeNextPosition_AddsVelocityTimesDeltaTime()
    {
        var next = LocalPlayerMovement.ComputeNextPosition(new Vector3(1f, 0f, 1f), new Vector3(2f, 0f, 0f), 0.5f);
        Assert.AreEqual(new Vector3(2f, 0f, 1f), next);
    }

    [Test]
    public void ComputeYaw_NoInput_HoldsCurrentYaw()
    {
        var yaw = LocalPlayerMovement.ComputeYaw(Vector2.zero, 137f);
        Assert.AreEqual(137f, yaw);
    }

    [Test]
    public void ComputeYaw_MovingForward_FacesZeroDegrees()
    {
        var yaw = LocalPlayerMovement.ComputeYaw(new Vector2(0f, 1f), 0f);
        Assert.AreEqual(0f, yaw, 0.01f);
    }

    [Test]
    public void ComputeYaw_MovingRight_FacesNinetyDegrees()
    {
        var yaw = LocalPlayerMovement.ComputeYaw(new Vector2(1f, 0f), 0f);
        Assert.AreEqual(90f, yaw, 0.01f);
    }

    [Test]
    public void ComputeYaw_MovingBackward_FacesOneEightyDegrees()
    {
        var yaw = LocalPlayerMovement.ComputeYaw(new Vector2(0f, -1f), 0f);
        Assert.AreEqual(180f, Mathf.Abs(yaw), 0.01f);
    }
}
