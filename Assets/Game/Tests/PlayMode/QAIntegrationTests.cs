using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using OneTimeGames.CoreSystems;
using Game.Audio;
using Game.Chat;
using Game.Emotes;
using Game.Environment;
using Game.Player;
using Game.UI;
using Game.Wildlife;
using Game.World;

// End-to-end player-journey coverage against the REAL, live-loaded Main.unity scene (registered in
// Build Settings, loaded here via SceneManager -- never UnityEditor APIs, which are forbidden in
// PlayMode tests). Individual components already have their own isolated PlayMode tests; this suite
// exists to catch defects that only show up when the whole assembled scene runs together -- exactly
// the class of bug QA found on #38: a missing ambient effect, and a missing AudioListener, neither
// of which any single component's own test could have caught.
//
// Recreated for #59 after the original file (built during the QA pass on #38) was lost before it
// could be committed -- see #59's process notes. Step numbers follow #38's "Player Journey" list.
public class QAIntegrationTests
{
    private const string MainSceneName = "Main";

    // Loads the real Main scene fresh for every test (Single mode replaces whatever the test
    // runner already had loaded) and waits for the scene's own FactoryAuth to settle (resolved or
    // failed) so every Start() coroutine chained off it has had a chance to run. No config.json is
    // served in this test environment, so FactoryAuth always fails fast with "charactersBaseUrl not
    // set" -- see AuthConnectControllerPlayTests for the same pattern in isolation.
    private static IEnumerator LoadMainScene()
    {
        var op = SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Single);
        while (op != null && !op.isDone) yield return null;
        yield return null;

        var auth = Object.FindAnyObjectByType<FactoryAuth>();
        if (auth != null)
        {
            var settled = auth.IsResolved;
            void MarkSettled(FactoryAuth _) => settled = true;
            void MarkSettledOnFail(string _) => settled = true;
            auth.OnResolved += MarkSettled;
            auth.OnFailed += MarkSettledOnFail;

            float elapsed = 0f;
            while (!settled && elapsed < 10f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            auth.OnResolved -= MarkSettled;
            auth.OnFailed -= MarkSettledOnFail;
        }

        yield return null;
    }

    // Each test loads Main.unity Single mode, which replaces whatever scene the runner had loaded
    // -- but nothing else ever unloads it afterward. Left alone, the last test's FactoryAuth/
    // WorldPresenceController/etc. would leak into every fixture that runs later in the same batch
    // domain and assumes a bare scene (e.g. "...NoFactoryAuthExistsAnywhere"). Explicitly clear the
    // active scene's roots after every test so this suite leaves no cross-fixture residue.
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        AudioListener.volume = 1f;
        var activeScene = SceneManager.GetActiveScene();
        foreach (var root in activeScene.GetRootGameObjects())
        {
            Object.Destroy(root);
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator Step1_StartScreen_IsVisibleOnLaunch()
    {
        yield return LoadMainScene();

        var startScreen = Object.FindAnyObjectByType<StartScreen>();
        Assert.IsNotNull(startScreen, "Main scene must contain a StartScreen.");
        Assert.IsTrue(startScreen.IsVisible, "Start screen should be visible when the player launches the game.");
    }

    [UnityTest]
    public IEnumerator Step2_JoinLobby_ShowsGameScreenAndHidesStartScreen()
    {
        yield return LoadMainScene();

        var startScreen = Object.FindAnyObjectByType<StartScreen>();
        var gameScreen = Object.FindAnyObjectByType<GameScreen>();
        Assert.IsNotNull(startScreen);
        Assert.IsNotNull(gameScreen);

        startScreen.RequestJoinLobby();
        yield return null;

        Assert.IsFalse(startScreen.IsVisible, "Start screen should hide after Join Lobby is tapped.");
        Assert.IsTrue(gameScreen.IsVisible, "Game screen should be visible after Join Lobby is tapped.");
    }

    [UnityTest]
    public IEnumerator Step3_PlayerMovement_UpdatesPositionAndYaw()
    {
        yield return LoadMainScene();

        var player = Object.FindAnyObjectByType<LocalPlayerController>();
        Assert.IsNotNull(player, "Main scene must contain a LocalPlayerController.");

        var startPosition = player.transform.position;
        player.Move(new Vector2(0f, 1f), 0.5f);
        yield return null;

        Assert.AreNotEqual(startPosition, player.transform.position,
            "Moving the player should change its position.");
    }

    [UnityTest]
    public IEnumerator Step4_MicToggle_ShowsThoughtBubble()
    {
        yield return LoadMainScene();

        var speechChat = Object.FindAnyObjectByType<SpeechChatController>();
        var thoughtBubble = Object.FindAnyObjectByType<ThoughtBubble>();
        Assert.IsNotNull(speechChat, "Main scene must contain a SpeechChatController.");
        Assert.IsNotNull(thoughtBubble, "Main scene must contain a ThoughtBubble for the local player.");

        speechChat.SetMicOn(true);
        var keyboardSource = speechChat.GetComponent<KeyboardSpeechSource>();
        Assert.IsNotNull(keyboardSource,
            "The Editor keyboard speech fallback should be attached once the mic is toggled on.");

        keyboardSource.AppendText("hello lobby");
        yield return null;
        yield return null;

        Assert.IsTrue(thoughtBubble.IsShowing, "Thought bubble should appear once speech is transcribed.");
    }

    [UnityTest]
    public IEnumerator Step5_SubmitMessage_RoutesThroughToWorldPresenceSendChat()
    {
        yield return LoadMainScene();

        var speechChat = Object.FindAnyObjectByType<SpeechChatController>();
        var integration = Object.FindAnyObjectByType<SpeechChatIntegrationController>();
        var presenceController = Object.FindAnyObjectByType<WorldPresenceController>();
        Assert.IsNotNull(speechChat);
        Assert.IsNotNull(integration);
        Assert.AreSame(presenceController, integration.Presence,
            "SpeechChatIntegrationController must be wired to the scene's own WorldPresenceController.");

        speechChat.SetMicOn(true);
        var keyboardSource = speechChat.GetComponent<KeyboardSpeechSource>();
        keyboardSource.AppendText("hello everyone");
        yield return null;

        // No live WorldConnection exists in this test environment, so the actual network send is
        // already covered by CoreSystems' own WorldPresence tests -- this proves the real scene's
        // SpeechChatController -> SpeechChatIntegrationController -> WorldPresenceController.SendChat
        // chain is wired end to end with no exception.
        Assert.DoesNotThrow(() => speechChat.SubmitCurrent());
    }

    [UnityTest]
    public IEnumerator Step6_InteractWithCrate_OpensAndStaysOpen()
    {
        yield return LoadMainScene();

        var crate = Object.FindAnyObjectByType<SupplyCrateInteractable>();
        Assert.IsNotNull(crate, "Main scene must contain a SupplyCrateInteractable.");
        var animator = crate.GetComponent<OneShotPropAnimator>();
        Assert.IsNotNull(animator);

        animator.Load();
        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.IsTrue(animator.IsLoaded, "SupplyCrate's GLB must load within 30 seconds.");

        Assert.IsFalse(crate.IsOpen, "Crate must start closed.");

        // Simulates the authoritative delta a real interaction would trigger via NetworkedEntity --
        // exercised directly since no live WorldConnection exists in this test environment (mirrors
        // SupplyCrateInteractablePlayTests' own testing seam).
        crate.HandleToggleChanged(true);
        yield return null;

        Assert.IsTrue(crate.IsOpen, "Crate must be open after the authoritative toggle arrives.");
        Assert.IsTrue(animator.IsPlaying, "Crate's open animation must start playing.");
    }

    [UnityTest]
    public IEnumerator Step6_InteractWithCrate_DoesNotReopenOrRecloseOnRepeatedToggle()
    {
        yield return LoadMainScene();

        var crate = Object.FindAnyObjectByType<SupplyCrateInteractable>();
        var animator = crate.GetComponent<OneShotPropAnimator>();

        animator.Load();
        var deadline = Time.realtimeSinceStartup + 30f;
        while (!animator.IsLoaded && Time.realtimeSinceStartup < deadline) yield return null;

        crate.HandleToggleChanged(true);
        yield return null;
        Assert.IsTrue(crate.IsOpen);

        crate.HandleToggleChanged(true);
        yield return null;

        Assert.IsTrue(crate.IsOpen, "Crate must stay open, never reset by a redundant authoritative toggle.");
    }

    [UnityTest]
    public IEnumerator Step7_MuteToggle_MutesAudioListenerVolume()
    {
        yield return LoadMainScene();

        var gameScreen = Object.FindAnyObjectByType<GameScreen>();
        Assert.IsNotNull(gameScreen, "Main scene must contain a GameScreen.");

        gameScreen.ToggleMute();
        yield return null;

        Assert.IsTrue(gameScreen.IsMuted);
        Assert.AreEqual(0f, AudioListener.volume);
    }

    [UnityTest]
    public IEnumerator Step7_MuteToggle_UnmuteRestoresAudioListenerVolume()
    {
        yield return LoadMainScene();

        var gameScreen = Object.FindAnyObjectByType<GameScreen>();

        gameScreen.ToggleMute();
        gameScreen.ToggleMute();
        yield return null;

        Assert.IsFalse(gameScreen.IsMuted);
        Assert.AreEqual(1f, AudioListener.volume);
    }

    [UnityTest]
    public IEnumerator Step8_EmoteTrigger_PlaysEmoteAndFiresLocalEcho()
    {
        yield return LoadMainScene();

        var emoteSystem = Object.FindAnyObjectByType<EmoteSystem>();
        var presenceController = Object.FindAnyObjectByType<WorldPresenceController>();
        Assert.IsNotNull(emoteSystem, "Main scene must contain an EmoteSystem.");
        Assert.IsNotNull(presenceController);

        // No backend serves the /emotes endpoint in this test environment -- apply a ready recorded
        // emote directly via the same public testing seam EmoteSystem itself exposes, matching the
        // acceptance criterion "the player has a recorded emote ready to play."
        emoteSystem.ApplyEmoteListResponse("[{\"name\":\"wave\",\"ready\":true}]");
        Assert.IsTrue(emoteSystem.HasRecordedEmotes);

        string localEcho = null;
        void HandleLocalEmote(string name) => localEcho = name;
        presenceController.OnLocalEmoteRequested += HandleLocalEmote;

        string playedSfx = null;
        void HandleSfx(string name) => playedSfx = name;
        AudioManager.Instance.OnSfxPlayed += HandleSfx;

        emoteSystem.PlayEmote();
        yield return null;

        presenceController.OnLocalEmoteRequested -= HandleLocalEmote;
        AudioManager.Instance.OnSfxPlayed -= HandleSfx;

        Assert.AreEqual("wave", localEcho,
            "The local player's own emote must fire via WorldPresenceController's local echo.");
        Assert.AreEqual("EmotePlay", playedSfx);
    }

    [UnityTest]
    public IEnumerator Step9_Wildlife_WandersAmbientlyWithoutFleeingFromPlayer()
    {
        yield return LoadMainScene();

        var manager = Object.FindAnyObjectByType<WildlifeManager>();
        Assert.IsNotNull(manager, "Main scene must contain a WildlifeManager.");

        var deadline = Time.realtimeSinceStartup + 5f;
        while (manager.SpawnedWildlife.Count == 0 && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.Greater(manager.SpawnedWildlife.Count, 0, "WildlifeManager must spawn at least one ambient creature.");

        for (var i = 0; i < 10; i++) yield return null;

        foreach (var agent in manager.SpawnedWildlife)
        {
            Assert.AreEqual(WildlifeState.Wandering, agent.CurrentState,
                "Ambient wildlife must never flee -- see #51/#37, fleeDistance is configured to 0.");
        }
    }

    [UnityTest]
    public IEnumerator Step10_DriftingPollen_IsVisibleInEnvironment()
    {
        yield return LoadMainScene();

        var pollen = GameObject.Find("PollenEffectPrefab");
        Assert.IsNotNull(pollen, "Main scene must contain a drifting pollen effect (journey step 10).");

        var particles = pollen.GetComponentInChildren<ParticleSystem>();
        Assert.IsNotNull(particles, "Pollen effect must have a ParticleSystem to render drifting motes.");
        Assert.IsTrue(particles.isPlaying, "Pollen effect must be actively playing/visible in the environment.");
    }

    [UnityTest]
    public IEnumerator RegressionRootCause1_DriftingPollenEffect_MustExistSomewhereInMainScene()
    {
        yield return LoadMainScene();

        var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        var hasPollen = allTransforms.Any(t => t.name.ToLower().Contains("pollen"));

        Assert.IsTrue(hasPollen,
            "Root Cause 1 regression guard: no GameObject anywhere in the Main scene has 'pollen' in its name.");
    }

    [UnityTest]
    public IEnumerator RegressionRootCause2_ExactlyOneAudioListener_ExistsInMainScene()
    {
        yield return LoadMainScene();

        var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include);

        Assert.AreEqual(1, listeners.Length,
            "Root Cause 2 regression guard: the Main scene must have exactly one AudioListener.");
    }
}
