using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    // Singleton audio playback hub: loads AudioClip references by name from Resources/Audio/Music
    // and Resources/Audio/SFX (Assets/Game/Resources/Audio/...) and exposes PlayMusic/PlaySFX for
    // the rest of the game to call. Auto-create + duplicate-destroy lifecycle mirrors ConfigService
    // (the established cross-scene singleton pattern in this repo) exactly, so it behaves
    // identically under the same PlayMode test-domain lifecycle.
    public class AudioManager : MonoBehaviour
    {
        private const string MusicResourcePrefix = "Audio/Music/";
        private const string SfxResourcePrefix = "Audio/SFX/";

        public static AudioManager Instance { get; private set; }

        [SerializeField] private string startupMusicName = "AmbientLobby";
        [SerializeField] private bool playMusicOnStart = true;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 1f;

        private readonly Dictionary<string, AudioClip> _clipCache = new();
        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        public bool IsMusicMuted { get; private set; }
        public string CurrentMusicName { get; private set; }

        // Fired once a named clip is actually found and handed to an AudioSource -- lets callers
        // (and tests, which can't rely on a real audio device in batch mode) observe playback
        // without depending on AudioSource.isPlaying.
        public event Action<string> OnMusicPlayed;
        public event Action<string> OnSfxPlayed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance != null) return;
            var go = new GameObject("[AudioManager]");
            go.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = musicVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
        }

        private void Start()
        {
            if (playMusicOnStart) PlayMusic(startupMusicName);
        }

        // Loads (if needed) and loops the named clip from Resources/Audio/Music. Missing clips warn
        // and no-op rather than throwing -- e.g. "AmbientLobby" has no generated asset yet, and the
        // game must keep working with no music rather than fail.
        public void PlayMusic(string clipName)
        {
            var clip = LoadClip(clipName, MusicResourcePrefix);
            if (clip == null)
            {
                Debug.LogWarning($"AudioManager: music clip '{clipName}' not found under Resources/{MusicResourcePrefix}");
                return;
            }

            CurrentMusicName = clipName;
            _musicSource.clip = clip;
            _musicSource.volume = IsMusicMuted ? 0f : musicVolume;
            _musicSource.Play();
            OnMusicPlayed?.Invoke(clipName);
        }

        public void StopMusic()
        {
            _musicSource.Stop();
            CurrentMusicName = null;
        }

        // Loads (if needed) and fires-and-forgets the named clip from Resources/Audio/SFX via
        // PlayOneShot, so overlapping SFX never cut each other off. Missing clips warn and no-op.
        public void PlaySFX(string clipName, float volumeScale = 1f)
        {
            var clip = LoadClip(clipName, SfxResourcePrefix);
            if (clip == null)
            {
                Debug.LogWarning($"AudioManager: SFX clip '{clipName}' not found under Resources/{SfxResourcePrefix}");
                return;
            }

            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
            OnSfxPlayed?.Invoke(clipName);
        }

        public void SetMusicMuted(bool muted)
        {
            IsMusicMuted = muted;
            _musicSource.volume = muted ? 0f : musicVolume;
        }

        public void ToggleMusicMute() => SetMusicMuted(!IsMusicMuted);

        private AudioClip LoadClip(string clipName, string prefix)
        {
            if (string.IsNullOrEmpty(clipName)) return null;

            var key = prefix + clipName;
            if (_clipCache.TryGetValue(key, out var cached)) return cached;

            var clip = Resources.Load<AudioClip>(key);
            if (clip != null) _clipCache[key] = clip;
            return clip;
        }
    }
}
