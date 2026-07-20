using System.Collections;
using UnityEngine;
using OneTimeGames.CoreSystems;
using OneTimeGames.CoreSystems.PersistentWorld;

namespace Game.World
{
    // Wires CoreSystems' WorldConnection to this game's runtime config + player identity: the
    // worldWsUrl comes from ConfigService (Rule C), the join token/character name come from
    // FactoryAuth once it resolves, and worldId is this game's own fixed value (must match the
    // "world.worldId" declared in game.json, which the operator uses to drop the matching
    // Tools/world-server/definitions/<worldId>.json on the serving box).
    [RequireComponent(typeof(WorldConnection))]
    public class WorldConnectController : MonoBehaviour
    {
        [Tooltip("Must match the \"world.worldId\" declared in game.json.")]
        [SerializeField] private string worldId = "lobby2d-local";

        [Tooltip("Resolved automatically from the scene if left unassigned.")]
        [SerializeField] private FactoryAuth auth;

        public WorldConnection Connection { get; private set; }
        public string WorldId => worldId;

        public FactoryAuth Auth
        {
            get => auth;
            set => auth = value;
        }

        private void Awake()
        {
            Connection = GetComponent<WorldConnection>();
            if (auth == null) auth = FindFirstObjectByType<FactoryAuth>();
        }

        private IEnumerator Start()
        {
            if (ConfigService.Instance != null)
                yield return ConfigService.Instance.EnsureLoaded();

            if (auth == null) yield break;

            if (auth.IsResolved) ConnectToWorld(auth);
            else auth.OnResolved += ConnectToWorld;
        }

        private void ConnectToWorld(FactoryAuth resolvedAuth)
        {
            auth.OnResolved -= ConnectToWorld;
            Connection.CharacterName = resolvedAuth.CharacterName;

            // Rule C / "game must work without a backend": if no config.json was served (e.g. this
            // Editor/test environment), silently skip connecting rather than joining a blank URL.
            var wsUrl = ConfigService.Instance != null ? ConfigService.Instance.Get("worldWsUrl") : "";
            if (string.IsNullOrEmpty(wsUrl)) return;

            Connection.Connect(wsUrl, worldId, resolvedAuth.Token);
        }
    }
}
