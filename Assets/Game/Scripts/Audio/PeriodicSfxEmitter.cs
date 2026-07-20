using UnityEngine;

namespace Game.Audio
{
    // Generic reusable component for a looping visual effect (electrical sparks, drifting pollen,
    // etc.) that has no discrete script-level "trigger" of its own -- it just periodically emits an
    // ambient SFX via AudioManager at a randomized interval. Attach alongside an existing prop's
    // renderer/Animator; this component adds no visuals.
    // CORESYSTEMS_CANDIDATE: fully generic (no game-specific dependency) -- worth promoting if a
    // second game needs the same "periodic ambient SFX for a looping effect" behaviour.
    public class PeriodicSfxEmitter : MonoBehaviour
    {
        [SerializeField] private string sfxName = "";
        [SerializeField] private float minIntervalSeconds = 3f;
        [SerializeField] private float maxIntervalSeconds = 8f;
        [SerializeField] private bool playOnStart = false;

        private float _timer;

        public string SfxName
        {
            get => sfxName;
            set => sfxName = value;
        }

        public float MinIntervalSeconds
        {
            get => minIntervalSeconds;
            set => minIntervalSeconds = value;
        }

        public float MaxIntervalSeconds
        {
            get => maxIntervalSeconds;
            set => maxIntervalSeconds = value;
        }

        public bool PlayOnStart
        {
            get => playOnStart;
            set => playOnStart = value;
        }

        private void Start()
        {
            _timer = playOnStart ? 0f : PickInterval();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            if (!string.IsNullOrEmpty(sfxName)) AudioManager.Instance?.PlaySFX(sfxName);
            _timer = PickInterval();
        }

        private float PickInterval() => Random.Range(minIntervalSeconds, maxIntervalSeconds);
    }
}
