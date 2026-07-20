using System.Collections;
using NUnit.Framework;
using OneTimeGames.CoreSystems;
using UnityEngine;
using UnityEngine.TestTools;

public class ElectricalSparkPlayTests
{
    private GameObject _cameraGo;
    private GameObject _sparkGo;
    private Camera _camera;
    private BillboardSprite _billboard;

    [SetUp]
    public void SetUp()
    {
        _cameraGo = new GameObject("TestCamera");
        _camera = _cameraGo.AddComponent<Camera>();
        _cameraGo.tag = "MainCamera";
        _cameraGo.transform.position = new Vector3(2f, 5f, -3f);
        _cameraGo.transform.rotation = Quaternion.Euler(30f, 45f, 0f);

        _sparkGo = new GameObject("ElectricalSparkPrefab");
        _sparkGo.AddComponent<SpriteRenderer>();
        _sparkGo.AddComponent<Animator>();
        _billboard = _sparkGo.AddComponent<BillboardSprite>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_sparkGo != null) Object.Destroy(_sparkGo);
        if (_cameraGo != null) Object.Destroy(_cameraGo);
    }

    [UnityTest]
    public IEnumerator ElectricalSparkPrefab_BillboardsToFaceMainCamera()
    {
        yield return null;

        Vector3 expectedForward = _camera.transform.forward;
        expectedForward.y = 0f;
        Quaternion expectedRotation = Quaternion.LookRotation(expectedForward);

        Assert.AreEqual(expectedRotation.eulerAngles.y, _sparkGo.transform.rotation.eulerAngles.y, 0.1f);
    }

    [UnityTest]
    public IEnumerator ElectricalSparkPrefab_HasRendererAndAnimatorPresentDuringPlay()
    {
        yield return null;

        Assert.IsNotNull(_sparkGo.GetComponent<SpriteRenderer>());
        Assert.IsNotNull(_sparkGo.GetComponent<Animator>());
        Assert.IsNotNull(_billboard);
    }
}
