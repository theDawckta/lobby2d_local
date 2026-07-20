using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Audio;

public class AudioManagerPlayTests
{
    [UnityTest]
    public IEnumerator Instance_IsAutoCreatedBeforeAnyTestRuns()
    {
        yield return null;
        Assert.IsNotNull(AudioManager.Instance);
    }

    [UnityTest]
    public IEnumerator PlaySFX_FiresOnSfxPlayed_ForExistingClip()
    {
        yield return null;

        string played = null;
        void Handler(string name) => played = name;
        AudioManager.Instance.OnSfxPlayed += Handler;

        AudioManager.Instance.PlaySFX("MuteToggleOn");

        AudioManager.Instance.OnSfxPlayed -= Handler;
        Assert.AreEqual("MuteToggleOn", played);
    }

    [UnityTest]
    public IEnumerator PlaySFX_DoesNotThrow_AndDoesNotFireEvent_ForMissingClip()
    {
        yield return null;

        var fired = false;
        void Handler(string name) => fired = true;
        AudioManager.Instance.OnSfxPlayed += Handler;

        Assert.DoesNotThrow(() => AudioManager.Instance.PlaySFX("DefinitelyNotARealClipName"));

        AudioManager.Instance.OnSfxPlayed -= Handler;
        Assert.IsFalse(fired);
    }

    [UnityTest]
    public IEnumerator PlaySFX_CanPlayTheSameClipRepeatedly()
    {
        yield return null;

        var count = 0;
        void Handler(string name) => count++;
        AudioManager.Instance.OnSfxPlayed += Handler;

        AudioManager.Instance.PlaySFX("MuteToggleOff");
        AudioManager.Instance.PlaySFX("MuteToggleOff");

        AudioManager.Instance.OnSfxPlayed -= Handler;
        Assert.AreEqual(2, count);
    }

    [UnityTest]
    public IEnumerator PlayMusic_DoesNotThrow_ForMissingClip()
    {
        yield return null;

        Assert.DoesNotThrow(() => AudioManager.Instance.PlayMusic("DefinitelyNotARealMusicTrack"));
    }

    [UnityTest]
    public IEnumerator SetMusicMuted_UpdatesIsMusicMuted()
    {
        yield return null;

        AudioManager.Instance.SetMusicMuted(true);
        Assert.IsTrue(AudioManager.Instance.IsMusicMuted);

        AudioManager.Instance.SetMusicMuted(false);
        Assert.IsFalse(AudioManager.Instance.IsMusicMuted);
    }

    [UnityTest]
    public IEnumerator ToggleMusicMute_FlipsCurrentState()
    {
        yield return null;

        AudioManager.Instance.SetMusicMuted(false);
        AudioManager.Instance.ToggleMusicMute();
        Assert.IsTrue(AudioManager.Instance.IsMusicMuted);

        AudioManager.Instance.ToggleMusicMute();
        Assert.IsFalse(AudioManager.Instance.IsMusicMuted);
    }

    [UnityTest]
    public IEnumerator StopMusic_ClearsCurrentMusicName()
    {
        yield return null;

        AudioManager.Instance.PlayMusic("DefinitelyNotARealMusicTrack");
        AudioManager.Instance.StopMusic();

        Assert.IsNull(AudioManager.Instance.CurrentMusicName);
    }

    [UnityTest]
    public IEnumerator AddingASecondComponent_DestroysTheDuplicateGameObject()
    {
        var duplicateGo = new GameObject("DuplicateAudioManager");
        duplicateGo.AddComponent<AudioManager>();
        yield return null;

        Assert.IsTrue(duplicateGo == null);
    }
}
