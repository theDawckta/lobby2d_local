using System.Collections;
using Game.Environment;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FullScreenBackgroundPlayTests
{
    private GameObject _cameraGo;
    private GameObject _backgroundGo;
    private Camera _camera;
    private FullScreenBackground _background;

    [SetUp]
    public void SetUp()
    {
        _cameraGo = new GameObject("TestCamera");
        _camera = _cameraGo.AddComponent<Camera>();
        _camera.fieldOfView = 60f;
        _camera.nearClipPlane = 0.3f;
        _camera.farClipPlane = 1000f;
        _cameraGo.tag = "MainCamera";

        _backgroundGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.DestroyImmediate(_backgroundGo.GetComponent<MeshCollider>());
        _background = _backgroundGo.AddComponent<FullScreenBackground>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_backgroundGo != null) Object.Destroy(_backgroundGo);
        if (_cameraGo != null) Object.Destroy(_cameraGo);
    }

    [UnityTest]
    public IEnumerator FullScreenBackground_ParentsToMainCamera()
    {
        yield return null;
        Assert.AreEqual(_camera.transform, _backgroundGo.transform.parent);
    }

    [UnityTest]
    public IEnumerator FullScreenBackground_ScalesToFillFrustumAtDistance()
    {
        yield return null;

        float expectedHeight = 2f * _background.Distance *
            Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * _background.MarginScale;
        float expectedWidth = expectedHeight * _camera.aspect;

        Assert.AreEqual(expectedHeight, _backgroundGo.transform.localScale.y, 0.01f);
        Assert.AreEqual(expectedWidth, _backgroundGo.transform.localScale.x, 0.01f);
    }

    [UnityTest]
    public IEnumerator FullScreenBackground_IsWithinCameraFrustum()
    {
        yield return null;

        var renderer = _backgroundGo.GetComponent<MeshRenderer>();
        var planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        Assert.IsTrue(GeometryUtility.TestPlanesAABB(planes, renderer.bounds));
    }

    [UnityTest]
    public IEnumerator FullScreenBackground_PositionedFarEnoughForGameplayToRenderInFront()
    {
        yield return null;

        // Anything gameplay/floor places near the camera's usual focus distance must end up
        // closer than the background so it wins normal depth testing.
        float distanceFromCamera = Vector3.Distance(_camera.transform.position, _backgroundGo.transform.position);
        Assert.Greater(distanceFromCamera, 10f);
    }
}
