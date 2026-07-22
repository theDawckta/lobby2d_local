using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OneTimeGames.CoreSystems;

namespace Game.Wildlife
{
    // Spawns 2D BILLBOARD versions of the wildlife that wander the lobby ALONGSIDE the 3D GLB
    // creatures. Each is a code-built GameObject with a WildlifeAgent (wander/flee, reused verbatim)
    // + a CoreSystems SpriteCharacterAnimator (loads the creature's 8-direction sheets from the
    // character host and animates by movement -- the 2D counterpart to the GLB path). Uses the
    // existing WildlifeManager for the wander area + player list, so no extra config is needed.
    public class Wildlife2DSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class Entry2D
        {
            [Tooltip("Character name; sheets load from characters-static/<name>/spritesheets/...")]
            public string characterName = "Deer";
            public string moveAnimation = "walk";
            public string idleAnimation = "idle";
            public int count = 1;
            [Tooltip("If > 0 this 2D creature FLIES at this world height (e.g. the bird), matching " +
                     "its 3D counterpart, instead of wandering on the floor.")]
            public float flyHeight = 0f;
        }

        [Tooltip("If left empty, defaults to one 2D Deer + one 2D Rabbit.")]
        [SerializeField] private List<Entry2D> entries = new();

        [Tooltip("World-space scale for the billboards. Sheets are 1 unit tall; the scene is built " +
                 "larger, so scale them up to match the 3D creatures.")]
        [SerializeField] private float avatarScale = 5f;

        [Tooltip("Sprite host used in the Editor when no config.json is served (public sprite PNGs only).")]
        [SerializeField] private string editorCharactersBaseUrl = "https://factory.tehfaktoree.com";

        private IEnumerator Start()
        {
            // Default set: a 2D deer + rabbit + bird alongside the 3D ones. The bird now renders a
            // usable sheet (its 2.1 single-image mesh has real spread wings, unlike the old blob),
            // so it gets a flying 2D billboard matching the 3D bird.
            if (entries == null || entries.Count == 0)
            {
                entries = new List<Entry2D>
                {
                    new Entry2D { characterName = "Deer" },
                    new Entry2D { characterName = "Rabbit" },
                    new Entry2D { characterName = "Bird", flyHeight = 4f },
                };
            }

            var manager = FindFirstObjectByType<WildlifeManager>();
            var areaCenter = manager != null ? manager.AreaCenter : Vector3.zero;
            var areaSize = manager != null ? manager.AreaSize : new Vector3(20f, 0f, 20f);
            IReadOnlyList<Transform> players = manager != null ? manager.Players : new List<Transform>();

            string baseUrl = "";
            if (ConfigService.Instance != null)
            {
                yield return ConfigService.Instance.EnsureLoaded();
                baseUrl = ConfigService.Instance.Get("charactersBaseUrl");
            }
            if (Application.isEditor && string.IsNullOrEmpty(baseUrl)) baseUrl = editorCharactersBaseUrl;

            foreach (var e in entries)
                for (int i = 0; i < Mathf.Max(1, e.count); i++)
                    SpawnOne(e, baseUrl, areaCenter, areaSize, players);
        }

        private void SpawnOne(Entry2D e, string baseUrl, Vector3 areaCenter, Vector3 areaSize,
                              IReadOnlyList<Transform> players)
        {
            var go = new GameObject($"Wildlife2D_{e.characterName}");
            go.transform.SetParent(transform, false);
            go.transform.position = WildlifeMovement.PickWanderTarget(areaCenter, areaSize);
            go.transform.localScale = Vector3.one * avatarScale;

            // Renderer: loads the creature's sheets + animates by this transform's movement.
            var sca = go.AddComponent<SpriteCharacterAnimator>();
            sca.charactersBaseUrl = baseUrl;
            sca.characterName = e.characterName;
            sca.moveAnimation = e.moveAnimation;
            sca.idleAnimation = e.idleAnimation;
            sca.Play();

            // Movement: the same wander/flee agent the 3D creatures use (GlbCharacterAnimator is
            // optional in WildlifeAgent, so it works fine driving a pure sprite).
            var agent = go.AddComponent<WildlifeAgent>();
            agent.FlyHeight = e.flyHeight;   // must be set before Initialize (it lifts flyers)
            agent.Initialize(areaCenter, areaSize, players);
        }
    }
}
